using Azure.Identity;
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
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Kiota.Abstractions.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Microsoft;
public static class DependencyInjection
{

    public static AuthenticationBuilder AddMicrsoftIdentityProvider(this AuthenticationBuilder builder, IConfiguration config)
    {
        var microConf = config.Parse();
        var azConf = microConf.AzureAd;
        var graphConf = microConf.DownstreamApis.MicrosoftGraph;
        builder.AddMicrosoftAccount(IntegrationConstants.IdProviders.MicrosoftAuthenticationSchemeName, opts =>
        {
            opts.ClientId = azConf.ClientId;
            opts.ClientSecret = azConf.ClientSecret;
            opts.CallbackPath = azConf.CallbackPath;
            opts.CorrelationCookie.Name = "microsoft-auth";
            opts.TokenEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
            opts.AuthorizationEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize";
        });
        var azureAdConfig = config.GetSection(MicrosoftEntraIdConstants.MicrosoftConfigurationName);
        var withIdentityWebApp = builder.AddMicrosoftIdentityWebApp(azureAdConfig, 
            configSectionName: "AzureAd",
            cookieScheme: IntegrationConstants.IdProviders.MicrosoftIdCookieName);
        var withDownstream = withIdentityWebApp.EnableTokenAcquisitionToCallDownstreamApi(
            opts => { 
                opts.ClientId = azConf.ClientId;
                opts.ClientCapabilities = azConf.ClientCapabilities;
                opts.TenantId = azConf.TenantId;
                opts.Instance = azConf.Instance;
                opts.ClientSecret = azConf.ClientSecret;
            },
            MicrosoftEntraIdConstants.GraphScopes.All);
        var withCache = withDownstream.AddInMemoryTokenCaches();
        var withGraph = withCache.AddMicrosoftGraph((IAuthenticationProvider provider) =>
        {
            var sharedTokenCacheCredential = new SharedTokenCacheCredential();
            var client = new GraphServiceClient(sharedTokenCacheCredential, scopes: graphConf.Scopes, baseUrl: graphConf.BaseUrl);
            return client;
        }, graphConf.Scopes);
        builder.Services.AddServices();
        return builder;
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
