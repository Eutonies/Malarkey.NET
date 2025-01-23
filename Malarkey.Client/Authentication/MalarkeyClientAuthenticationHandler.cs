using Malarkey.Abstractions;
using Malarkey.Abstractions.API.Profile;
using Malarkey.Abstractions.API.Profile.Requests;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Token;
using Malarkey.Abstractions.Util;
using Malarkey.Client.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace Malarkey.Client.Authentication;
internal class MalarkeyClientAuthenticationHandler : AuthenticationHandler<MalarkeyClientAuthenticationSchemeOptions>, IMalarkeyClientAuthenticatedCallback
{
    private SecurityKey? _malarkeySigningCertificatePublicKey;
    private readonly string _clientCertificateString;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MalarkeyClientConfiguration _conf;
    private readonly MalarkeyAuthenticationRequestContinuationCache _cache;

    public MalarkeyClientAuthenticationHandler(
        IOptionsMonitor<MalarkeyClientAuthenticationSchemeOptions> options, 
        ILoggerFactory logger, 
        UrlEncoder encoder, 
        IHttpClientFactory httpClientFactory,
        IOptions<MalarkeyClientConfiguration> conf,
        MalarkeyAuthenticationRequestContinuationCache cache
        ) : base(options, logger, encoder)
    {
        _conf = conf.Value;
        _clientCertificateString = _conf.ClientCertificateString;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
    }

    private static readonly Regex _idTokenIndexRegex = new Regex($"{MalarkeyConstants.Authentication.IdentityCookieBaseName}\\.([0-9]+)");

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var authAttribute = ExtractAttribute();
        if (authAttribute == null)
            return;
        var state = await _cache.Cache(Request);
        var idProvider = authAttribute.IdentityProvider;
        var scopes = authAttribute.Scopes?.Split(" ");
        var forwardUrl = BuildRequestString(state, idProvider, scopes);
        Response.Redirect(forwardUrl);
    }

    private string BuildRequestString(string state, MalarkeyIdentityProvider? provider, string[]? scopes)
    {
        var returnee = new StringBuilder($"{_conf.MalarkeyServerBaseAddress}{MalarkeyConstants.Authentication.ServerAuthenticationPath}");
        returnee.Append($"&{MalarkeyConstants.AuthenticationRequestQueryParameters.ForwarderName}={_conf.FullClientServerUrl.UrlEncoded()}");
        returnee.Append($"?{MalarkeyConstants.AuthenticationRequestQueryParameters.ForwarderStateName}={state.UrlEncoded()}");
        if(provider != null)
        {
            returnee.Append($"?{MalarkeyConstants.AuthenticationRequestQueryParameters.IdProviderName}={provider.ToString()}");
        }
        if (scopes != null)
        {
            returnee.Append($"?{MalarkeyConstants.AuthenticationRequestQueryParameters.ScopesName}={scopes.MakeString(" ").UrlEncoded()}");
        }
        return returnee.ToString();
    }


    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authAttribute = ExtractAttribute();
        if (authAttribute == null)
            return AuthenticateResult.NoResult();
        var profileCookie = Request.Cookies
            .Where(_ => _.Key == MalarkeyConstants.Authentication.ProfileCookieName)
            .Select(_ => _.Value.ToString())
            .FirstOrDefault();
        if (profileCookie == null)
            return AuthenticateResult.Fail("No profile cookie found");
        var profileToken = MalarkeyToken.ParseProfileToken(profileCookie);
        if (profileToken == null)
            return AuthenticateResult.Fail("Profile cookie could not be parsed as profile token");
        if (profileToken.ValidUntil < DateTime.Now)
            return AuthenticateResult.Fail("Profile cookie has expired");
        var profileTokenVerifies = await VerifySignature(profileCookie);
        if(!profileTokenVerifies)
            return AuthenticateResult.Fail("Profile cookie signature does not verify");
        var identityCookies = Request.Cookies
            .Select(cook => (Matcher: _idTokenIndexRegex.Match(cook.Key), Value: cook.Value.ToString()))
            .Where(_ => _.Matcher.Success && _.Matcher.Groups.Count > 1)
            .Select(_ => (Index: int.Parse(_.Matcher.Groups[1].Value), _.Value))
            .OrderBy(_ => _.Index)
            .Select(_ => _.Value)
            .ToList();
        identityCookies = (await FilterVerifiableTokens(identityCookies)).ToList();
        var identityTokens = identityCookies
            .Select(cook => MalarkeyToken.ParseIdentityToken(cook))
            .Where(_ => _ != null && _.ValidUntil > DateTime.Now)
            .Select(_ => _!)
            .ToList();
        var identities = identityTokens
            .Select(_ => _.Identity)
            .ToList();
        if (authAttribute.IdentityProvider != null)
        {
            if(!identities.Any(_ => _.IdentityProvider == authAttribute.IdentityProvider))
               return AuthenticateResult.Fail($"No valid identity token found for ID Provider: {authAttribute.IdentityProvider}");
            if(authAttribute.Scopes != null)
            {
                var requestedScopes = authAttribute.Scopes
                    .Split(" ")
                    .Select(s => s.Trim().ToLower())
                    .ToHashSet();
                if(requestedScopes.Any())
                {
                    var relIdent = identities.First(_ => _.IdentityProvider == authAttribute.IdentityProvider);
                    var idpToken = relIdent.IdentityProviderTokenToUse;
                    if(idpToken == null)
                        return AuthenticateResult.Fail($"No valid access token found for ID Provider: {authAttribute.IdentityProvider}");
                    if(idpToken.Expires < DateTime.Now)
                    {
                        var refreshed = await RefreshToken(idpToken);
                        if(refreshed == null)
                            return AuthenticateResult.Fail($"Twas not possible to refresh IDP access token for provider: {authAttribute.IdentityProvider}");
                        relIdent = relIdent.WithToken(refreshed);
                        identities = identities
                            .Select(_ => _.IdentityId == relIdent.IdentityId ? relIdent : _)
                            .ToList();
                    }
                    var tokenScopes = idpToken.Scopes
                        .Select(_ => _.ToLower().Trim())
                        .ToHashSet();
                    var missingScopes = requestedScopes
                        .Except(tokenScopes)
                        .ToList();
                    if (missingScopes.Any())
                    {
                        return AuthenticateResult.Fail($"IDP access token for provider: {authAttribute.IdentityProvider} did not contain scopes: {missingScopes.Order().MakeString(" ")}");
                    }
                }
            }
        }
        var principal = profileToken.Profile.ToClaimsPrincipal(identities);
        var ticket = new AuthenticationTicket(principal, MalarkeyConstants.MalarkeyAuthenticationScheme);
        return AuthenticateResult.Success(ticket);
    }


    private MalarkeyAuthenticationAttribute? ExtractAttribute()
    {
        var endpoint = Context.GetEndpoint();
        if (endpoint == null)
            return null;
        var authAttribute = endpoint.Metadata
            .OfType<MalarkeyAuthenticationAttribute>()
            .FirstOrDefault();
        return authAttribute;

    }

    private async Task<IReadOnlyCollection<string>> FilterVerifiableTokens(IEnumerable<string> tokens)
    {
        var tasks = tokens
            .Select(_ => (Token: _, ResultTask: VerifySignature(_)))
            .ToList();
        await Task.WhenAll(tasks.Select(_ => _.ResultTask));
        var returnee = tasks
            .Where(_ => _.ResultTask.Result)
            .Select(_ => _.Token)
            .ToList();
        return returnee;
    }

    private async Task<bool> VerifySignature(string token)
    {
        if (_malarkeySigningCertificatePublicKey == null)
            await LoadMalarkeyCertificate();
        var jwtHandler = new JsonWebTokenHandler();
        var validationResult = await jwtHandler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = MalarkeyConstants.Authentication.TokenIssuer,
            ValidAudience = _clientCertificateString,
            IssuerSigningKey = _malarkeySigningCertificatePublicKey!
        });
        return validationResult.IsValid;
    }


    private async Task<IdentityProviderToken?> RefreshToken(IdentityProviderToken token)
    {
        using var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(
            HttpMethod.Post, 
            new Uri($"{_conf.MalarkeyServerBaseAddress}{MalarkeyConstants.API.Paths.Profile.RefreshTokenRelativePath}"));
        var requestContent = new MalarkeyProfileRefreshProviderTokenRequest(token.Token);
        request.Content = JsonContent.Create(requestContent);
        request.Headers.Add(MalarkeyConstants.Authentication.AudienceHeaderName, _clientCertificateString.Base64UrlEncoded());
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseToken = (await response.Content.ReadFromJsonAsync<MalarkeyIdentityProviderTokenDto>())!;
        return responseToken.ToDomain();

    }

    private async Task LoadMalarkeyCertificate()
    {
        using var client = _httpClientFactory.CreateClient();
        var reqUrl = $"{_conf.MalarkeyServerBaseAddress}{MalarkeyConstants.API.Paths.Certificates.SigningCertificateAbsolutePath}";
        var response = (await client.GetAsync(reqUrl)).EnsureSuccessStatusCode();
        var certificateString = await response.Content.ReadAsStringAsync();
        var certBytes = Encoding.UTF8.GetBytes(certificateString);
        var signingCert = X509CertificateLoader.LoadCertificate(certBytes);
        _malarkeySigningCertificatePublicKey = new RsaSecurityKey(signingCert.GetRSAPublicKey());
    }

    public async Task<IResult> HandleCallback(HttpRequest request)
    {
        var profileParam = request.Form
            .Where(_ => _.Key == MalarkeyConstants.AuthenticationSuccessParameters.ProfileTokenName)
            .Select(_ => _.Value.ToString())
            .FirstOrDefault();
        if (profileParam == null)
            return TypedResults.BadRequest("No profile parameter found");
        var profileVerifies = await VerifySignature(profileParam);
        if(!profileVerifies)
            return TypedResults.BadRequest("Profile parameter does not verify");
        var profileToken = MalarkeyToken.ParseProfileToken(profileParam);
        if (profileToken == null) 
            return TypedResults.BadRequest("Profile token could not be parsed: " + profileParam);
        if (profileToken.ValidUntil < DateTime.Now)
            return TypedResults.BadRequest("Profile token has expired");
        request.HttpContext.Response.Cookies.Append(MalarkeyConstants.Authentication.ProfileCookieName, profileParam);

        var identityParam = request.Query
            .Where(_ => _.Key == MalarkeyConstants.AuthenticationSuccessParameters.IdentityTokenName)
            .Select(_ => _.Value.ToString())
            .FirstOrDefault();
        MalarkeyIdentityToken? identityToken = null;
        if(identityParam != null)
        {
            var validIdentityParams = await FilterVerifiableTokens([identityParam]);
            if(validIdentityParams.Any())
                identityToken = MalarkeyToken.ParseIdentityToken(identityParam);
        }
        if(identityToken != null)
            request.HttpContext.Response.Cookies.Append(MalarkeyConstants.Authentication.ProfileCookieName + ".0", identityParam!);

        MalarkeyAuthenticationRequestContinuation? continuation = null;
        var forwardedState = request.Query
            .Where(_ => _.Key == MalarkeyConstants.AuthenticationSuccessParameters.ForwarderStateName)
            .Select(_ => _.Value.ToString())
            .FirstOrDefault();
        if (forwardedState != null)
            continuation = _cache.Pop(forwardedState);
        if(continuation != null)
           return new MalarkeyClientAuthenticationSuccessResult(continuation);
        return TypedResults.Redirect("/");

    }


}
