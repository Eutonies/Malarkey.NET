
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Malarkey.UI.Pages.Profile;

public partial class ProfileIdentityConnectionSucceededPage
{
    public const string SucceededPagePath = "connect-succeeded";

    [Inject]
    public IJSRuntime JS { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await JS.InvokeVoidAsync("closeTab");
    }
}
