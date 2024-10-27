using Malarkey.Integration;
using Malarkey.Integration.Microsoft;
using Microsoft.Identity.Web;

namespace Malarkey.UI;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.json");
        builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);
        return builder;
    }


    public static WebApplicationBuilder AddUiServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddMicrosoftIdentityConsentHandler();
        //builder.Services.AddAppEntraIdIdentityProvider(builder.Configuration);
        //builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration.GetSection(MicrosoftEntraIdConstants.AzureConfigurationName));
        builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration.GetSection("Integration:Microsoft"));
        builder.Services.AddAuthenticatedAuthorizationPolicy();
        builder.Services.AddAntiforgery();
        return builder;
    }
}
