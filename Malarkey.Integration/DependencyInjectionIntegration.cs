using Malarkey.Abstractions;
using Malarkey.Integration.Authentication;
using Malarkey.Integration.Authentication.OAuthFlowHandlers;
using Malarkey.Integration.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpyOff.Infrastructure.Tracks;
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
        builder.Services.AddAuthentication(MalarkeyConstants.MalarkeyAuthenticationScheme)
            .AddScheme<MalarkeyServerAuthenticationHandlerOptions, MalarkeyServerAuthenticationHandler>(
               authenticationScheme: MalarkeyConstants.MalarkeyAuthenticationScheme,
               configureOptions: opts =>
               {
                   opts.AccessDeniedUrl = conf.AccessDeniedPath;
                   opts.PublicKey = conf.PublicKey;
               });
        builder.Services.AddScoped<IMalarkeyOAuthFlowHandler, MalarkeyMicrosoftOAuthFlowHandler>();
        builder.Services.AddScoped<IMalarkeyOAuthFlowHandler, MalarkeyGoogleOAuthFlowHandler>();
        builder.Services.AddScoped<IMalarkeyOAuthFlowHandler, MalarkeyFacebookOAuthFlowHandler>();
        builder.Services.AddScoped<IMalarkeyOAuthFlowHandler, MalarkeySpotifyOAuthFlowHandler>();
        builder.Services.AddHttpClients();
        return builder;
    }


    public static WebApplication UseIntegration(this WebApplication app)
    {
        var conf = app.Configuration.IntegrationConfig();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet(
            conf.RedirectPath, 
            async ([FromServices] MalarkeyServerAuthenticationHandler authHandler, 
                   [FromServices] NavigationManager navManager,
                    HttpRequest request) => await authHandler.HandleCallback(navManager, request)
        );
        app.MapPost(
            conf.RedirectPath, 
            async ([FromServices] MalarkeyServerAuthenticationHandler authHandler, 
                   [FromServices] NavigationManager navManager,
                    HttpRequest request) => await authHandler.HandleCallback(navManager, request)
        );
        return app;
    }

    private static async Task HandleCallback(this MalarkeyServerAuthenticationHandler authHandler, NavigationManager navManager, HttpRequest req) 
    {
        var result = await authHandler.HandleCallback(req);
        if(result is MalarkeyAuthenticationSuccessHttpResult succ){
            navManager.NavigateTo(succ.ForwardLocation, forceLoad: true);
        }
        else if(result is BadRequest<string> badReq) {
            throw new Exception(badReq.Value);
        }
        else {
            throw new Exception("Did not complete authentication flow correctly");
        }
    }


    private static MalarkeyIntegrationConfiguration IntegrationConfig(this IConfiguration conf)
    {
        var returnee = new MalarkeyIntegrationConfiguration();
        conf.Bind(MalarkeyIntegrationConfiguration.ConfigurationElementName, returnee);
        return returnee;
    }

    private static IServiceCollection AddHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient();

        return services;
    }


}
