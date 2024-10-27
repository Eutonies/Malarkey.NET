using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Microsoft;
public static class MicrosoftEntraIdConstants
{
    public const string MicrosoftIntegrationElementName = "Microsoft";
    public const string AzureElementName = "AzureAd";
    public const string AzureConfigurationName = $"{IntegrationConstants.IntegrationConfigurationName}:{MicrosoftIntegrationElementName}:{AzureElementName}";


}
