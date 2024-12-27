using Malarkey.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Malarkey.Client.Authentication;
internal class MalarkeyClientAuthenticationHandler : AuthenticationHandler<MalarkeyClientAuthenticationSchemeOptions>
{

    public MalarkeyClientAuthenticationHandler(IOptionsMonitor<MalarkeyClientAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var endpoint = Context.GetEndpoint();
        if (endpoint == null)
            return AuthenticateResult.Fail("No endpoint info found");
        var authAttribute = endpoint.Metadata
            .OfType<MalarkeyAuthenticationAttribute>()
            .FirstOrDefault();
        if (authAttribute == null)
            return AuthenticateResult.NoResult();
        var profileCookie = Request.Cookies
            .Where(_ => _.Key == MalarkeyConstants.Authentication.ProfileCookieName)
            .Select(_ => _.Value.ToString())
            .FirstOrDefault();
        if (profileCookie == null)
            return AuthenticateResult.Fail("No profile cookie found");
        var profile = 
        if(authAttribute.IdentityProvider == null)
        {

        }



        throw new NotImplementedException();
    }
}
