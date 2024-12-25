using Malarkey.Application;
using Malarkey.Domain.Authentication;
using Malarkey.Domain.Util;
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

    [SupplyParameterFromQuery(Name = "forwarder")]
    [Parameter]
    public string? Forwarder { get; set; }

    private bool IsAuthenticated => SessionState.User != null;

    protected async override Task OnInitializedAsync()
    {
        await Task.CompletedTask;
    }

    public void OnMicrosoftClick(MouseEventArgs e) => GoTo(MalarkeyOAuthIdentityProvider.Microsoft);

    public void OnGoogleClick(MouseEventArgs e) => GoTo(MalarkeyOAuthIdentityProvider.Google);

    public void OnFacebookClick(MouseEventArgs e) => GoTo(MalarkeyOAuthIdentityProvider.Facebook);

    public void OnSpotifyClick(MouseEventArgs e) => GoTo(MalarkeyOAuthIdentityProvider.Spotify);

    private void GoTo(MalarkeyOAuthIdentityProvider provider) =>
        NavManager.NavigateTo(BuildChallengeUrl(provider), forceLoad: true);


    private string BuildChallengeUrl(MalarkeyOAuthIdentityProvider provider)
    {
        var returnee = $"challenge?{IntegrationConstants.IdProviderHeaderName}={provider.ToString()}";
        if(Forwarder != null)
        {
            returnee = returnee + $"&{IntegrationConstants.ForwarderQueryParameterName}={Forwarder.UrlEncoded()}";
        }
        return returnee;
    }

}
