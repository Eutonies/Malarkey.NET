using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Configuration;
public class MalarkeyIntegrationConfiguration
{
    public const string ConfigurationElementName = "Integration";

    public string AuthenticationPath { get; set; }
    public string RedirectUrl { get; set; }
    public string AccessDeniedUrl { get; set; }

    public string PublicKeyFile {  get; set; }

    private string? _publicKey;

    public string PublicKey { get
        {
            _publicKey ??= File.ReadAllText(PublicKeyFile);
            return _publicKey;
        } }


    public MalarkeyIdentityProviderConfiguration Microsoft { get; set; }
    public MalarkeyIdentityProviderConfiguration Google { get; set; }
    public MalarkeyIdentityProviderConfiguration Facebook{ get; set; }
    public MalarkeyIdentityProviderConfiguration Spotify { get; set; }


}
