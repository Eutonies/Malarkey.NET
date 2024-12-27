using Malarkey.Domain.Authentication;
using Malarkey.Abstractions.Profile;
using Malarkey.Domain.Util;
using Malarkey.Integration.Authentication.Naming;
using Malarkey.Integration.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Malarkey.Abstractions.Token;
using Malarkey.Application.Util;
using System.Net.Http;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Malarkey.Integration.Authentication.OAuthFlowHandlers;
internal class MalarkeyGoogleOAuthFlowHandler : MalarkeyOAuthFlowHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    public override MalarkeyIdentityProvider HandlerFor => MalarkeyIdentityProvider.Google;


    public MalarkeyGoogleOAuthFlowHandler(IOptions<MalarkeyIntegrationConfiguration> intConf, IHttpClientFactory httpClientFactory) : base(intConf)
    {
        _httpClientFactory = httpClientFactory;
    }
    public override string AuthorizationEndpoint => _conf.AuthorizationEndpointTemplate;

    protected override MalarkeyOAuthNamingScheme ProduceNamingScheme() => MalarkeyGoogleOAuthNamingScheme.Init(_conf.NamingSchemeOverwrites);
    protected override MalarkeyIdentityProviderConfiguration ProduceConfiguration() => _intConf.Google;

    public override IReadOnlyDictionary<string, string> ProduceRequestQueryParameters(MalarkeyAuthenticationSession session)
    {
        var returnee = new Dictionary<string, string>();
        returnee[_namingScheme.ClientId] = _conf.ClientId;
        returnee[_namingScheme.ResponseType] = _conf.ResponseType!;
        returnee[_namingScheme.RedirectUri] = _intConf.RedirectUrl;
        returnee[_namingScheme.Scope] = (session.Scopes ?? (_conf.Scopes ?? DefaultScopes))
            .MakeString(" ")
            .UrlEncoded();
        returnee[_namingScheme.CodeChallenge] = session.CodeChallenge
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
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var formParameters = new List<(string, string)>
        {
            ("grant_type", "authorization_code"),
            ("code", redirectData.Code!),
            ("redirect_uri", _intConf.RedirectUrl),
            ("client_id", _conf.ClientId),
            ("client_secret", _conf.ClientSecret),
            ("code_verifier", session.CodeVerifier)
        };
        request.Content = formParameters.ToFormContent();
        using var client = _httpClientFactory.CreateClient();
        var response = await client.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
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
                Issued: DateTime.Now,
                Expires: DateTime.Now.AddSeconds(responseJson.expires_in),
                RefreshToken: null,
                Scopes: responseJson.scope.Split(" ")
                )


        );


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


}


/*
 * ####################### Google ##########################
https://developers.google.com/identity/protocols/oauth2/web-server#httprest_1

https://accounts.google.com/o/oauth2/v2/auth?
    client_id=00001111-aaaa-2222-bbbb-3333cccc4444
    &response_type=code
    &redirect_uri=http%3A%2F%2Flocalhost%2Fmyapp%2F
    &response_mode=query
    &state=12345
    &nonce=0394852-3190485-2490358&
    &code_challenge=YTFjNjI1OWYzMzA3MTI4ZDY2Njg5M2RkNmVjNDE5YmEyZGRhOGYyM2IzNjdmZWFhMTQ1ODg3NDcxY2Nl
    &code_challenge_method=S256

POST /token HTTP/1.1
Host: oauth2.googleapis.com
Content-Type: application/x-www-form-urlencoded

code=4/P7q7W91a-oMsCeLvIaQm6bTrgtp7&
client_id=your_client_id&
client_secret=your_client_secret&
redirect_uri=https%3A//oauth2.example.com/code&
grant_type=authorization_code
 
 * 
 * 
 * 
 */
