using Malarkey.Integration.Authentication;
using Malarkey.Integration.Authentication.OAuthFlowHandlers;
using Malarkey.Integration.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration;
public static class DependencyInjectionIntegration
{

    public static WebApplicationBuilder AddIntegrationConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<MalarkeyIntegrationConfiguration>(builder.Configuration.GetSection(MalarkeyIntegrationConfiguration.ConfigurationElementName));
        return builder;
    }

    public static WebApplicationBuilder AddIntegrationServices(this WebApplicationBuilder builder)
    {
        var conf = builder.Configuration.IntegrationConfig();
        builder.Services.AddAuthentication(IntegrationConstants.MalarkeyAuthenticationScheme)
            .AddScheme<MalarkeyServerAuthenticationHandlerOptions, MalarkeyServerAuthenticationHandler>(
               authenticationScheme: IntegrationConstants.MalarkeyAuthenticationScheme,
               configureOptions: opts =>
               {
                   opts.AccessDeniedUrl = conf.AccessDeniedUrl;
               });
        builder.Services.AddScoped<IMalarkeyOAuthFlowHandler, MalarkeyMicrosoftOAuthFlowHandler>();
        builder.Services.AddScoped<IMalarkeyOAuthFlowHandler, MalarkeyGoogleOAuthFlowHandler>();
        builder.Services.AddScoped<IMalarkeyOAuthFlowHandler, MalarkeyFacebookOAuthFlowHandler>();
        builder.Services.AddScoped<IMalarkeyOAuthFlowHandler, MalarkeySpotifyOAuthFlowHandler>();
        return builder;
    }


    private static MalarkeyIntegrationConfiguration IntegrationConfig(this IConfiguration conf)
    {
        var returnee = new MalarkeyIntegrationConfiguration();
        conf.Bind(MalarkeyIntegrationConfiguration.ConfigurationElementName, returnee);
        return returnee;
    }



}
