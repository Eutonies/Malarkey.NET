using Malarkey.Integration;
using Malarkey.Integration.Facebook;
using Malarkey.Integration.Microsoft;
using Malarkey.UI.Pages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace Malarkey.UI;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.json");
        builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);
        builder.AddFacebookConfiguration();
        return builder;
    }


    public static WebApplicationBuilder AddUiServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddMicrosoftIdentityConsentHandler();
        builder.Services.AddRazorPages()
            .AddMvcOptions(_ => { })
            .AddMicrosoftIdentityUI();
        builder
            .AddMicrsoftIdentityProvider()
            .AddFacebookIdentityProvider();
        builder.Services.AddAuthenticatedAuthorizationPolicy();
        builder.Services.AddAntiforgery();
        builder.Services.AddCascadingAuthenticationState();
        return builder;
    }

    public static WebApplication UseUiServices(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        app.MapRazorPages();

        return app;
    }

}
