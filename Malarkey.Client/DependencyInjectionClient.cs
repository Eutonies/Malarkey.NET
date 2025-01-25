using Malarkey.Abstractions;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Util;
using Malarkey.Client.Authentication;
using Malarkey.Client.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Client;
public static class DependencyInjectionClient
{

    public static WebApplicationBuilder AddMalarkeyClientConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<MalarkeyClientConfiguration>(builder.Configuration.GetSection(MalarkeyClientConfiguration.ConfigurationElementName));
        return builder;
    }

    public static WebApplicationBuilder AddMalarkeyClientAuthentication(this WebApplicationBuilder builder, int requestCacheCapacity = 3_000)
    {
        builder.Services.AddMalarkeyCaching<string, MalarkeyAuthenticationSession>(requestCacheCapacity);
        builder.Services.AddScoped<IMalarkeyClientAuthenticatedCallback, MalarkeyClientAuthenticationHandler>();
        builder.Services.AddAuthentication(MalarkeyConstants.MalarkeyAuthenticationScheme)
            .AddScheme<MalarkeyClientAuthenticationSchemeOptions, MalarkeyClientAuthenticationHandler>(
               authenticationScheme: MalarkeyConstants.MalarkeyAuthenticationScheme,
               configureOptions: null);
        return builder;
    }

    public static WebApplication UseMalarkeyClientAuthentication(this WebApplication app)
    {
        var conf = app.Configuration.ClientConfig();
        app.UseMiddleware<MalarkeyClientAuthenticationAttributeMiddleware>();
        app.UseAuthentication();
        app.MapPost(conf.ClientAuthenticatedPathToUse, (
            [FromServices] IMalarkeyClientAuthenticatedCallback callback,
            HttpRequest request
            ) => callback.HandleCallback(request)
        );

        return app;
    }


    private static MalarkeyClientConfiguration ClientConfig(this IConfiguration conf)
    {
        var returnee = new MalarkeyClientConfiguration();
        conf.Bind(MalarkeyClientConfiguration.ConfigurationElementName, returnee);
        return returnee;
    }

}
