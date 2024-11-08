using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Malarkey.Application.ProfileImport;
using Malarkey.Domain.ProfileImport;
using Malarkey.Integration.Microsoft.ProfileImport;

namespace Malarkey.UI.Pages.Authenticate;

public partial class AuthenticateMicrosoftPage
{
    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    [Inject]
    public IServiceScopeFactory ScopeFactory { get; set; }

    [Inject]
    public IHttpContextAccessor ContextAccessor { get; set; }

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
            using var scope = ScopeFactory.CreateScope();
            var profileService = await scope.ServiceProvider.Create(AuthenticationStateProvider, ContextAccessor.HttpContext!);
            if (profileService != null)
            {
                var profile = await profileService.LoadForImport();
                if (profile?.Images != null)
                    _images = profile.Images.ToList();
                await InvokeAsync(StateHasChanged);
            }
        }

    }


}
