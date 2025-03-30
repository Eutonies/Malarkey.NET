using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Util;
using Malarkey.Abstractions;
using Malarkey.UI.Session;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Malarkey.Abstractions.Authentication;
using Malarkey.UI.Pages.Authenticate;

namespace Malarkey.UI.Components.Authentication;

public partial class IdpSelectionComponent
{
    [Parameter]
    public NavigationManager NavManager { get; set; }

    [CascadingParameter]
    public MalarkeySessionState SessionState { get; set; }

    [Parameter]
    public MalarkeyAuthenticationSession Session { get; set; }

    [Inject]
    public IMalarkeyAuthenticationSessionCache AuthenticationRepo { get; set; }

    private bool IsAuthenticated => SessionState.User != null;

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        await Task.CompletedTask;
        var idProvider = Session.RequestedIdProvider?.ToString();
        if (idProvider != null)
        {
            var provMap = new List<MalarkeyIdentityProvider> {
                MalarkeyIdentityProvider.Microsoft,
                MalarkeyIdentityProvider.Google,
                MalarkeyIdentityProvider.Facebook,
                MalarkeyIdentityProvider.Spotify
            }.Select(_ => (Key: _.ToString().ToLower(), Value: _))
            .ToDictionarySafe(_ => _.Key, _ => _.Value);
            if (provMap.TryGetValue(idProvider.ToLower().Trim(), out var prov))
            {
                GoTo(prov);
            }
        }
    }


    public void OnMicrosoftClick(MouseEventArgs e) => OnClick(MalarkeyIdentityProvider.Microsoft);

    public void OnGoogleClick(MouseEventArgs e) => OnClick(MalarkeyIdentityProvider.Google);

    public void OnFacebookClick(MouseEventArgs e) => OnClick(MalarkeyIdentityProvider.Facebook);

    public void OnSpotifyClick(MouseEventArgs e) => OnClick(MalarkeyIdentityProvider.Spotify);

    private void OnClick(MalarkeyIdentityProvider provider) =>
        _ = Task.Run(async () =>
        {
            await AuthenticationRepo.UpdateSession(Session.SessionId, provider);
            GoTo(provider);
        });

    private void GoTo(MalarkeyIdentityProvider provider) =>
        NavManager.NavigateTo(ChallengePage.BuildChallengeUrl(Session), forceLoad: true);



}
