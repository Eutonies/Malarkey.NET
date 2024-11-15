using Malarkey.Application.Security;
using Malarkey.Domain.Profile;
using Malarkey.Integration;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;

namespace Malarkey.UI.Pages.Authenticate;

public class AuthenticatedProviderPage : ComponentBase
{

    [Inject]
    public IHttpContextAccessor ContextAccessor { get; set; }

    [Inject]
    public IMalarkeyTokenHandler TokenHandler { get; set; }

    protected async Task ExtractIdentities()
    {
        var context = ContextAccessor.HttpContext;
        foreach(var ident in context!.User.Identities)
        {
            if (ident.IsAuthenticatedMicrosoftIdentity())
                await CaptureMicrosoft(ident);
            else if (ident.IsAuthenticatedGoogleIdentity())
                await CaptureGoogle(ident);
            else if (ident.IsAuthenticatedFacebookIdentity())
                await CaptureGoogle(ident);

        }
    }

    protected async Task CaptureMicrosoft(ClaimsIdentity ident)
    {
        var identity = new MicrosoftIdentity()
    }

    protected async Task CaptureGoogle(ClaimsIdentity ident)
    {

    }

    protected async Task CaptureFacebook(ClaimsIdentity ident)
    {

    }


}
