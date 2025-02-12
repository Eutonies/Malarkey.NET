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
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace Malarkey.Client.Authentication;
internal class MalarkeyClientAuthenticationHandler : AuthenticationHandler<MalarkeyClientAuthenticationSchemeOptions>, IMalarkeyClientAuthenticatedCallback
{
    private RsaSecurityKey? _malarkeySigningCertificatePublicKey;
    private async Task<RsaSecurityKey> MalarkeySigningCertificatePublicKey()
    {
        if (_malarkeySigningCertificatePublicKey != null)
            return _malarkeySigningCertificatePublicKey;
        await LoadMalarkeyCertificate();
        return _malarkeySigningCertificatePublicKey!;
    }
        
    private readonly X509Certificate2 _clientCertificate;
    private readonly string _clientPublicKey;
    private readonly string _clienPublicKeyHashForValidation;


    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MalarkeyClientConfiguration _conf;
    private readonly IMalarkeyCache<string, MalarkeyAuthenticationSession> _cache;
    private readonly ILogger<MalarkeyClientAuthenticationHandler> _logger;
    private readonly LogLevel _logLevel;

    public MalarkeyClientAuthenticationHandler(
        IOptionsMonitor<MalarkeyClientAuthenticationSchemeOptions> options, 
        ILoggerFactory loggerFactory, 
        UrlEncoder encoder, 
        IHttpClientFactory httpClientFactory,
        IOptions<MalarkeyClientConfiguration> conf,
        IMalarkeyCache<string, MalarkeyAuthenticationSession> cache,
        ILogger<MalarkeyClientAuthenticationHandler> logger
        ) : base(options, loggerFactory, encoder)
    {
        _logger = logger;
        _conf = conf.Value;
        _logLevel = _conf.LogLevelToUse;
        _clientCertificate = conf.Value.ClientCertificate;
        _clientPublicKey = _clientCertificate.GetRSAPublicKey()!.ExportRSAPublicKeyPem().CleanCertificate();
        _clienPublicKeyHashForValidation = _clientPublicKey.HashPem();
        _httpClientFactory = httpClientFactory;
        _cache = cache;
    }

    private static readonly Regex _idTokenIndexRegex = new Regex($"{MalarkeyConstants.Authentication.IdentityCookieBaseName}\\.([0-9]+)");

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var authAttribute = ExtractAttribute();
        if (authAttribute == null)
        {
            Log($"No challange to do, since no authorization attribute");
            return;
        }
        var authSession = Request.ResolveSession(_clientCertificate.ExportCertificatePem());
        var state = Guid.NewGuid().ToString();
        await _cache.Cache(state, authSession);
        var idProvider = authAttribute.IdentityProvider;
        var scopes = authAttribute.Scopes?.Split(" ");
        var forwardUrl = await BuildRequestString(state, idProvider, scopes);
        Log($"On challenge, forwarding to: {forwardUrl}");
        Response.Redirect(forwardUrl);
    }

    private async Task<string> BuildRequestString(string state, MalarkeyIdentityProvider? provider, string[]? scopes)
    {
        var returnee = new StringBuilder($"{_conf.MalarkeyServerBaseAddress}{MalarkeyConstants.Authentication.ServerAuthenticationPath}");
        returnee.Append($"?{MalarkeyConstants.AuthenticationRequestQueryParameters.SendToName}={_conf.FullClientServerUrl.UrlEncoded()}");
        var malarkeyKey = await MalarkeySigningCertificatePublicKey();
        var encryptedStateBytes = malarkeyKey.Rsa.Encrypt(UTF8Encoding.UTF8.GetBytes(state), MalarkeyConstants.RSAPadding);
        var encryptedState = Convert.ToBase64String(encryptedStateBytes);
        returnee.Append($"&{MalarkeyConstants.AuthenticationRequestQueryParameters.EncryptedStateName}={encryptedState.UrlEncoded()}");
        if(provider != null)
        {
            returnee.Append($"&{MalarkeyConstants.AuthenticationRequestQueryParameters.IdProviderName}={provider.ToString()}");
        }
        if (scopes != null)
        {
            returnee.Append($"&{MalarkeyConstants.AuthenticationRequestQueryParameters.ScopesName}={scopes.MakeString(" ").UrlEncoded()}");
        }
        returnee.Append($"&{MalarkeyConstants.AuthenticationRequestQueryParameters.ClientPublicKey}={_clientPublicKey.UrlEncoded()}");
        return returnee.ToString();
    }


    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authAttribute = ExtractAttribute();
        if (authAttribute == null)
        {
            Log($"No authorization attribute found, so will not invalidate request");
            return AuthenticateResult.NoResult();
        }
        var profileCookie = Request.Cookies
            .Where(_ => _.Key == MalarkeyConstants.Authentication.ProfileCookieName)
            .Select(_ => _.Value.ToString())
            .FirstOrDefault();
        if (profileCookie == null)
        {
            Log($"No profile cookie found");
            return AuthenticateResult.Fail("No profile cookie found");
        }
        var profileToken = MalarkeyToken.ParseProfileToken(profileCookie);
        if (profileToken == null)
        {
            Log($"Profile cookie could not be parsed as profile token");
            Log($"  profile cookie: {profileCookie}");
            return AuthenticateResult.Fail("Profile cookie could not be parsed as profile token");

        }
        if (profileToken.ValidUntil < DateTime.Now.ToUniversalTime())
        {
            Log($"Profile token expired. Only valid until: {profileToken.ValidUntil}");
            return AuthenticateResult.Fail("Profile cookie has expired");
        }
        var profileTokenVerifies = await VerifySignature(profileCookie);
        if(!profileTokenVerifies)
        {
            Log($"Profile cookie signature does not verify");
            return AuthenticateResult.Fail("Profile cookie signature does not verify");
        }
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
            .Where(_ => _ != null && _.ValidUntil > DateTime.Now.ToUniversalTime())
            .Select(_ => _!)
            .ToList();
        var identities = identityTokens
            .Select(_ => _.Identity)
            .ToList();
        if (authAttribute.IdentityProvider != null)
        {
            if(!identities.Any(_ => _.IdentityProvider == authAttribute.IdentityProvider))
            {
                Log($"No valid identity token found for ID Provider: {authAttribute.IdentityProvider}");
                return AuthenticateResult.Fail($"No valid identity token found for ID Provider: {authAttribute.IdentityProvider}");
            }
            if (authAttribute.Scopes != null)
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
                    {
                        Log($"No valid access token found for ID Provider: {authAttribute.IdentityProvider}");
                        return AuthenticateResult.Fail($"No valid access token found for ID Provider: {authAttribute.IdentityProvider}");
                    }
                    if (idpToken.Expires < DateTime.Now.ToUniversalTime())
                    {
                        var refreshed = await RefreshToken(idpToken);
                        if(refreshed == null)
                        {
                            Log($"Twas not possible to refresh IDP access token for provider: {authAttribute.IdentityProvider}");
                            return AuthenticateResult.Fail($"Twas not possible to refresh IDP access token for provider: {authAttribute.IdentityProvider}");
                        }
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
                        Log($"IDP access token for provider: {authAttribute.IdentityProvider} did not contain scopes: {missingScopes.Order().MakeString(" ")}");
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
        var malarkeyKey = await MalarkeySigningCertificatePublicKey();
        var jwtHandler = new JsonWebTokenHandler();
        Log($"Using client public key for validation: {_clientPublicKey}");
        Log($"  With certificate hash: {_clienPublicKeyHashForValidation}");
        var validationResult = await jwtHandler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = MalarkeyConstants.Authentication.TokenIssuer,
            ValidAudience = _clienPublicKeyHashForValidation,
            IssuerSigningKey = malarkeyKey
        });
        if(!validationResult.IsValid)
        {
            Log("Token validation failed");
            Log(validationResult.Exception.ToString());
        }
        return validationResult.IsValid;
    }


    private async Task<IdentityProviderToken?> RefreshToken(IdentityProviderToken token)
    {
        using var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(
            HttpMethod.Post, 
            new Uri($"{_conf.MalarkeyServerBaseAddress}{MalarkeyConstants.API.Paths.Profile.RefreshTokenRelativePath}"));
        var requestContent = new MalarkeyProfileRefreshProviderTokenRequest(
            IdentityProvider: token.Provider.ToDto(), 
            AccessToken: token.Token,
            ClientCertificate: _clientPublicKey);
        request.Content = JsonContent.Create(requestContent);
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseToken = (await response.Content.ReadFromJsonAsync<MalarkeyIdentityProviderTokenDto>())!;
        return responseToken.ToDomain(token.Provider.ToDto());
    }

    private async Task LoadMalarkeyCertificate()
    {
        using var client = _httpClientFactory.CreateClient();
        var reqUrl = $"{_conf.MalarkeyServerBaseAddress}{MalarkeyConstants.API.Paths.Certificates.SigningCertificateAbsolutePath}";
        Log($"Loading Malarkey certificate from: {reqUrl}");
        var response = (await client.GetAsync(reqUrl)).EnsureSuccessStatusCode();
        var certificateString = await response.Content.ReadAsStringAsync();
        Log($"Loaded Malarkey certificate: {certificateString}");
        certificateString = certificateString.CleanCertificate();
        var bytes = Convert.FromBase64String(certificateString);
        var cert = X509CertificateLoader.LoadCertificate(bytes);
        var rsaPublicKey = cert.GetRSAPublicKey()!;
        _malarkeySigningCertificatePublicKey = new RsaSecurityKey(rsaPublicKey);
    }

    public async Task<IResult> HandleCallback(HttpRequest request)
    {
        var profileTokenString = request.Form
            .Where(_ => _.Key == MalarkeyConstants.AuthenticationSuccessParameters.ProfileTokenName)
            .Select(_ => _.Value.ToString())
            .FirstOrDefault();
        if (profileTokenString == null)
            return TypedResults.BadRequest("No profile parameter found");
        var profileVerifies = await VerifySignature(profileTokenString);
        if(!profileVerifies)
            return TypedResults.BadRequest("Profile parameter does not verify");
        var profileToken = MalarkeyToken.ParseProfileToken(profileTokenString);
        if (profileToken == null) 
            return TypedResults.BadRequest("Profile token could not be parsed: " + profileTokenString);
        if (profileToken.ValidUntil < DateTime.Now.ToUniversalTime())
            return TypedResults.BadRequest("Profile token has expired");

        var identityParams = request.Form
            .Where(_ => _.Key.ToLower().StartsWith(MalarkeyConstants.AuthenticationSuccessParameters.IdentityTokenBaseName.ToLower()))
            .OrderBy(_ => _.Key.ToLower())
            .Select(_ => _.Value.ToString())
            .ToList();
        var identityTokenStrings = await FilterVerifiableTokens(identityParams);

        var state = request.Form
            .Where(_ => _.Key == MalarkeyConstants.AuthenticationSuccessParameters.StateName)
            .Select(_ => _.Value.ToString())
            .FirstOrDefault();
        if(state != null)
        {
            var encryptedStateBytes = Convert.FromBase64String(state);
            var decryptedBytes = _clientCertificate.GetRSAPrivateKey()!.Decrypt(encryptedStateBytes, MalarkeyConstants.RSAPadding);
            var decryptedState = UTF8Encoding.UTF8.GetString(decryptedBytes);
            var authSession = await _cache.Pop(decryptedState);
            if(authSession != null)
            {
                var returnee = new MalarkeyAuthenticationSuccessHttpResult(authSession, profileTokenString, identityTokenStrings, Logger);
                return returnee;
            }
        }
        return TypedResults.Redirect("/");

    }


    private void Log(string message)
    {
        _logger.Log(_logLevel, message);
    }

}
