using Malarkey.UI.Session;

namespace Malarkey.UI.Pages.Layout;

public partial class MainLayout
{
    private readonly MalarkeySessionState _sessionState;
    private readonly IHttpContextAccessor _contextAccessor;

    public MainLayout(IServiceScopeFactory serviceScopeFactory, IHttpContextAccessor contextAccessor)
    {
        _sessionState = new MalarkeySessionState(serviceScopeFactory);
        _contextAccessor = contextAccessor;
    }

    protected override async Task OnInitializedAsync() => await _sessionState.UpdateUserFromContext(context: _contextAccessor.HttpContext!);



}
