using Malarkey.Integration.Google.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Google;
public static class DependencyInjectionIntegrationGoogle
{

    public static WebApplicationBuilder AddGoogleIdentityProvider(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddAuthentication()
            .AddGoogleIdentityProvider(builder.Configuration);
        return builder;
    }


    public static AuthenticationBuilder AddGoogleIdentityProvider(this AuthenticationBuilder builder, IConfiguration config)
    {
        var googleConf = config.Parse();
        var idConf = googleConf.Identity;
        var withGoogle = builder
            .AddGoogle(
               authenticationScheme: IntegrationConstants.IdProviders.GoogleAuthenticationSchemeName,
               configureOptions: opts =>
               {
                   opts.CallbackPath = idConf.CallbackPath;
                   opts.ClientId = idConf.ClientId;
                   opts.ClientSecret = idConf.ClientSecret;
                   opts.AuthorizationEndpoint = idConf.AuthenticationUri;
                   opts.TokenEndpoint = idConf.TokenUri;
                   foreach (var scop in idConf.Scopes)
                       opts.Scope.Add(scop);
               }
            );
        builder.Services.AddServices();
        return builder;
    }




    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services;
    }

    private static GoogleIntegrationConfiguration Parse(this IConfiguration config)
    {
        var returnee = new GoogleIntegrationConfiguration();
        config.Bind(GoogleIntegrationConstants.ConfigurationElementName, returnee);
        return returnee;
    }



}
