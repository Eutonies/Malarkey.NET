using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace Malarkey.Abstractions.UI;

public class MalarkeyX509Utilities
{

    public static X509Certificate2 GenerateCertificate(string commonName)
    {
        using (RSA parent = RSA.Create(4096))
        using (RSA rsa = RSA.Create(2048))
        {
            CertificateRequest parentReq = new CertificateRequest(
                $"CN={commonName}",
                parent,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            parentReq.CertificateExtensions.Add(
                new X509BasicConstraintsExtension(true, false, 0, true));

            parentReq.CertificateExtensions.Add(
                new X509SubjectKeyIdentifierExtension(parentReq.PublicKey, false));

            using (X509Certificate2 parentCert = parentReq.CreateSelfSigned(
                DateTimeOffset.UtcNow.AddDays(-45),
                DateTimeOffset.UtcNow.AddDays(365)))
            {
                CertificateRequest req = new CertificateRequest(
                    "CN=Malarkey.NET",
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                req.CertificateExtensions.Add(
                    new X509BasicConstraintsExtension(false, false, 0, false));

                req.CertificateExtensions.Add(
                    new X509KeyUsageExtension(
                        X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation,
                        false));

                req.CertificateExtensions.Add(
                    new X509EnhancedKeyUsageExtension(
                        new OidCollection
                        {
                    new Oid("1.3.6.1.5.5.7.3.8")
                        },
                        true));

                req.CertificateExtensions.Add(
                    new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

                var returnee = req.Create(
                    parentCert,
                    DateTimeOffset.UtcNow.AddDays(-1),
                    DateTimeOffset.UtcNow.AddDays(90),
                    new byte[] { 1, 2, 3, 4 });
                return returnee;
            }
        }
    }

}
