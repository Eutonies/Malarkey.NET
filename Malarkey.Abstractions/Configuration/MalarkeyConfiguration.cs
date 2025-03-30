using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Configuration;
public class MalarkeyConfiguration
{
    public const string ConfigurationElementName = "Malarkey";
    public string? SigningCertificateFile { get; set; }
    public string? SigningCertificatePassword { get; set; }

}
