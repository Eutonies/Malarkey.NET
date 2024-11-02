using Malarkey.Application.ProfileImport;
using Malarkey.Domain.ProfileImport;
using Malarkey.Integration.Microsoft.Configuration;
using Malarkey.Integration.Microsoft.ProfileImport;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
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


    public static IServiceCollection AddApiEntraIdIdentityProvider(this IServiceCollection services, IConfiguration config)
    {
        var withAuth = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
        var withIdentityWebApi = withAuth.AddMicrosoftIdentityWebApi(config.GetSection(MicrosoftEntraIdConstants.MicrosoftConfigurationName));
        return services;
    }

    public static AuthenticationBuilder AddMicrsoftIdentityProvider(this AuthenticationBuilder builder, IConfiguration config)
    {
        var microConf = config.Parse();
        var azConf = microConf.AzureAd;
        builder.AddMicrosoftAccount(IntegrationConstants.IdProviders.MicrosoftAuthenticationSchemeName, opts =>
        {
            opts.ClientId = azConf.ClientId;
            opts.ClientSecret = azConf.ClientSecret;
            opts.CallbackPath = azConf.CallbackPath;
            opts.CorrelationCookie.Name = "microsoft-auth";
            opts.TokenEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
            opts.AuthorizationEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize";
        });
        var azureAdConfig = config.GetSection(MicrosoftEntraIdConstants.AzureAdConfigurationName);
        var withIdentityWebApp = builder.AddMicrosoftIdentityWebApp(azureAdConfig, cookieScheme: "azure-ad-cookies");
        var withDownstream = withIdentityWebApp.EnableTokenAcquisitionToCallDownstreamApi(MicrosoftEntraIdConstants.GraphScopes.All);
        var withGraph = withDownstream.AddMicrosoftGraph(config.GetSection(MicrosoftEntraIdConstants.GraphApiConfigurationName));
        var withCache = withGraph.AddInMemoryTokenCaches();
        builder.Services.AddServices();
        return builder;
    }


    public static IServiceCollection AddAppEntraIdIdentityProvider(this IServiceCollection services, IConfiguration config)
    {
        var withAuth = services.AddAuthentication(opts =>
        {
        });
        var azureAdConfig = config.GetSection(MicrosoftEntraIdConstants.AzureAdConfigurationName);
        var withIdentityWebApp = withAuth.AddMicrosoftIdentityWebApp(azureAdConfig);
            
            /*services.AddMicrosoftIdentityWebAppAuthentication(
            configuration: config.GetSection(MicrosoftEntraIdConstants.MicrosoftConfigurationName),
            configSectionName: MicrosoftEntraIdConstants.AzureAdElementName
        );*/
        var withDownstream = withIdentityWebApp.EnableTokenAcquisitionToCallDownstreamApi(MicrosoftEntraIdConstants.GraphScopes.All);
        var withGraph = withDownstream.AddMicrosoftGraph(config.GetSection(MicrosoftEntraIdConstants.GraphApiConfigurationName));
        var withCache = withGraph.AddInMemoryTokenCaches();
        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IProfileImporter<MicrosoftImportProfile>, MicrosoftProfileImporter>();
        return services;
    }

    private static MicrosoftIntegrationConfiguration Parse(this IConfiguration config)
    {
        var returnee = new MicrosoftIntegrationConfiguration();
        config.Bind(MicrosoftIntegrationConfiguration.ConfigurationElementName, returnee);
        return returnee;
    }


}
