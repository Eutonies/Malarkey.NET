using Malarkey.Integration.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Google.Configuration;
public class GoogleIdentityProviderConfiguration : IdentityProviderConfiguration
{
    public string ProjectId { get; set; }
    public string AuthenticationUri { get; set; }
    public string TokenUri { get; set; }
    public string? AuthenticationProviderCertificateUrl { get; set; }
    public string[] Scopes { get; set; }

}
