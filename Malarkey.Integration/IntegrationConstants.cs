using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration;
public static class IntegrationConstants
{
    public const string IntegrationConfigurationName = "Integration";

    public const string MalarkeyAuthenticationScheme = "Malarkey.Session";

    public const string IdProviderHeaderName = "Malarkey.IDP";

    public const string TokenReceiverHeaderName = "Malarkey.Token.Audience";

    public const string ForwarderQueryParameterName = "forwarder";


    public static class MalarkeyIdProviders 
    {
        public const string Facebook = "facebook";
        public const string Google = "google";
        public const string Microsoft = "microsoft";
        public const string Spotify = "spotify";
        public const string MicrosoftSchemaClaimName = "http://schemas.microsoft.com/identity";
    }


    public static class AuthorizationPolicies
    {
        public const string FacebookIsAuthenticatedPolicyName = "MalarkeyFacebookIsAuthenticated";
        public const string GoogleIsAuthenticatedPolicyName = "MalarkeyGoogleIsAuthenticated";
        public const string MicrosoftIsAuthenticatedPolicyName = "MalarkeyMicrosoftIsAuthenticated";

    }

    public static class SuccessHeaders
    {
        public const string ProfileTokenHeaderName = "Malarkey.Profile";
        public const string IdentityTokenHeaderName = "Malarkey.Identity";
        public const string IdentityProviderAccessTokenHeaderName = "Malarkey.IdP.Token";
    }


}


