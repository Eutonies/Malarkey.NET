using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Malarkey.Application.ProfileImport;
using Malarkey.Domain.ProfileImport;

namespace Malarkey.UI.Pages.Authenticate;

public partial class AuthenticateGooglePage
{
    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    private string? _profilePhoto;
    private IReadOnlyCollection<ImportImage> _images = [];

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
