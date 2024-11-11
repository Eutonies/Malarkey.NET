using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Malarkey.Application.Security;
using Malarkey.Security.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Malarkey.Security;
public static class DependencyInjectionSecurity
{

    public static WebApplicationBuilder AddSecurityConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<SecurityConfiguration>(builder.Configuration.GetSection(SecurityConfiguration.ConfigurationElementName));
        return builder;
    }




    public static WebApplicationBuilder AddSecurity(this WebApplicationBuilder builder)
    {

        builder.Services.AddSingleton<IMalarkeyTokenHandler, MalarkeyTokenHandler>();
        return builder;
    }

}
