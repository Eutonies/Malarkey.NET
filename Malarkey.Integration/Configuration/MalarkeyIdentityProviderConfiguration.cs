using Malarkey.Integration.Authentication.Naming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Configuration;
public class MalarkeyIdentityProviderConfiguration
{
    public MalarkeyOAuthNamingSchemeConfiguration? NamingSchemeOverwrites { get; set; }

    public string AuthorizationEndpointTemplate { get; set; }

    public string? Tenant { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string? ResponseType { get; set; }
    public string? CodeChallengeMethod { get; set; }
    public string[]? Scopes { get; set; }


}
