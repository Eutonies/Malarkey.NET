using Malarkey.Domain.Authentication;
using Malarkey.Domain.Util;
using Malarkey.Integration.Authentication.Naming;
using Malarkey.Integration.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication.OAuthFlowHandlers;
internal class MalarkeyGoogleOAuthFlowHandler : MalarkeyOAuthFlowHandler
{
    public override MalarkeyOAuthIdentityProvider HandlerFor => MalarkeyOAuthIdentityProvider.Microsoft;

    public MalarkeyGoogleOAuthFlowHandler(MalarkeyIntegrationConfiguration intConf) : base(intConf)
    {
    }
    public override string AuthorizationEndpoint => _conf.AuthorizationEndpointTemplate;

    protected override MalarkeyOAuthNamingScheme ProduceNamingScheme() => MalarkeyGoogleOAuthNamingScheme.Init(_conf.NamingSchemeOverwrites);
    protected override MalarkeyIdentityProviderConfiguration ProduceConfiguration() => _intConf.Google;

    protected override string[] DefaultScopes => ["openid", "email"];



}


/*
 * ####################### Google ##########################
https://developers.google.com/identity/protocols/oauth2/web-server#httprest_1

https://accounts.google.com/o/oauth2/v2/auth?
    client_id=00001111-aaaa-2222-bbbb-3333cccc4444
    &response_type=code
    &redirect_uri=http%3A%2F%2Flocalhost%2Fmyapp%2F
    &response_mode=query
    &scope=https%3A%2F%2Fgraph.microsoft.com%2Fmail.read
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
