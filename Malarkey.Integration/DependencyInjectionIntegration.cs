﻿using Malarkey.Abstractions;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Profile;
using Malarkey.Application.Authentication;
using Malarkey.Application.Configuration;
using Malarkey.Application.Profile;
using Malarkey.Integration.Authentication;
using Malarkey.Integration.Authentication.OAuthFlowHandlers;
using Malarkey.Integration.Configuration;
using Malarkey.Integration.Profile;
using Microsoft.AspNetCore.Authentication;
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
        var appConf = builder.Configuration.ApplicationConfig();
        builder.Services.AddSingleton<IMalarkeyServerAuthenticationEventHandler, MalarkeyServerAuthenticationEvents>();
        builder.Services.AddAuthentication(MalarkeyConstants.MalarkeyAuthenticationScheme)
            .AddScheme<MalarkeyServerAuthenticationHandlerOptions, MalarkeyServerAuthenticationHandler>(
               authenticationScheme: MalarkeyConstants.MalarkeyAuthenticationScheme,
               configureOptions: opts =>
               {
                   opts.AccessDeniedUrl = conf.AccessDeniedPath;
                   opts.PublicKey = appConf.Certificate.PublicKeyPem;
               });
        builder.Services.AddScoped<IMalarkeyOAuthFlowHandler, MalarkeyMicrosoftOAuthFlowHandler>();
        builder.Services.AddKeyedScoped<IMalarkeyIdentityProviderTokenRefresher, MalarkeyMicrosoftOAuthFlowHandler>(MalarkeyIdentityProvider.Microsoft);
        builder.Services.AddScoped<IMalarkeyOAuthFlowHandler, MalarkeyGoogleOAuthFlowHandler>();
        builder.Services.AddKeyedScoped<IMalarkeyIdentityProviderTokenRefresher, MalarkeyGoogleOAuthFlowHandler>(MalarkeyIdentityProvider.Google);
        builder.Services.AddScoped<IMalarkeyOAuthFlowHandler, MalarkeyFacebookOAuthFlowHandler>();
        builder.Services.AddKeyedScoped<IMalarkeyIdentityProviderTokenRefresher, MalarkeyFacebookOAuthFlowHandler>(MalarkeyIdentityProvider.Facebook);
        builder.Services.AddScoped<IMalarkeyOAuthFlowHandler, MalarkeySpotifyOAuthFlowHandler>();
        builder.Services.AddKeyedScoped<IMalarkeyIdentityProviderTokenRefresher, MalarkeySpotifyOAuthFlowHandler>(MalarkeyIdentityProvider.Spotify);
        builder.Services.AddSingleton<IVerificationEmailSender, VerificationEmailSender>();
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
                    HttpRequest request) => await authHandler.HandleCallback(request)
        );
        app.MapPost(
            conf.RedirectPath, 
            async ([FromServices] MalarkeyServerAuthenticationHandler authHandler, 
                    HttpRequest request) => await authHandler.HandleCallback(request)
        );
        return app;
    }



    private static MalarkeyIntegrationConfiguration IntegrationConfig(this IConfiguration conf)
    {
        var returnee = new MalarkeyIntegrationConfiguration();
        conf.Bind(MalarkeyIntegrationConfiguration.ConfigurationElementName, returnee);
        return returnee;
    }
    private static MalarkeyApplicationConfiguration ApplicationConfig(this IConfiguration conf)
    {
        var returnee = new MalarkeyApplicationConfiguration();
        conf.Bind(MalarkeyApplicationConfiguration.ConfigurationElementName, returnee);
        return returnee;
    }


    private static IServiceCollection AddHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient();

        return services;
    }


}
