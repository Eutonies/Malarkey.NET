using Malarkey.Application.Security;

namespace Malarkey.UI.Session;

public class MalarkeySessionState
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MalarkeySessionState(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public MalarkeyUser? User { get; private set; }


    public async Task UpdateUserFromContext(HttpContext context)
    {
        if (User != null)
            return;
        using var scope = _scopeFactory.CreateScope();
        var tokenHandler = scope.ServiceProvider.GetRequiredService<IMalarkeyTokenHandler>();
        var profileAndIdentities = await tokenHandler.ExtractProfileAndIdentities(context);
        if(profileAndIdentities != null)
        {
            User = new MalarkeyUser(
                Profile: profileAndIdentities.Profile,
                Identities: profileAndIdentities.Identities
                );
        }
    }


}
