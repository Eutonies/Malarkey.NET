using Malarkey.Domain.Authentication;
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

namespace Malarkey.Integration.Authentication.OAuthFlowHandlers;
internal class MalarkeyFacebookOAuthFlowHandler : MalarkeyOAuthFlowHandler
{
    public override MalarkeyOAuthIdentityProvider HandlerFor => MalarkeyOAuthIdentityProvider.Facebook;

    public MalarkeyFacebookOAuthFlowHandler(IOptions<MalarkeyIntegrationConfiguration> intConf) : base(intConf)
    {
    }
    public override string AuthorizationEndpoint => _conf.AuthorizationEndpointTemplate;

    protected override MalarkeyOAuthNamingScheme ProduceNamingScheme() => MalarkeyFacebookOAuthNamingScheme.Init(_conf.NamingSchemeOverwrites);
    protected override MalarkeyIdentityProviderConfiguration ProduceConfiguration() => _intConf.Facebook;

    public override IReadOnlyDictionary<string, string> ProduceRequestQueryParameters(MalarkeyAuthenticationSession session) => new List<(string, string)>
    {
        (_namingScheme.ClientId, _conf.ClientId),
        (_namingScheme.RedirectUri, _intConf.RedirectUrl),
        (_namingScheme.State, session.State)
    }.ToDictionarySafe(_ => _.Item1, _ => _.Item2);

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
