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
        public const string FacebookAuthenticationSchemeName = "facebook";
        public const string GoogleAuthenticationSchemeName = "google";
        public const string MicrosoftAuthenticationSchemeName = "microsoft";
        public const string MicrosoftIdCookieName = "MalarkeyAzureId";
        public const string MicrosoftSchemaClaimName = "http://schemas.microsoft.com/identity";
    }


    public static class AuthorizationPolicies
    {
        public const string FacebookIsAuthenticatedPolicyName = "MalarkeyFacebookIsAuthenticated";
        public const string GoogleIsAuthenticatedPolicyName = "MalarkeyGoogleIsAuthenticated";
        public const string MicrosoftIsAuthenticatedPolicyName = "MalarkeyMicrosoftIsAuthenticated";

    }


}


