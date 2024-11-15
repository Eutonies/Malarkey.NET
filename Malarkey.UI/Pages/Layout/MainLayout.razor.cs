using Malarkey.UI.Session;
using Microsoft.AspNetCore.Components;

namespace Malarkey.UI.Pages.Layout;

public partial class MainLayout
{
    private MalarkeySessionState? _sessionState;

    [Inject]
    public IServiceScopeFactory ScopeFactory { get; set; }

    [Inject]
    public IHttpContextAccessor ContextAccessor { get; set; } 

    protected override async Task OnInitializedAsync()
    {
        _sessionState = new MalarkeySessionState(ScopeFactory, onUpdate: () => InvokeAsync(StateHasChanged));
        await _sessionState.UpdateUserFromContext(context: ContextAccessor.HttpContext!);
    }





}
