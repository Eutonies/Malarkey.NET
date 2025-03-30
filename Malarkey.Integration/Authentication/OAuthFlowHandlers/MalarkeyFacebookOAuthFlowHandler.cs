using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Util;
using Malarkey.Integration.Authentication.Naming;
using Malarkey.Integration.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

namespace Malarkey.Integration.Authentication.OAuthFlowHandlers;
public class MalarkeyFacebookOAuthFlowHandler : MalarkeyOAuthFlowHandler
{
    private readonly IHttpClientFactory _httpClientFactory;

    public override MalarkeyIdentityProvider HandlerFor => MalarkeyIdentityProvider.Facebook;

    public MalarkeyFacebookOAuthFlowHandler(
        IOptions<MalarkeyIntegrationConfiguration> intConf, 
        IHttpClientFactory httpClientFactory,
        ILogger<MalarkeyFacebookOAuthFlowHandler> logger) : base(intConf, logger)
    {
        _httpClientFactory = httpClientFactory;
    }
    public override string AuthorizationEndpoint => _conf.AuthorizationEndpointTemplate;

    protected override MalarkeyOAuthNamingScheme ProduceNamingScheme() => MalarkeyFacebookOAuthNamingScheme.Init(_conf.NamingSchemeOverwrites);
    protected override MalarkeyIdentityProviderConfiguration ProduceConfiguration() => _intConf.Facebook!;

    protected override bool StripCodeChallengePadding => true;

    public override IReadOnlyDictionary<string, string> ProduceRequestQueryParameters(MalarkeyAuthenticationSession session, MalarkeyAuthenticationIdpSession idpSession) => new List<(string, string?)>
    {
        (_namingScheme.ClientId, _conf.ClientId),
        (_namingScheme.Scope, _conf.Scopes?.MakeString(" ")?.UrlEncoded() ?? ""),
        (_namingScheme.ResponseType, _conf.ResponseType),
        (_namingScheme.RedirectUri, _intConf.RedirectUrl.UrlEncoded()),
        (_namingScheme.State, session.State
            .Replace('+','-')
            .Replace('/','_')
        ),
        (_namingScheme.CodeChallenge, idpSession.CodeChallenge),
        (_namingScheme.CodeChallengeMethod, _conf.CodeChallengeMethod),
        (_namingScheme.Nonce, idpSession.Nonce)
    }
    .Where(_ => _.Item2 != null)
    .ToDictionarySafe(_ => _.Item1, _ => _.Item2!);

    public override string ProduceAuthorizationUrl(MalarkeyAuthenticationSession session, MalarkeyAuthenticationIdpSession idpSession)
    {
        var returnee =  base.ProduceAuthorizationUrl(session, idpSession);
        _logger.LogInformation($"Using facebook authorization URL: {returnee}");
        return returnee;
    }

    public override async Task<IMalarkeyOAuthFlowHandler.RedirectData?> ExtractRedirectData(HttpRequest request)
    {
        await Task.CompletedTask;
        var queryMap = request.Query
            .Select(_ => (_.Key, Value: _.Value.ToString()))
            .ToDictionarySafe(_ => _.Key, _ => _.Value);
        if(!queryMap.TryGetValue("state", out var state))
            return null;
        if (!queryMap.TryGetValue("code", out var code))
            return null;
        var returnee = new IMalarkeyOAuthFlowHandler.RedirectData(State: state, Code: code);
        return returnee;
    }

    public override async Task<MalarkeyProfileIdentity?> ResolveIdentity(MalarkeyAuthenticationSession session, IMalarkeyOAuthFlowHandler.RedirectData redirectData)
    {
        var codeVerifier = session.IdpSession!.CodeVerifier.UrlEncoded();
        var urlBuilder = new StringBuilder(_conf.TokenEndpointTemplate!);
        urlBuilder.Append($"?{_namingScheme.ClientId}={_conf.ClientId.UrlEncoded()}");
        urlBuilder.Append($"&{_namingScheme.RedirectUri}={_intConf.RedirectUrl.UrlEncoded()}");
        urlBuilder.Append($"&{_namingScheme.RedemptionCodeVerifier}={codeVerifier.UrlEncoded()}");
        urlBuilder.Append($"&{_namingScheme.RedemptionCode}={redirectData.Code!.UrlEncoded()}");
        var url = urlBuilder.ToString();
        _logger.LogInformation($"Firing off access token request against facebook through URL: {url}");


        using var client = _httpClientFactory.CreateClient();
        var accessTokenRequest = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await client.SendAsync(accessTokenRequest);
        var content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Received error code: {response.StatusCode} on access token request. Message: {content}");
            return null;
        }
        var tokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(content);
        _logger.LogInformation($"Got back token response from facebook: {tokenResponse}");
        var jwtHandler = new JwtSecurityTokenHandler();
        var parsedToken = jwtHandler.ReadJwtToken(tokenResponse!.id_token);
        var tokenClaims = parsedToken.Claims
            .ToDictionarySafe(_ => _.Type, _ => _.Value);
        var returnee = new FacebookIdentity(
            IdentityId: Guid.Empty,
            ProfileId: Guid.Empty,
            FacebookId: parsedToken.Subject,
            PreferredName: tokenClaims.GetValueOrDefault("name") ?? tokenClaims.GetValueOrDefault("given_name") ?? "Unknown",
            Name: tokenClaims.GetValueOrDefault("given_name") ?? "Unknown",
            MiddleNames: null,
            LastName: tokenClaims.GetValueOrDefault("family_name"),
            Email: tokenClaims.GetValueOrDefault("email")
        );
        return returnee;

    }


    private record AccessTokenResponse(
        string access_token,
        string token_type,
        int expires_in,
        string id_token
        );


}
