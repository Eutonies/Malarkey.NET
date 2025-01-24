using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Util;
using Malarkey.Integration.Authentication.Naming;
using Malarkey.Integration.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

namespace Malarkey.Integration.Authentication.OAuthFlowHandlers;
internal class MalarkeyFacebookOAuthFlowHandler : MalarkeyOAuthFlowHandler
{
    private readonly IHttpClientFactory _httpClientFactory;

    public override MalarkeyIdentityProvider HandlerFor => MalarkeyIdentityProvider.Facebook;

    public MalarkeyFacebookOAuthFlowHandler(IOptions<MalarkeyIntegrationConfiguration> intConf, IHttpClientFactory httpClientFactory) : base(intConf)
    {
        _httpClientFactory = httpClientFactory;
    }
    public override string AuthorizationEndpoint => _conf.AuthorizationEndpointTemplate;

    protected override MalarkeyOAuthNamingScheme ProduceNamingScheme() => MalarkeyFacebookOAuthNamingScheme.Init(_conf.NamingSchemeOverwrites);
    protected override MalarkeyIdentityProviderConfiguration ProduceConfiguration() => _intConf.Facebook;

    public override IReadOnlyDictionary<string, string> ProduceRequestQueryParameters(MalarkeyAuthenticationSession session, MalarkeyAuthenticationIdpSession idpSession) => new List<(string, string?)>
    {
        (_namingScheme.ClientId, _conf.ClientId),
        (_namingScheme.Scope, _conf.Scopes?.MakeString(",") ?? ""),
        (_namingScheme.ResponseType, _conf.ResponseType),
        (_namingScheme.RedirectUri, _intConf.RedirectUrl),
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
        urlBuilder.Append($"&{_namingScheme.RedemptionCodeVerifier}={codeVerifier}");
        urlBuilder.Append($"&{_namingScheme.RedemptionCode}={redirectData.Code!.UrlEncoded()}");
        var url = urlBuilder.ToString();

        using var client = _httpClientFactory.CreateClient();
        var accessTokenRequest = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await client.SendAsync(accessTokenRequest);
        var content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return null;
        var tokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(content);
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


/*
######################### Facebook ##############################
https://www.facebook.com/v19.0/dialog/oauth?
  client_id={app-id}
  &redirect_uri={redirect-uri}
  &state={state-param}

GET
https://graph.facebook.com/v19.0/oauth/access_token


 
 * 
 * 
 * 
 */
