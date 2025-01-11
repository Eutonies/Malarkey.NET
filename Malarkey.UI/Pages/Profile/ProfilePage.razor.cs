using Microsoft.AspNetCore.Components;
namespace Malarkey.UI.Pages.Profile;
public partial class ProfilePage 
{
    [Inject]
    public NavigationManager NavManager { get; set; }
    private void OnForwardClick() 
    {
        NavManager.NavigateTo("https://eutonies.com", forceLoad: true);
    }

}