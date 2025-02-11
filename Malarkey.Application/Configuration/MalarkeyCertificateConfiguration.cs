using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
        var fileContent = File.ReadAllText(fileToUse);
        var certBytes = Convert.FromBase64String(fileContent);
        var returnee = X509CertificateLoader.LoadPkcs12(certBytes, password: CertificatePasswordToUse());
        return returnee;
    }
    private string? _exportableCertificate;
    public string PublicKeyPem => _exportableCertificate ??= AsCertificate.ExportCertificatePem();

    private RSA? _publicKey;
    public RSA PublicKey => _publicKey ??= AsCertificate.GetRSAPublicKey()!;

    private RSA? _privateKey;
    public RSA PrivateKey => _privateKey ??= AsCertificate.GetRSAPrivateKey()!;




}
