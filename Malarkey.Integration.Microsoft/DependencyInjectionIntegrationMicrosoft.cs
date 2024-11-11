using Azure.Identity;
using Malarkey.Application.ProfileImport;
using Malarkey.Domain.ProfileImport;
using Malarkey.Integration.Microsoft.Configuration;
using Malarkey.Integration.Microsoft.ProfileImport;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

    public static AuthenticationBuilder AddMicrosoftIdentityProvider(this AuthenticationBuilder builder, IConfiguration config)
    {
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        var microConf = config.Parse();
        var azConf = microConf.AzureAd;
        var azureAdConfig = config.GetSection(MicrosoftEntraIdConstants.MicrosoftConfigurationName);
        var withIdentityWebApp = builder
            .AddMicrosoftIdentityWebApp(azureAdConfig,
                configSectionName: "AzureAd",
                openIdConnectScheme: IntegrationConstants.IdProviders.MicrosoftAuthenticationSchemeName,
                cookieScheme: IntegrationConstants.IdProviders.MicrosoftIdCookieName,
                subscribeToOpenIdConnectMiddlewareDiagnosticsEvents: true);

        var acqOpts = new TokenAcquisitionOptions { };
        var withDownstream = withIdentityWebApp.EnableTokenAcquisitionToCallDownstreamApi(
            configureConfidentialClientApplicationOptions: opts =>
            {
                opts.EnablePiiLogging = true;
                opts.LegacyCacheCompatibilityEnabled = false;
            },            
            ["openid", "profile", "offline_access"]);
        builder.Services.AddMicrosoftIdentityConsentHandler();
        var withCache = withDownstream.AddInMemoryTokenCaches();
        builder.Services.Configure<MicrosoftIntegrationConfiguration>(configureOptions: microConf.WriteTo);
        builder.Services.AddServices();
        return builder;
    }
    public static AuthenticationBuilder AddMicrosoftIdentityProviderYO(this AuthenticationBuilder builder, IConfiguration config)
    {
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        var microConf = config.Parse();
        var azConf = microConf.AzureAd;
        var graphConf = microConf.DownstreamApis.MicrosoftGraph;
        var azureAdConfig = config.GetSection(MicrosoftEntraIdConstants.MicrosoftConfigurationName);
        var withMicrosoft = builder
            .AddMicrosoftAccount(
               authenticationScheme: IntegrationConstants.IdProviders.MicrosoftAuthenticationSchemeName,
               opts =>
               {
                   opts.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                   opts.ClientId = azConf.ClientId;
                   opts.ClientSecret = azConf.ClientSecret;
                   opts.CallbackPath = azConf.CallbackPath;
                   opts.AuthorizationEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize";
                   opts.TokenEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
               });
        builder.Services.AddMicrosoftIdentityConsentHandler();

        builder.Services.Configure<MicrosoftIntegrationConfiguration>(configureOptions: microConf.WriteTo);
        builder.Services.AddServices();
        return builder;
    }



    public static WebApplicationBuilder AddMicrosoftIdentityProvider(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddAuthentication()
            .AddMicrosoftIdentityProvider(builder.Configuration);
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
