using Malarkey.Integration.Authentication.Naming;
using Malarkey.Integration.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication;
internal abstract class MalarkeyOAuthFlowHandler
{
    private readonly MalarkeyOAuthNamingScheme _namingScheme;
    private readonly MalarkeyIdentityProviderConfiguration _conf;


    public MalarkeyOAuthFlowHandler(IConfiguration configuration)
    {
        _namingScheme = ProduceNamingScheme();
        _conf = ProduceConfiguration(configuration);
    }
    public string AuthorizationEndpoint { get; }

    protected abstract MalarkeyOAuthNamingScheme ProduceNamingScheme();
    protected abstract MalarkeyIdentityProviderConfiguration ProduceConfiguration(IConfiguration configuration);
    



    public virtual IReadOnlyDictionary<string, string> ProduceRequestQueryParameters(HttpClient client, MalarkeyAuthenticationSession session)
    {
        var returnee = new Dictionary<string, string>();
        returnee[_namingScheme.ClientId] = _conf.ClientId;
        returnee[_namingScheme.ClientSecret] = _conf.ClientSecret;
        returnee[_namingScheme.ResponseType] = _conf.ResponseType!;
        returnee[_namingScheme.ClientId] = _conf.ClientId;
        returnee[_namingScheme.ClientId] = _conf.ClientId;
        returnee[_namingScheme.ClientId] = _conf.ClientId;

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
