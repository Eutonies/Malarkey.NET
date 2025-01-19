using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Util;
using Malarkey.Abstractions;
using Malarkey.UI.Session;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace Malarkey.UI.Components.Authentication;

public partial class IdpSelectionComponent
{
    [Parameter]
    public NavigationManager NavManager { get; set; }

    [CascadingParameter]
    public MalarkeySessionState SessionState { get; set; }

    [Parameter]
    public string? Forwarder { get; set; }

    [Parameter]
    public string? IdProvider { get; set; }

    [Parameter]
    public string? Scopes { get; set; }

    [Parameter]
    public string? ForwarderState { get; set; }

    [Parameter]
    public string? ExistingProfileId { get; set; }


    private bool IsAuthenticated => SessionState.User != null;

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        await Task.CompletedTask;
        if (IdProvider != null)
        {
            var provMap = new List<MalarkeyIdentityProvider> {
                MalarkeyIdentityProvider.Microsoft,
                MalarkeyIdentityProvider.Google,
                MalarkeyIdentityProvider.Facebook,
                MalarkeyIdentityProvider.Spotify
            }.Select(_ => (Key: _.ToString().ToLower(), Value: _))
            .ToDictionarySafe(_ => _.Key, _ => _.Value);
            if (provMap.TryGetValue(IdProvider.ToLower().Trim(), out var prov))
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
        if (Forwarder != null)
            returnee.Append($"&{MalarkeyConstants.AuthenticationRequestQueryParameters.ForwarderName}={Forwarder.UrlEncoded()}");
        if (Scopes != null)
            returnee.Append($"&{MalarkeyConstants.AuthenticationRequestQueryParameters.ScopesName}={Scopes.UrlEncoded()}");
        if (ForwarderState != null)
            returnee.Append($"&{MalarkeyConstants.AuthenticationRequestQueryParameters.ForwarderStateName}={ForwarderState.UrlEncoded()}");
        if (ExistingProfileId != null)
            returnee.Append($"&{MalarkeyConstants.AuthenticationRequestQueryParameters.ExistingProfileIdName}={ExistingProfileId.UrlEncoded()}");
        return returnee.ToString();
    }


}
