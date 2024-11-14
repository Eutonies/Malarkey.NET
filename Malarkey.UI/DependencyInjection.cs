using Malarkey.Integration;
using Malarkey.Integration.Facebook;
using Malarkey.Integration.Google;
using Malarkey.Integration.Microsoft;
using Malarkey.Application;
using Malarkey.Application.Security;
using Malarkey.API;
using Malarkey.UI.Pages;
using Malarkey.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Logging;

namespace Malarkey.UI;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.json");
        builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);
        builder.AddSecurityConfiguration();
        builder.AddFacebookConfiguration();
        return builder;
    }


    public static WebApplicationBuilder AddUiServices(this WebApplicationBuilder builder)
    {
        IdentityModelEventSource.ShowPII = true;
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddMicrosoftIdentityConsentHandler();
        builder.Services.AddRazorPages()
            .AddMvcOptions(_ => { })
            .AddMicrosoftIdentityUI();
        builder.Services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityProvider(builder.Configuration)
            .AddFacebookIdentityProvider(builder.Configuration)
            .AddGoogleIdentityProvider(builder.Configuration)
            .AddJwtBearer()
            .AddCookie();
        builder.AddApplication();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddAuthorization(opts =>
        {
            opts.RegisterIdProviderIsAuthenticatedPolicies();
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
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseApi();

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        app.MapRazorPages();

        return app;
    }

    private static string? SelectScheme(HttpContext cont)
    {
        return IntegrationConstants.IdProviders.MicrosoftAuthenticationSchemeName;
    }

}
