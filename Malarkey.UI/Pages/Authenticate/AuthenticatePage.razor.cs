using Malarkey.Application;
using Malarkey.Domain.Authentication;
using Malarkey.Integration;
using Malarkey.UI.Session;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Malarkey.UI.Pages.Authenticate;

public partial class AuthenticatePage
{
    [Inject]
    public NavigationManager NavManager { get; set; }

    [Inject]
    public IHttpContextAccessor ContextAccessor { get; set; }

    [CascadingParameter]
    public MalarkeySessionState SessionState { get; set; }

    [SupplyParameterFromForm(Name = "redirect-url")]
    public string? RedirectUri { get; set; }

    private bool IsAuthenticated => SessionState.User != null;

    protected async override Task OnInitializedAsync()
    {
        // https://localhost/authenticate?redirect=http%3A%2F%2Flocalhost%3A8080%2Fmcdonalds
        var context = ContextAccessor.HttpContext;
        var parameters = context!.Request.Query;
        var url = RedirectUri;
        if (parameters.TryGetValue("redirect", out var redirUrl))
            url = redirUrl.ToString();
        if(url != null)
        {
            context!.Response.Cookies.Append(MalarkeyApplicationConstants.MalarkeyRedirectUrlCookie, url);
        }

    }

    public void OnMicrosoftClick(MouseEventArgs e)
    {
        NavManager.NavigateTo($"challenge?{IntegrationConstants.IdProviderHeaderName}={MalarkeyOAuthIdentityProvider.Microsoft.ToString()}", forceLoad: true);
    }

    public void OnGoogleClick(MouseEventArgs e)
    {
        NavManager.NavigateTo($"challenge?{IntegrationConstants.IdProviderHeaderName}={MalarkeyOAuthIdentityProvider.Google.ToString()}", forceLoad: true);
    }

    public void OnFacebookClick(MouseEventArgs e)
    {
        NavManager.NavigateTo($"challenge?{IntegrationConstants.IdProviderHeaderName}={MalarkeyOAuthIdentityProvider.Facebook.ToString()}", forceLoad: true);
    }

    public void OnSpotifyClick(MouseEventArgs e)
    {
        NavManager.NavigateTo($"challenge?{IntegrationConstants.IdProviderHeaderName}={MalarkeyOAuthIdentityProvider.Spotify.ToString()}", forceLoad: true);
    }


}
