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
                    foreach(var ident in cont.User.Identities)
                    {
                        if (ident.Claims.Any(_ => _.Issuer.Contains("login.microsoftonline.com") && ident.IsAuthenticated))
                            return true;

                    }
                    return false;
                });
            });
            opts.AddPolicy(IntegrationConstants.AuthorizationPolicies.FacebookIsAuthenticatedPolicyName, pol =>
            {
                pol.RequireAssertion(cont =>
                {
                    foreach(var ident in cont.User.Identities)
                    {
                        if (ident.AuthenticationType == IntegrationConstants.IdProviders.FacebookAuthenticationSchemeName && ident.IsAuthenticated)
                            return true;
                    }
                    return false;
                });
            });
        });
        return services;
    }

}
