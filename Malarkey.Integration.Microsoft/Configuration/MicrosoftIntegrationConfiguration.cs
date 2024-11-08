using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Microsoft.Configuration;
public class MicrosoftIntegrationConfiguration
{
    public const string ConfigurationElementName = $"{IntegrationConstants.IntegrationConfigurationName}:Microsoft";

    public MicrosoftIdentityProviderConfiguration AzureAd { get; set; }
    public MicrosoftDownstreamApisConfiguration DownstreamApis { get; set; }

    public void WriteTo(MicrosoftIntegrationConfiguration other)
    {
        other.AzureAd = AzureAd;
        other.DownstreamApis = DownstreamApis;
    }


}
