using Malarkey.UI.Session;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Malarkey.UI.Pages.Authenticate;

public partial class AuthenticatePage
{
    [Inject]
    public NavigationManager NavManager { get; set; }

    [CascadingParameter]
    public MalarkeySessionState SessionState { get; set; }

    private bool IsAuthenticated => SessionState.User != null;

    public void OnMicrosoftClick(MouseEventArgs e)
    {
        NavManager.NavigateTo("authenticate-microsoft");
        NavManager.Refresh(forceReload: true);
    }

    public void OnGoogleClick(MouseEventArgs e)
    {
        NavManager.NavigateTo("authenticate-google");
        NavManager.Refresh(forceReload: true);
    }

    public void OnFacebookClick(MouseEventArgs e)
    {
        NavManager.NavigateTo("authenticate-facebook");
        NavManager.Refresh(forceReload: true);
    }

}
