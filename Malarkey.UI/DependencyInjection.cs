using Malarkey.Integration;
using Malarkey.Integration.Facebook;
using Malarkey.Integration.Google;
using Malarkey.Integration.Microsoft;
using Malarkey.UI.Pages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Logging;

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
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddAuthenticatedAuthorizationPolicy();
        builder.Services.AddAntiforgery();
        builder.Services.AddCascadingAuthenticationState();
        return builder;
    }

    public static WebApplication UseUiServices(this WebApplication app)
    {
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        app.MapRazorPages();

        /*var middware = app.Services.GetRequiredService<OpenIdConnectMiddlewareDiagnostics>();
        middware.Subscribe(new OpenIdConnectEvents()
        {
            OnTokenValidated = async ctx =>
            {
                var resp = ctx.TokenEndpointResponse;
            }


        });*/


        return app;
    }

    private static string? SelectScheme(HttpContext cont)
    {
        return IntegrationConstants.IdProviders.MicrosoftAuthenticationSchemeName;
    }

}
