using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Token;
using Malarkey.Application.Profile.Persistence;
using Malarkey.Application.Security;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Util;
using Malarkey.Persistence.Context;
using Malarkey.Persistence.Token.Model;
using Malarkey.Security.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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

    public async Task<MalarkeyAuthenticationSession> InitNewSession(
        MalarkeyIdentityProvider idProvider, 
        string nonce, 
        string? forwarder, 
        string codeChallenge, 
        string codeVerifier, 
        DateTime initTime,
        string audience,
        string[]? scopes,
        string? forwarderState)
    {
        await using var cont = await _contectFactory.CreateDbContextAsync();
        var insertee = new MalarkeyAuthenticationSessionDbo
        {
            IdProvider = idProvider.ToDbo(),
            Nonce = nonce,
            Forwarder = forwarder,
            CodeVerifier = codeVerifier,
            CodeChallenge = codeChallenge,
            InitTime = initTime,
            Audience = audience,
            Scopes = scopes?.MakeString(" "),
            ForwarderState = forwarderState
        };
        cont.Add(insertee);
        await cont.SaveChangesAsync();
        var returnee = insertee.ToDomain();
        return returnee;
    }


    public async Task<MalarkeyAuthenticationSession?> SessionFor(string state)
    {
        if (!Guid.TryParse(state, out var guidState))
            return null;
        await using var cont = await _contectFactory.CreateDbContextAsync();
        var loaded = await cont.AuthenticationSessions
            .FirstOrDefaultAsync(_ => _.State == guidState);
        var returnee = loaded?.ToDomain();
        return returnee;
    }


    public async Task<MalarkeyAuthenticationSession?> UpdateWithAuthenticationInfo(string state, DateTime authenticatedTime, Guid profileTokenId, Guid identityTokenId)
    {
        if (!Guid.TryParse(state, out var guidState))
            return null;
        await using var cont = await _contectFactory.CreateDbContextAsync();
        var loaded = await cont.AuthenticationSessions
            .FirstOrDefaultAsync(_ => _.State == guidState);
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

    public async Task<IMalarkeySessionRepository.RefreshTokenLoadData?> LoadRefreshTokenForAccessToken(string accessToken, string clientCertificate)
    {
        await using var cont = await _contectFactory.CreateDbContextAsync();
        var relevantTokenQuery = from tok in cont.IdentityProviderTokens.Where(_ => _.TokenString == accessToken)
                                 join ident in cont.Identities
                                 on tok.IdentityId equals ident.IdentityId
                                 join identTok in cont.Tokens
                                 on ident.IdentityId equals identTok.IdentityId
                                 join sess in cont.AuthenticationSessions.Where(_ => _.Audience == clientCertificate)
                                 on identTok.TokenId equals sess.IdentityTokenId
                                 select new { Token = tok, Provider = ident.Provider };
        var relevantToken = await relevantTokenQuery.FirstOrDefaultAsync();
        if (relevantToken == null)
            return null;
        var scopesSet = relevantToken.Token.ToDomain().Scopes.ToHashSet();
        var issuedTokens = await cont.IdentityProviderTokens
            .Where(_ => _.IdentityId == relevantToken.Token.IdentityId && _.RefreshToken != null)
            .OrderByDescending(_ => _.Expires)
            .ToListAsync();
        var bestToken = issuedTokens
            .FirstOrDefault(_ => _.ToDomain().Scopes.ToHashSet().IsSupersetOf(scopesSet));
        if (bestToken?.RefreshToken == null)
            return null;
        var returnee = new IMalarkeySessionRepository.RefreshTokenLoadData(
            bestToken.RefreshToken!,
            IdentityId: relevantToken.Token.IdentityId,
            relevantToken.Provider.ToDomain()
            );
        return returnee;
    }

    public async Task SaveRefreshedToken(IdentityProviderToken token, Guid identityId)
    {
        await using var cont = await _contectFactory.CreateDbContextAsync();
        var insertee = token.ToDbo(identityId);
        insertee.IdentityId = identityId;
        cont.Add(insertee);
        await cont.SaveChangesAsync();
    }

}
