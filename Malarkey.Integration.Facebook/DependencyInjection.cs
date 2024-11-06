using Malarkey.Integration.Facebook.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Facebook;
public static class DependencyInjection
{
    private const string FacebookConfigName = $"{IntegrationConstants.IntegrationConfigurationName}:Facebook";

    public static WebApplicationBuilder AddFacebookConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<FacebookIntegrationConfiguration>(builder.Configuration.GetSection(FacebookConfigName));
        return builder;
    }


    public static WebApplicationBuilder AddFacebookIdentityProvider(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication()
            .AddFacebookIdentityProvider(builder.Configuration);
        return builder;
    }

    public static AuthenticationBuilder AddFacebookIdentityProvider(this AuthenticationBuilder builder, IConfiguration config)
    {
        var conf = config.FacebookConfig();
        builder.AddFacebook(IntegrationConstants.IdProviders.FacebookAuthenticationSchemeName, opts =>
        {
            opts.AppId = conf.Identity.AppId;
            opts.AppSecret = conf.Identity.ClientSecret;
            opts.CallbackPath = conf.Identity.CallbackPath;
            opts.CorrelationCookie.Name = "facebook-auth";
            opts.AccessDeniedPath = "/authentication";
            opts.Scope.Add(FacebookIntegrationConstants.Scopes.Email);
            opts.Scope.Add(FacebookIntegrationConstants.Scopes.PublicProfile);
        });

        return builder;
    }


    public static WebApplicationBuilder AddFacebookIdentityProvider(this WebApplicationBuilder builder, IConfiguration config)
    {
        var conf = config.FacebookConfig();
        builder.Services
            .AddAuthentication()
            .AddFacebook(IntegrationConstants.IdProviders.FacebookAuthenticationSchemeName, opts =>
        {
            opts.AppId = conf.Identity.AppId;
            opts.AppSecret = conf.Identity.ClientSecret;
            opts.CallbackPath = conf.Identity.CallbackPath;
            opts.CorrelationCookie.Name = "facebook-auth";
            opts.AccessDeniedPath = "/authentication";
            opts.Scope.Add(FacebookIntegrationConstants.Scopes.Email);
            opts.Scope.Add(FacebookIntegrationConstants.Scopes.PublicProfile);
        });

        return builder;
    }


    private static FacebookIntegrationConfiguration FacebookConfig(this IConfiguration conf)
    {
        var returnee = new FacebookIntegrationConfiguration();
        conf.Bind(FacebookConfigName,returnee);
        return returnee;
    }


}
