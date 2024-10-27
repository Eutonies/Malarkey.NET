using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;

namespace Malarkey.UI.Pages.Authenticate;

public partial class AuthenticatePage
{
    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; }


    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (authState != null && authState.User?.Identity != null && authState.User.Identity.IsAuthenticated)
        {
            var user = authState.User;
            var identity = user.Identity;
        }

    }


}
