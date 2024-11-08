using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Microsoft.Configuration;
public class MicrosoftClientCertificateConfiguration
{
    public string SourceType { get; set; }
    public string CertificateDiskPath { get; set; }
    public string CertificatePassword { get; set; }

    private X509Certificate2? _asCert;
    public X509Certificate2 AsCertificate => _asCert ??= Create();

    private X509Certificate2 Create()
    {
        var bytes = File.ReadAllBytes(CertificateDiskPath);
        var returnee = new X509Certificate2(bytes, CertificatePassword);
        return returnee;
    }



}
