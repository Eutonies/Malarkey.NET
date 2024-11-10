using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Security.Configuration;
internal class SecurityConfiguration
{
    public const string ConfigurationElementName = "Security";

    public string TokenCertificate { get; set; }

    public string TokenCertificatePassword { get; set; }

}
