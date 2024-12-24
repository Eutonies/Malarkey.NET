using Malarkey.Application.Profile.Persistence;
using Malarkey.Application.Util;
using Malarkey.Domain.Authentication;
using Malarkey.Domain.Profile;
using Malarkey.Domain.Util;
using Malarkey.Integration.Authentication.Naming;
using Malarkey.Integration.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication.OAuthFlowHandlers;
internal class MalarkeyMicrosoftOAuthFlowHandler : MalarkeyOAuthFlowHandler
{
    private readonly IMalarkeyProfileRepository _profileRepo;
    
    public override MalarkeyOAuthIdentityProvider HandlerFor => MalarkeyOAuthIdentityProvider.Microsoft;


    public MalarkeyMicrosoftOAuthFlowHandler(IOptions<MalarkeyIntegrationConfiguration> intConf, IMalarkeyProfileRepository profileRepo) : base(intConf)
    {
        _profileRepo = profileRepo;
    }
    public override string AuthorizationEndpoint => _conf.AuthorizationEndpointTemplate.Replace("{tenant}", _conf.Tenant);

    protected override MalarkeyOAuthNamingScheme ProduceNamingScheme() => MalarkeyMicrosoftOAuthNamingScheme.Init(_conf.NamingSchemeOverwrites);
    protected override MalarkeyIdentityProviderConfiguration ProduceConfiguration() => _intConf.Microsoft;
    public override async Task<IMalarkeyOAuthFlowHandler.RedirectData?> ExtractRedirectData(HttpRequest request)
    {
        var body = await request.Body.ReadAsStringAsync();
        var splitted = body.Split('&')
            .ToList();
        var keyValued = splitted
            .Where(_ => _.Contains('='))
            .Select(_ => _.Split('='))
            .Select(_ => (Key: _[0], Value: _[1]))
            .ToDictionarySafe(_ => _.Key, _ => _.Value);

        if (!keyValued.TryGetValue("state", out var state))
            return null;
        var returnee = new IMalarkeyOAuthFlowHandler.RedirectData(
            State: state,
            Token: keyValued.GetValueOrDefault("token"),
            IdToken: keyValued.GetValueOrDefault("id_token")
        );
        return returnee;
    }

    public override async Task<MalarkeyProfileIdentity?> ResolveIdentity(MalarkeyAuthenticationSession session, IMalarkeyOAuthFlowHandler.RedirectData redirectData)
    {
        if (redirectData.IdToken == null)
            return null;
        var jwtHandler = new JwtSecurityTokenHandler();
        var parsedToken = jwtHandler.ReadJwtToken(redirectData.IdToken);
        var claimsMap = parsedToken.Claims
            .Select(_ => (_.Type, _.Value))
            .ToDictionarySafe(_ => _.Type, _ => _.Value);
        if(!claimsMap.TryGetValue("nonce", out var nonce))
            return null;
        var sessionNonce = session?.Nonce?.UrlDecoded();
        if (sessionNonce != null && sessionNonce != nonce)
            return null;
        if (!claimsMap.TryGetValue("oid", out var userId))
            return null;
        userId = ShortenUserObjectId(userId);
        if (!claimsMap.TryGetValue("email", out var email))
            return null;
        if (!claimsMap.TryGetValue("preferred_username", out var prefName))
            prefName = "unknown";
        if (!claimsMap.TryGetValue("given_name", out var givenName))
            givenName = "unknown";
        if (!claimsMap.TryGetValue("family_name", out var famName))
            famName = "unknown";

        var identity = new MicrosoftIdentity(
            IdentityId: Guid.Empty,
            ProfileId: Guid.Empty,
            MicrosoftId: userId,
            PreferredName: prefName,
            Name: givenName,
            MiddleNames: null,
            LastName: famName
            );
        return identity;
    }

    private readonly static Regex UserObjectIdRegex = new Regex("^0*(.*)");
    private static string ShortenUserObjectId(string userObjectId)
    {
        userObjectId = userObjectId.Replace("-", "");
        var parseResult = UserObjectIdRegex.Match(userObjectId);
        if (parseResult.Groups.Count <= 1)
            return userObjectId;
        var returnee = parseResult.Groups[1].Value;
        return returnee;
    }

}


/*
 * ###############    Microsoft ####################
https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-auth-code-flow

https://login.microsoftonline.com/{tenant}/oauth2/v2.0/authorize?
    client_id=00001111-aaaa-2222-bbbb-3333cccc4444
    &response_type=code
    &redirect_uri=http%3A%2F%2Flocalhost%2Fmyapp%2F
    &response_mode=query
    &scope=https%3A%2F%2Fgraph.microsoft.com%2Fmail.read
    &state=12345
    &code_challenge=YTFjNjI1OWYzMzA3MTI4ZDY2Njg5M2RkNmVjNDE5YmEyZGRhOGYyM2IzNjdmZWFhMTQ1ODg3NDcxY2Nl
    &code_challenge_method=S256

RESPONSE:
GET http://localhost?
code=AwABAAAAvPM1KaPlrEqdFSBzjqfTGBCmLdgfSTLEMPGYuNHSUYBrq...
&state=12345
 

REDEEM CODE
POST /{tenant}/oauth2/v2.0/token HTTP/1.1
Host: https://login.microsoftonline.com
Content-Type: application/x-www-form-urlencoded

client_id=11112222-bbbb-3333-cccc-4444dddd5555
&scope=https%3A%2F%2Fgraph.microsoft.com%2Fmail.read
&code=OAAABAAAAiL9Kn2Z27UubvWFPbm0gLWQJVzCTE9UkP3pSx1aXxUjq3n8b2JRLk4OxVXr...
&redirect_uri=http%3A%2F%2Flocalhost%2Fmyapp%2F
&grant_type=authorization_code
&code_verifier=ThisIsntRandomButItNeedsToBe43CharactersLong 
&client_secret=sampleCredentia1s    // NOTE: Only required for web apps. This secret needs to be URL-Encoded.

 * 
 * ####################### Google ##########################
https://developers.google.com/identity/protocols/oauth2/web-server#httprest_1

https://accounts.google.com/o/oauth2/v2/auth?
    client_id=00001111-aaaa-2222-bbbb-3333cccc4444
    &response_type=code
    &redirect_uri=http%3A%2F%2Flocalhost%2Fmyapp%2F
    &response_mode=query
    &scope=https%3A%2F%2Fgraph.microsoft.com%2Fmail.read
    &state=12345
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

######################### Facebook ##############################
https://www.facebook.com/v19.0/dialog/oauth?
  client_id={app-id}
  &redirect_uri={redirect-uri}
  &state={state-param}

GET
https://graph.facebook.com/v19.0/oauth/access_token


 * 
 * 
 * ######################## Spotify ##############################

const clientId = 'YOUR_CLIENT_ID';
const redirectUri = 'http://localhost:8080';

const scope = 'user-read-private user-read-email';
const authUrl = new URL("https://accounts.spotify.com/authorize")

// generated in the previous step
window.localStorage.setItem('code_verifier', codeVerifier);

const params =  {
  response_type: 'code',
  client_id: clientId,
  scope,
  code_challenge_method: 'S256',
  code_challenge: codeChallenge,
  redirect_uri: redirectUri,
}

authUrl.search = new URLSearchParams(params).toString();
window.location.href = authUrl.toString();

REDEEM ACCESS TOKEN:
grant_type	Required	This field must contain the value authorization_code.
code	Required	The authorization code returned from the previous request.
redirect_uri	Required	This parameter is used for validation only (there is no actual redirection). The value of this parameter must exactly match the value of redirect_uri supplied when requesting the authorization code.
client_id	Required	The client ID for your app, available from the developer dashboard.
code_verifier	Required	The value of this parameter must match the value of the code_verifier that your app generated in the previous step.
The request must include the following HTTP header:

Header Parameter	Relevance	Value
Content-Type	Required	Set to application/x-www-form-urlencoded. 
 
 * 
 * 
 * 
 */
