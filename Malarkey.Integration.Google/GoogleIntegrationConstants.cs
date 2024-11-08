using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Google;
public static class GoogleIntegrationConstants
{
    public const string ConfigurationElementName = $"{IntegrationConstants.IntegrationConfigurationName}:Google";

    public static class Scopes
    {
        public const string UserProfile = "auth/userinfo.profile";
        public const string Email = "auth/userinfo.email";
    }

}
