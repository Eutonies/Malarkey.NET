using Malarkey.Abstractions;
using Malarkey.Abstractions.Profile;
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

    [SupplyParameterFromQuery(Name = MalarkeyConstants.AuthenticationRequestQueryParameters.ForwarderName)]
    [Parameter]
    public string? Forwarder { get; set; }

    [SupplyParameterFromQuery(Name = MalarkeyConstants.AuthenticationRequestQueryParameters.IdProviderName)]
    [Parameter]
    public string? IdProvider { get; set; }

    [SupplyParameterFromQuery(Name = MalarkeyConstants.AuthenticationRequestQueryParameters.ScopesName)]
    [Parameter]
    public string? Scopes { get; set; }



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
        var returnee = $"challenge?{IntegrationConstants.IdProviderHeaderName}={provider.ToString()}";
        if(Forwarder != null)
        {
            returnee = returnee + $"&{IntegrationConstants.ForwarderQueryParameterName}={Forwarder.UrlEncoded()}";
        }
        if(Scopes != null)
        {
            returnee = returnee + $"&{IntegrationConstants.ScopesQueryParameterName}={Scopes.UrlEncoded()}";
        }
        return returnee;
    }

}
