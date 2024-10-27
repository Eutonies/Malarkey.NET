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
            opts.AddPolicy(IntegrationConstants.AuthorizationPolicies.IsAuthenticatedName, pol =>
            {
                pol.RequireAuthenticatedUser();
            });
        });
        return services;
    }

}
