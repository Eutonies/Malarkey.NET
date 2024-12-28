using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Configuration;
public class MalarkeyCertificateConfiguration
{
    public string? CertificateFileName { get; set; }
    public string? CertificatePassword { get; set; }

    protected X509Certificate2? _asCert;
    public X509Certificate2 AsCertificate => _asCert ??= Create();

    protected virtual string CertificateFileToUse() => CertificateFileName!;
    protected virtual string CertificatePasswordToUse() => CertificatePassword!;

    private X509Certificate2 Create()
    {
        var fileToUse = CertificateFileToUse();
        var returnee = X509CertificateLoader.LoadPkcs12FromFile(fileToUse, password: CertificatePasswordToUse());
        return returnee;
    }
    private string? _exportableCertificate;
    public string ExportableCertificate => _exportableCertificate ??= AsCertificate.ExportCertificatePem();
}
