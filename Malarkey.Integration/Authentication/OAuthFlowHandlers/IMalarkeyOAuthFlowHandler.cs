using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Util;
using Malarkey.Integration.Authentication.Naming;
using Malarkey.Integration.Configuration;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Malarkey.Abstractions.Profile;

namespace Malarkey.Integration.Authentication.OAuthFlowHandlers;
public interface IMalarkeyOAuthFlowHandler
{
    public MalarkeyIdentityProvider HandlerFor { get; }
    public string ProduceAuthorizationUrl(MalarkeyAuthenticationSession session, MalarkeyAuthenticationIdpSession idpSession);

    public MalarkeyAuthenticationIdpSession PopulateIdpSession(MalarkeyAuthenticationSession session);

    public Task<RedirectData?> ExtractRedirectData(HttpRequest request);

    public Task<MalarkeyProfileIdentity?> ResolveIdentity(MalarkeyAuthenticationSession session, RedirectData redirectData);

    public record RedirectData(string State, string? Token = null, string? IdToken = null, string? Code = null)
    {
        public override string ToString() =>
            $"State: {State}, Token: {Token}, IdToken: {IdToken}, Code: {Code}";
    }



}
