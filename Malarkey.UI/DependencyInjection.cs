using Malarkey.Application;
using Malarkey.Application.Security;
using Malarkey.API;
using Malarkey.UI.Pages;
using Malarkey.Security;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Malarkey.Persistence;
using Malarkey.Integration;

namespace Malarkey.UI;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.json");
        builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);
        builder.Configuration.AddEnvironmentVariables();
        builder.AddApplicationConfiguration();
        builder.AddPersistenceConfiguration();
        builder.AddIntegrationConfiguration();
        return builder;
    }


    public static WebApplicationBuilder AddUiServices(this WebApplicationBuilder builder)
    {
        IdentityModelEventSource.ShowPII = true;
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddMicrosoftIdentityConsentHandler();
        builder.Services.AddRazorPages()
            .AddMvcOptions(_ => { });
        builder.AddIntegrationServices();
        builder.AddApplication();
        builder.AddPersistence();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddAuthorization(opts =>
        {
            opts.RegisterIsAuthenticatedPolicy();
        });
        builder.Services.AddAntiforgery();
        builder.Services.AddCascadingAuthenticationState();
        builder.AddSecurity();
        return builder;
    }

    public static WebApplication UseUiServices(this WebApplication app)
    {
        app.UseRouting();
        app.UseStaticFiles();
        app.UseIntegration();
        app.UseAntiforgery();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        app.MapRazorPages();

        return app;
    }

}
