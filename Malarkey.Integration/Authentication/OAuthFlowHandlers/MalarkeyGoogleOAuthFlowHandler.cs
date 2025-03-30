using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Util;
using Malarkey.Integration.Authentication.Naming;
using Malarkey.Integration.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Malarkey.Abstractions.Token;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;

namespace Malarkey.Integration.Authentication.OAuthFlowHandlers;
public class MalarkeyGoogleOAuthFlowHandler : MalarkeyOAuthFlowHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    public override MalarkeyIdentityProvider HandlerFor => MalarkeyIdentityProvider.Google;


    public MalarkeyGoogleOAuthFlowHandler(
        IOptions<MalarkeyIntegrationConfiguration> intConf,
        IHttpClientFactory httpClientFactory,
        ILogger<MalarkeyGoogleOAuthFlowHandler> logger) : base(intConf, logger)
    {
        _httpClientFactory = httpClientFactory;
    }
    public override string AuthorizationEndpoint => _conf.AuthorizationEndpointTemplate;

    protected override MalarkeyOAuthNamingScheme ProduceNamingScheme() => MalarkeyGoogleOAuthNamingScheme.Init(_conf.NamingSchemeOverwrites);
    protected override MalarkeyIdentityProviderConfiguration ProduceConfiguration() => _intConf.Google!;

    public override IReadOnlyDictionary<string, string> ProduceRequestQueryParameters(MalarkeyAuthenticationSession session, MalarkeyAuthenticationIdpSession idpSession)
    {
        var returnee = new Dictionary<string, string>();
        returnee[_namingScheme.ClientId] = _conf.ClientId;
        returnee[_namingScheme.ResponseType] = _conf.ResponseType!;
        returnee[_namingScheme.RedirectUri] = _intConf.RedirectUrl;
        returnee[_namingScheme.Scope] = idpSession.Scopes
            .MakeString(" ")
            .UrlEncoded();
        returnee[_namingScheme.CodeChallenge] = idpSession.CodeChallenge
            .Replace('+', '-')
            .Replace('/', '_');
        returnee[_namingScheme.CodeChallengeMethod] = DefaultCodeChallengeMethod;
        returnee[_namingScheme.State] = session.State;
        return returnee;
    }


    public override async Task<IMalarkeyOAuthFlowHandler.RedirectData?> ExtractRedirectData(HttpRequest request)
    {
        var keyValued = request.Query
            .Select(_ => (_.Key, Value: _.Value.ToString()))
            .ToDictionarySafe(_ => _.Key, _ => _.Value);
        if (!keyValued.TryGetValue("code", out var code))
            return null;
        if (!keyValued.TryGetValue("state", out var state))
            return null;
        await Task.CompletedTask;
        var returnee = new IMalarkeyOAuthFlowHandler.RedirectData(
            State: state,
            Code: code
            );
        return returnee;
    }

    public override async Task<MalarkeyProfileIdentity?> ResolveIdentity(MalarkeyAuthenticationSession session, IMalarkeyOAuthFlowHandler.RedirectData redirectData)
    {
        if (redirectData.Code == null)
            return null;
        var url = _conf.TokenEndpointTemplate!;
        var codeVerifier = session.IdpSession!.CodeVerifier;
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var formParameters = new List<(string, string)>
        {
            ("grant_type", "authorization_code"),
            ("code", redirectData.Code!),
            ("redirect_uri", _intConf.RedirectUrl),
            ("client_id", _conf.ClientId),
            ("client_secret", _conf.ClientSecret),
            ("code_verifier", codeVerifier)
        };
        LogDebug($"Requesting access token with parameters: {formParameters.Select(_ => $"{_.Item1}={_.Item2}").MakeString(", ")}");
        request.Content = formParameters.ToFormContent();
        using var client = _httpClientFactory.CreateClient();
        var response = await client.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Received status code: {response.StatusCode} on request for access token");
            _logger.LogError($"  Message: {responseContent}");
            return null;
        }
        LogDebug($"Received back access token data: {responseContent}");
        var responseJson = JsonSerializer.Deserialize<AccessCodeResponseJsonLayout>(responseContent)!;
        var jwtHandler = new JwtSecurityTokenHandler();
        var parsedToken = jwtHandler.ReadJwtToken(responseJson.id_token);
        var tokenClaims = parsedToken.Claims
            .ToDictionarySafe(_ => _.Type, _ => _.Value);
        var email = tokenClaims["email"];
        var returnee = new GoogleIdentity(
            IdentityId: Guid.Empty,
            ProfileId: Guid.Empty,
            GoogleId: tokenClaims["sub"],
            Name: email,
            MiddleNames: null,
            LastName: null,
            Email: email,
            AccessToken: new IdentityProviderToken(
                Token: responseJson.access_token,
                Provider: MalarkeyIdentityProvider.Google,
                Issued: DateTime.Now,
                Expires: DateTime.Now.AddSeconds(responseJson.expires_in),
                RefreshToken: null,
                Scopes: responseJson.scope.Split(" ")
                )


        );
        LogDebug($"Resolved Google Identity: \n {returnee.ToPropertiesString()}");
        return returnee;
    }

    protected override string[] DefaultScopes => ["openid", "email"];


    private record AccessCodeResponseJsonLayout(
        string access_token,
        long expires_in,
        string scope,
        string token_type,
        string id_token
        );


    protected override void LogDebug(string str) {
        _logger.LogInformation(str);
    }


}

