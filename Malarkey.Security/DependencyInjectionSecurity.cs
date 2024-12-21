using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Malarkey.Application;
using Malarkey.Application.Security;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Malarkey.Security;
public static class DependencyInjectionSecurity
{


    public static WebApplicationBuilder AddSecurity(this WebApplicationBuilder builder)
    {

        builder.Services.AddSingleton<IMalarkeyTokenHandler, MalarkeyTokenHandler>();
        return builder;
    }

    public static AuthenticationBuilder AddMalarkeyToken(this AuthenticationBuilder builder)
    {
        return builder;
    }


}
