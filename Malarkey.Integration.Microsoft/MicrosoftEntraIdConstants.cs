using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Microsoft;
public static class MicrosoftEntraIdConstants
{
    public const string MicrosoftIntegrationElementName = "Microsoft";
    public const string MicrosoftConfigurationName = $"{IntegrationConstants.IntegrationConfigurationName}:{MicrosoftIntegrationElementName}";
    public const string AzureAdElementName = "AzureAd";
    public const string AzureAdConfigurationName = $"{MicrosoftConfigurationName}:{AzureAdElementName}";
    public const string DownstreamApisElementName = "DownstreamApis";
    public const string DownstreamApisConfigurationName = $"{MicrosoftConfigurationName}:{DownstreamApisElementName}";
    public const string GraphApiElementName = "MicrosoftGraph";
    public const string GraphApiConfigurationName = $"{DownstreamApisConfigurationName}:{GraphApiElementName}";



    public static class GraphScopes
    {
        public const string ReadContacts = "Contacts.Read";
        public const string Email = "email";
        public const string OpenId = "openid";
        public const string Profile = "profile";
        public const string ReadUser = "User.Read";


        public static readonly string[] All = [ReadContacts, Email, OpenId, Profile, ReadUser];

    }

}
