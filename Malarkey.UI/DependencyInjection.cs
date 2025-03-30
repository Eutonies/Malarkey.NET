using Malarkey.Application;
using Malarkey.Application.Security;
using Malarkey.API;
using Malarkey.UI.Pages;
using Malarkey.Security;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Malarkey.Persistence;
using Malarkey.Integration;
using Malarkey.UI.Configuration;
using Malarkey.UI.Middleware;
using Malarkey.Integration.Authentication;
using Malarkey.UI.Components.Authentication;
using Malarkey.UI.Pages.Profile;
using Malarkey.Abstractions.Util;
using Malarkey.UI.BackgroundServices;
using Malarkey.UI.Pages.Authenticate;

namespace Malarkey.UI;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.json");
        builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);
        if(Path.Exists("/config"))
           builder.Configuration.AddJsonFile("appsettings.production.json", optional: true);
        builder.Configuration.AddEnvironmentVariables();
        builder.AddApplicationConfiguration();
        builder.AddPersistenceConfiguration();
        builder.AddIntegrationConfiguration();
        builder.Services.Configure<MalarkeyUIConfiguration>(builder.Configuration.GetSection(MalarkeyUIConfiguration.ConfigurationElementName));
        return builder;
    }


    public static WebApplicationBuilder AddUiServices(this WebApplicationBuilder builder)
    {
        IdentityModelEventSource.ShowPII = true;
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
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
        builder.Services.AddHttpLogging();
        builder.Services.AddSingleton<MalarkeySynchronizer>();
        builder.AddApi();
        builder.Services.AddHostedService<CleanupBackgroundService>();

        return builder;
    }

    public static WebApplication UseUiServices(this WebApplication app)
    {
        var uiConf = app.Configuration.UiConf();
        if(!string.IsNullOrWhiteSpace(uiConf.HostingBasePath))
           app.UsePathBase("/" + uiConf.HostingBasePath);
        app.UseMiddleware<MalarkeyRedirectHttpMethodCorrectionMiddleware>();
        app.UseMiddleware<MalarkeyRequestLoggingMiddleware>();   
        app.UseHttpLogging();
        app.MapStaticAssets();
        app.UseApi(uiConf.HostingBasePath);
        app.UseRouting();
        app.MapRazorComponents<App>()
            .DisableAntiforgery()
            .AddInteractiveServerRenderMode();
        app.UseIntegration();
        //app.MapRazorPages();

        return app;
    }


    private static MalarkeyUIConfiguration UiConf(this IConfiguration conf)
    {
        var returnee = new MalarkeyUIConfiguration();
        conf.Bind(MalarkeyUIConfiguration.ConfigurationElementName, returnee);
        return returnee;
    }


}
