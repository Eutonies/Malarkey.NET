using Malarkey.Abstractions.Token;
using Malarkey.Persistence.Context;
using Malarkey.Security.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Token.Model;
internal class MalarkeyTokenRepository : IMalarkeyTokenRepository
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
