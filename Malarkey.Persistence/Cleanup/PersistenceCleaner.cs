using Malarkey.Application.Cleanup;
using Malarkey.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Cleanup;

internal class PersistenceCleaner : IPersistenceCleaner
{
    private readonly IDbContextFactory<MalarkeyDbContext> _factory;

    public PersistenceCleaner(IDbContextFactory<MalarkeyDbContext> factory)
    {
        _factory = factory;
    }

    public async Task PerformCleanup()
    {
        await using var cont = await _factory.CreateDbContextAsync();
        await cont.Database.ExecuteSqlRawAsync("call cleanup();");
    }

}
