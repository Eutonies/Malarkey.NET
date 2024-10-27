using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration;
public static class IntegrationConstants
{
    public const string IntegrationConfigurationName = "Integration";

    public static class IdProviders
    {
        public const string MicrosoftIdCookieName = "MalarkeyAzureId";
    }


    public static class AuthorizationPolicies
    {
        public const string IsAuthenticatedName = "MalarkeyIsAuthenticated";
    }


}


