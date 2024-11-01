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


}
