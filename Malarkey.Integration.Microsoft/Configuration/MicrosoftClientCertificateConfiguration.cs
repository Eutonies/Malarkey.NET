using Malarkey.Application.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Microsoft.Configuration;
public class MicrosoftClientCertificateConfiguration : MalarkeyCertificateConfiguration
{
    public string SourceType { get; set; }
    public string CertificateDiskPath { get; set; }

    protected override string CertificateFileToUse() => CertificateDiskPath;


}
