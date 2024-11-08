using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration;
public static class DependencyInjectionIntegration
{

    public static IServiceCollection AddAuthenticatedAuthorizationPolicy(this IServiceCollection services)
    {
        services.AddAuthorization(opts =>
        {
            opts.AddPolicy(IntegrationConstants.AuthorizationPolicies.MicrosoftIsAuthenticatedPolicyName, pol =>
            {
                pol.RequireAssertion(cont => cont.User.IsAuthenticatedMicrosoftUser());
;
            });
            opts.AddPolicy(IntegrationConstants.AuthorizationPolicies.FacebookIsAuthenticatedPolicyName, pol =>
            {
                pol.RequireAssertion(cont => cont.User.IsAuthenticatedFacebookUser());
            });
            opts.AddPolicy(IntegrationConstants.AuthorizationPolicies.GoogleIsAuthenticatedPolicyName, pol =>
            {
                pol.RequireAssertion(cont =>
                {
                    var tess = cont.User;
                    return true;
                });
            });
        });
        return services;
    }

}
