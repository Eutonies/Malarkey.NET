using Malarkey.Abstractions.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Configuration;
public class MalarkeyIntegrationConfiguration
{
    public const string ConfigurationElementName = "Integration";

    public MalarkeyEmailVerificationConfiguration Email { get; set; }

    public string ServerBasePath { get; set; }

    public string AuthenticationPath { get; set; }
    public string RedirectPath { get; set; }
    public string RedirectUrl => $"{ServerBasePath}/{RedirectPath}";

    public string AccessDeniedPath { get; set; }

    public string PublicKeyFile {  get; set; }

    private string? _publicKey;

    public string PublicKey { get
        {
            _publicKey ??= File.ReadAllText(PublicKeyFile);
            return _publicKey;
        } }


    public string? PrivateKeyFile { get; set; }

    private string? _privateKey;
    public string? PrivateKey => _privateKey ??= PrivateKeyFile?.Pipe(File.ReadAllText);


    public MalarkeyIdentityProviderConfiguration Microsoft { get; set; }
    public MalarkeyIdentityProviderConfiguration Google { get; set; }
    public MalarkeyIdentityProviderConfiguration Facebook{ get; set; }
    public MalarkeyIdentityProviderConfiguration Spotify { get; set; }


}
