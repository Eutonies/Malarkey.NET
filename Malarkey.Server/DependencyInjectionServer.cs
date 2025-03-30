using Malarkey.Abstractions;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Profile;
using Malarkey.Application.Profile;
using Malarkey.Integration.Authentication;
using Malarkey.Integration.Authentication.OAuthFlowHandlers;
using Malarkey.Integration.Configuration;
using Malarkey.Server.Authentication;
using Malarkey.Server.Profile;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Malarkey.Server;
public static class DependencyInjectionServer
{

    public static WebApplicationBuilder AddServerConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<MalarkeyIntegrationConfiguration>(builder.Configuration.GetSection(MalarkeyServerConfiguration.ConfigurationElementName));
        builder.Services.Configure<MalarkeyServerConfiguration>(builder.Configuration.GetSection(MalarkeyServerConfiguration.ConfigurationElementName));
        return builder;
    }

    public static WebApplicationBuilder AddServerServices(this WebApplicationBuilder builder)
    {
        var conf = builder.Configuration.IntegrationConfig();
        builder.Services.AddSingleton<IMalarkeyServerAuthenticationEventHandler, MalarkeyServerAuthenticationEvents>();
        builder.Services.AddAuthentication(MalarkeyConstants.MalarkeyAuthenticationScheme)
            .AddScheme<MalarkeyIntegrationAuthenticationHandlerOptions, MalarkeyServerAuthenticationHandler>(
               authenticationScheme: MalarkeyConstants.MalarkeyAuthenticationScheme,
               configureOptions: opts =>
               {
                   opts.AccessDeniedUrl = conf.AccessDeniedPath;
                   opts.PublicKey = conf.PublicKey;
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


    public static WebApplication UseMalarkeyServer(this WebApplication app)
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

    private static IServiceCollection AddHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient();

        return services;
    }


}
