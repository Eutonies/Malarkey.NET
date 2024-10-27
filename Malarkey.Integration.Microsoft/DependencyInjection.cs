using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Microsoft;
public static class DependencyInjection
{

    public static IServiceCollection AddEntraIdIdentityProvider(this IServiceCollection services, IConfiguration config)
    {
        var withAuth = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
        var withIdentityWebApi = withAuth.AddMicrosoftIdentityWebApi(config.GetSection(MicrosoftEntraIdConstants.AzureConfigurationName));

        return services;
    }


}
