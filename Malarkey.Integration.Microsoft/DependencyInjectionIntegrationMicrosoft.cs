using Azure.Identity;
using Malarkey.Application.ProfileImport;
using Malarkey.Domain.ProfileImport;
using Malarkey.Integration.Microsoft.Configuration;
using Malarkey.Integration.Microsoft.ProfileImport;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Malarkey.Integration.Microsoft;
public static class DependencyInjectionIntegrationMicrosoft
{

    /*public static AuthenticationBuilder AddMicrsoftIdentityProvider(this AuthenticationBuilder builder, IConfiguration config)
    {
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
        var microConf = config.Parse();
        var azConf = microConf.AzureAd;
        var graphConf = microConf.DownstreamApis.MicrosoftGraph;
        var azureAdConfig = config.GetSection(MicrosoftEntraIdConstants.MicrosoftConfigurationName);
        var withIdentityWebApp = builder.AddMicrosoftIdentityWebApp(azureAdConfig, 
            configSectionName: "AzureAd",
            openIdConnectScheme: IntegrationConstants.IdProviders.MicrosoftAuthenticationSchemeName,
            cookieScheme: IntegrationConstants.IdProviders.MicrosoftIdCookieName);
        var withDownstream = withIdentityWebApp.EnableTokenAcquisitionToCallDownstreamApi(MicrosoftEntraIdConstants.GraphScopes.All);
        var withCache = withDownstream.AddInMemoryTokenCaches();
        var withGraph = withCache.AddMicrosoftGraph((IAuthenticationProvider prov) => 
        {
            var returnee = new GraphServiceClient(prov);
            return returnee;
        }, graphConf.Scopes);
        builder.Services.Configure<GraphServiceClientOptions>(opts =>
        {
            var tessa = opts;
        });
        builder.Services.AddServices();
        return builder;
    }*/

    public static WebApplicationBuilder AddMicrsoftIdentityProvider(this WebApplicationBuilder builder)
    {
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        var config = builder.Configuration;
        var microConf = config.Parse();
        var azConf = microConf.AzureAd;
        var graphConf = microConf.DownstreamApis.MicrosoftGraph;
        var azureAdConfig = config.GetSection(MicrosoftEntraIdConstants.MicrosoftConfigurationName);
        var withAuth = builder.Services
            .AddAuthentication();
        var withIdentityWebApp = withAuth
            .AddMicrosoftIdentityWebApp(azureAdConfig,
            configSectionName: "AzureAd",
            openIdConnectScheme: IntegrationConstants.IdProviders.MicrosoftAuthenticationSchemeName,
            cookieScheme: IntegrationConstants.IdProviders.MicrosoftIdCookieName);

        var withDownstream = withIdentityWebApp.EnableTokenAcquisitionToCallDownstreamApi(
            configureConfidentialClientApplicationOptions: opts =>
            {
                opts.EnablePiiLogging = true;
                opts.LegacyCacheCompatibilityEnabled = false;
            },
            []);
        var withCache = withDownstream.AddInMemoryTokenCaches();
        builder.Services.Configure<MicrosoftIntegrationConfiguration>(configureOptions: microConf.WriteTo);
        builder.Services.AddServices();
        return builder;
    }


    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services;
    }

    private static MicrosoftIntegrationConfiguration Parse(this IConfiguration config)
    {
        var returnee = new MicrosoftIntegrationConfiguration();
        config.Bind(MicrosoftIntegrationConfiguration.ConfigurationElementName, returnee);
        return returnee;
    }



}
