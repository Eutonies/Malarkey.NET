using Malarkey.Abstractions;
using Malarkey.Abstractions.Profile;
using Malarkey.Application;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Util;
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

    [SupplyParameterFromQuery(Name = MalarkeyConstants.AuthenticationRequestQueryParameters.ForwarderStateName)]
    [Parameter]
    public string? ForwarderState { get; set; }

    [SupplyParameterFromQuery(Name = MalarkeyConstants.AuthenticationRequestQueryParameters.ExistingProfileIdName)]
    [Parameter]
    public string? ProfileId { get; set; }


    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        await Task.CompletedTask;

    }


}
