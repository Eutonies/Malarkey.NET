using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Util;
using Malarkey.Abstractions;
using Malarkey.UI.Session;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using System.Text;
using Malarkey.Abstractions.Authentication;

namespace Malarkey.UI.Components.Authentication;

public partial class IdpSelectionComponent
{
    [Parameter]
    public NavigationManager NavManager { get; set; }

    [CascadingParameter]
    public MalarkeySessionState SessionState { get; set; }

    [Parameter]
    public MalarkeyAuthenticationSession Session { get; set; }

    [Parameter]
    public string? AlwaysChallenge { get; set; }



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


    public void OnMicrosoftClick(MouseEventArgs e) => GoTo(MalarkeyIdentityProvider.Microsoft);

    public void OnGoogleClick(MouseEventArgs e) => GoTo(MalarkeyIdentityProvider.Google);

    public void OnFacebookClick(MouseEventArgs e) => GoTo(MalarkeyIdentityProvider.Facebook);

    public void OnSpotifyClick(MouseEventArgs e) => GoTo(MalarkeyIdentityProvider.Spotify);

    private void GoTo(MalarkeyIdentityProvider provider) =>
        NavManager.NavigateTo(BuildChallengeUrl(provider), forceLoad: true);


    private string BuildChallengeUrl(MalarkeyIdentityProvider provider)
    {
        var returnee = new StringBuilder($"challenge?{MalarkeyConstants.AuthenticationRequestQueryParameters.IdProviderName}={provider.ToString()}");
        returnee.Append($"&{MalarkeyConstants.AuthenticationRequestQueryParameters.SendToName}={Session.SendTo.UrlEncoded()}");
        if (Session.RequestedScopes != null)
            returnee.Append($"&{MalarkeyConstants.AuthenticationRequestQueryParameters.ScopesName}={Session.RequestedScopes.MakeString(" ").UrlEncoded()}");
        if (Session.RequestState != null)
            returnee.Append($"&{MalarkeyConstants.AuthenticationRequestQueryParameters.SendToStateName}={Session.RequestState.UrlEncoded()}");
        if (Session.ExistingProfileId != null)
            returnee.Append($"&{MalarkeyConstants.AuthenticationRequestQueryParameters.ExistingProfileIdName}={Session.ExistingProfileId.Value.ToString().UrlEncoded()}");
        if (AlwaysChallenge != null)
            returnee.Append($"&{MalarkeyConstants.AuthenticationRequestQueryParameters.AlwaysChallengeName}={AlwaysChallenge}");

        return returnee.ToString();
    }


}
