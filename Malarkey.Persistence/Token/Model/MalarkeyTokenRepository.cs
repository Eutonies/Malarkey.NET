using Malarkey.Abstractions.Token;
using Malarkey.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Malarkey.Persistence.Token.Model;
internal class MalarkeyTokenRepository : IMalarkeyTokenCache
{
    private readonly IDbContextFactory<MalarkeyDbContext> _contextFactory;

    public MalarkeyTokenRepository(IDbContextFactory<MalarkeyDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<MalarkeyIdentityToken> SaveToken(MalarkeyIdentityToken token)
    {
        await using var cont = await _contextFactory.CreateDbContextAsync();
        var insertee = token.ToDbo();
        cont.Add(insertee);
        await cont.SaveChangesAsync();
        var returnee = insertee.ToIdentityToken(token.Identity);
        return returnee;
    }

    public async Task<MalarkeyProfileToken> SaveToken(MalarkeyProfileToken token)
    {
        await using var cont = await _contextFactory.CreateDbContextAsync();
        var insertee = token.ToDbo();
        cont.Add(insertee);
        await cont.SaveChangesAsync();
        var returnee = insertee.ToProfileToken(token.Profile);
        return returnee;
    }
}
