using Malarkey.Application.Security;
using Malarkey.Domain.Authentication;
using Malarkey.Persistence.Context;
using Malarkey.Security.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Authentication;
internal class MalarkeyAuthenticationSessionRepository : IMalarkeySessionRepository
{
    private readonly IDbContextFactory<MalarkeyDbContext> _contectFactory;

    public MalarkeyAuthenticationSessionRepository(IDbContextFactory<MalarkeyDbContext> contectFactory)
    {
        _contectFactory = contectFactory;
    }

    public async Task<MalarkeyAuthenticationSession> InitNewSession(MalarkeyOAuthIdentityProvider idProvider, string nonce, string? forwarder, string codeChallenge, string codeVerifier, DateTime initTime)
    {
        await using var cont = await _contectFactory.CreateDbContextAsync();
        var insertee = new MalarkeyAuthenticationSessionDbo
        {
            IdProvider = idProvider.ToString(),
            Nonce = nonce,
            Forwarder = forwarder,
            CodeVerifier = codeVerifier,
            CodeChallenge = codeChallenge,
            InitTime = initTime
        };
        cont.Add(insertee);
        await cont.SaveChangesAsync();
        var returnee = insertee.ToDomain();
        return returnee;
    }


    public async Task<MalarkeyAuthenticationSession?> SessionFor(string state)
    {
        await using var cont = await _contectFactory.CreateDbContextAsync();
        var loaded = await cont.AuthenticationSessions
            .FirstOrDefaultAsync(_ => _.State == state);
        var returnee = loaded?.ToDomain();
        return returnee;
    }


    public async Task<MalarkeyAuthenticationSession?> UpdateWithAuthenticationInfo(string state, DateTime authenticatedTime, string profileTokenId, string identityTokenId)
    {
        await using var cont = await _contectFactory.CreateDbContextAsync();
        var loaded = await cont.AuthenticationSessions
            .FirstOrDefaultAsync(_ => _.State == state);
        if (loaded == null)
            return null;
        loaded.AuthenticatedTime = authenticatedTime;
        loaded.ProfileTokenId = profileTokenId;
        loaded.IdentityTokenId = identityTokenId;
        cont.Update(loaded);
        await cont.SaveChangesAsync();
        var returnee = loaded?.ToDomain();
        return returnee;
    }
}
