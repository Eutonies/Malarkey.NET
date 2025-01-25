using Malarkey.Abstractions;
using Malarkey.Abstractions.Profile;
using Malarkey.Application;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Util;
using Malarkey.Integration;
using Malarkey.UI.Session;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Malarkey.Application.Authentication;
using Malarkey.Integration.Authentication;
using Microsoft.Extensions.Options;
using Malarkey.Integration.Configuration;

namespace Malarkey.UI.Pages.Authenticate;

public partial class AuthenticatePage
{
    [Inject]
    public NavigationManager NavManager { get; set; }

    [Inject]
    public IHttpContextAccessor ContextAccessor { get; set; }

    [Inject]
    public IMalarkeyAuthenticationSessionRepository AuthenticationSessionRepo { get; set; }

    [Inject]
    public IOptions<MalarkeyIntegrationConfiguration> IntegrationConfiguration { get; set; }

    [CascadingParameter]
    public MalarkeySessionState SessionState { get; set; }

    [SupplyParameterFromQuery(Name = MalarkeyConstants.AuthenticationRequestQueryParameters.SendToName)]
    [Parameter]
    public string? SendTo { get; set; }

    [SupplyParameterFromQuery(Name = MalarkeyConstants.AuthenticationRequestQueryParameters.IdProviderName)]
    [Parameter]
    public string? IdProvider { get; set; }

    [SupplyParameterFromQuery(Name = MalarkeyConstants.AuthenticationRequestQueryParameters.ScopesName)]
    [Parameter]
    public string? Scopes { get; set; }

    [SupplyParameterFromQuery(Name = MalarkeyConstants.AuthenticationRequestQueryParameters.SendToStateName)]
    [Parameter]
    public string? SendToState { get; set; }

    [SupplyParameterFromQuery(Name = MalarkeyConstants.AuthenticationRequestQueryParameters.ExistingProfileIdName)]
    [Parameter]
    public string? ProfileId { get; set; }

    [SupplyParameterFromQuery(Name = MalarkeyConstants.AuthenticationRequestQueryParameters.AlwaysChallengeName)]
    [Parameter]
    public string? AlwaysChallenge { get; set; }

    private MalarkeyAuthenticationSession? _authenticationSession;


    protected override async Task OnParametersSetAsync()
    {
        if (_authenticationSession == null)
        {
            var audience = IntegrationConfiguration.Value
                .PublicKey.CleanCertificate();
            var context = ContextAccessor.HttpContext!;
            var sess = context.Request.ResolveSession(audience);
            _authenticationSession = await AuthenticationSessionRepo.InitiateSession(sess);
            await InvokeAsync(StateHasChanged);
        }

    }



}
