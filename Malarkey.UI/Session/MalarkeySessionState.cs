using Malarkey.Abstractions.Token;
using Malarkey.Abstractions.Util;
using Malarkey.Application.Configuration;
using Malarkey.Application.Security;
using Malarkey.Integration.Configuration;
using Microsoft.Extensions.Options;

namespace Malarkey.UI.Session;

public class MalarkeySessionState
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Action _onUpdate;

    public MalarkeySessionState(IServiceScopeFactory scopeFactory, Action onUpdate)
    {
        _scopeFactory = scopeFactory;
        _onUpdate = onUpdate;
    }

    public MalarkeyUser? User { get; private set; }


    public async Task UpdateUserFromContext(HttpContext context)
    {
        if (User != null)
            return;
        using var scope = _scopeFactory.CreateScope();
        var appConf = scope.ServiceProvider.GetRequiredService<IOptions<MalarkeyApplicationConfiguration>>().Value;
        var tokenReceiver = appConf.Certificate.PublicKeyPem.CleanCertificate();
        var tokenHandler = scope.ServiceProvider.GetRequiredService<IMalarkeyTokenIssuer>();
        var profileAndIdentities = await tokenHandler.ExtractProfileAndIdentities(context,tokenReceiver);
        if(profileAndIdentities != null)
        {
            User = new MalarkeyUser(
                Profile: profileAndIdentities.Profile,
                Identities: profileAndIdentities.Identities
                );
            _onUpdate();
        }
    }


}
