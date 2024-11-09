using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication;

internal class MalarkeyAuthenticationPolicySchemeHandler : PolicySchemeHandler
{
    public MalarkeyAuthenticationPolicySchemeHandler(IOptionsMonitor<PolicySchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
    }


    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return base.HandleAuthenticateAsync();
    }

}



