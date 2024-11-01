using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration;
public static class DependencyInjection
{

    public static IServiceCollection AddAuthenticatedAuthorizationPolicy(this IServiceCollection services)
    {
        services.AddAuthorization(opts =>
        {
            opts.AddPolicy(IntegrationConstants.AuthorizationPolicies.MicrosoftIsAuthenticatedPolicyName, pol =>
            {
                pol.RequireAssertion(cont =>
                {
                    foreach(var identity in cont.User.Identities)
                    {
                        foreach(var claim in identity.Claims.Where(_ => _.Type.ToLower().StartsWith(IntegrationConstants.IdProviders.MicrosoftSchemaClaimName)))
                        {
                            return identity.IsAuthenticated;
                        }
                    }
                    return false;
                });
            });
            opts.AddPolicy(IntegrationConstants.AuthorizationPolicies.FacebookIsAuthenticatedPolicyName, pol =>
            {
                pol.RequireAssertion(cont =>
                {
                    return false;
                });
            });
        });
        return services;
    }

}
