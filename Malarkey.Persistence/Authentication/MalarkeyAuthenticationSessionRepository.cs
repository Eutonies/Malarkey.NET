using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Token;
using Malarkey.Application.Profile.Persistence;
using Malarkey.Abstractions.Authentication;
using Malarkey.Persistence.Context;
using Malarkey.Persistence.Token.Model;
using Microsoft.EntityFrameworkCore;
using Malarkey.Application.Authentication;
using Malarkey.Persistence.Authentication.Model;

namespace Malarkey.Persistence.Authentication;
internal class MalarkeyAuthenticationSessionRepository : IMalarkeyAuthenticationSessionRepository
{
    private readonly IDbContextFactory<MalarkeyDbContext> _contectFactory;

    public MalarkeyAuthenticationSessionRepository(IDbContextFactory<MalarkeyDbContext> contectFactory)
    {
        _contectFactory = contectFactory;
    }



    public async Task SaveRefreshedToken(IdentityProviderToken token, Guid identityId)
    {
        await using var cont = await _contectFactory.CreateDbContextAsync();
        var insertee = token.ToDbo(identityId);
        insertee.IdentityId = identityId;
        cont.Add(insertee);
        await cont.SaveChangesAsync();
    }

    public async Task<MalarkeyAuthenticationSession> InitiateSession(MalarkeyAuthenticationSession session)
    {
        await using var cont = await _contectFactory.CreateDbContextAsync();
        var (insSession, insPars, insIdpSession) = session.ToDbo();
        cont.Add(insSession);
        if(insPars.Any())
            cont.AddRange(insPars);
        if(insIdpSession != null)
            cont.Add(insIdpSession);
        await cont.SaveChangesAsync();
        var returnee = insSession.ToDomain(insPars, insIdpSession);
        return returnee;
    }

    public async Task<MalarkeyAuthenticationSession> UpdateSession(long sessionId, MalarkeyIdentityProvider identityProvider)
    {
        await using var cont = await _contectFactory.CreateDbContextAsync();
        var loaded = await cont.AuthenticationSessions
            .FirstAsync(_ => _.SessionId == sessionId);
        loaded.RequestedIdentityProvider = identityProvider.ToDbo();
        cont.Update(loaded);
        await cont.SaveChangesAsync();
        var returnee = await Load(sessionId, cont);
        return returnee;
    }

    public async Task<MalarkeyAuthenticationSession> InitiateIdpSession(long sessionId, MalarkeyAuthenticationIdpSession session)
    {
        await using var cont = await _contectFactory.CreateDbContextAsync();
        var insertee = session.ToDbo();
        insertee.SessionId = sessionId;
        cont.Add(insertee);
        await cont.SaveChangesAsync();
        var returnee = await Load(sessionId, cont);
        return returnee;
    }

    public async Task<MalarkeyAuthenticationSession?> LoadByState(string state)
    {
        await using var cont = await _contectFactory.CreateDbContextAsync();
        if (!Guid.TryParse(state, out var guidState))
            return null;
        var loaded = await cont.AuthenticationSessions
            .FirstOrDefaultAsync(_ => _.State == guidState);
        if (loaded == null)
            return null;
        var returnee = await Load(loaded.SessionId, cont);
        return returnee;
    }

    public async Task<MalarkeyAuthenticationSession> UpdateSessionWithTokenInfo(MalarkeyAuthenticationSession session, MalarkeyProfileToken profileToken, MalarkeyIdentityToken identityToken)
    {
        await using var cont = await _contectFactory.CreateDbContextAsync();
        var loaded = await cont.AuthenticationSessions
            .FirstAsync(_ => _.SessionId == session.SessionId);
        loaded.AuthenticatedTime = DateTime.Now;
        loaded.ProfileTokenId = profileToken.TokenId;
        loaded.IdentityTokenId = identityToken.TokenId;
        cont.Update(loaded);
        var returnee = await Load(loaded.SessionId, cont);
        return returnee;
    }

    public async Task<IdentityProviderToken?> Refresh(string accessToken, string audiencePublicKey)
    {
        throw new NotImplementedException();
    }

    private async Task<MalarkeyAuthenticationSession> Load(long sessionId, MalarkeyDbContext cont)
    {
        var session = await cont.AuthenticationSessions
            .FirstAsync(_ => _.SessionId == sessionId);
        var idpSession = await cont.AuthenticationIdpSessions
            .FirstOrDefaultAsync(_ => _.SessionId == sessionId);
        var sessionPars = await cont.AuthenticationSessionParameters
            .Where(_ => _.SessionId == sessionId)
            .ToListAsync();
        var returnee = session.ToDomain(sessionPars, idpSession);
        return returnee;
    }

    public async Task<MalarkeyRefreshTokenData?> LoadRefreshTokenForAccessToken(string accessToken, string clientCertificate)
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
        var scopesSet = relevantToken.Token.ToDomain(relevantToken.Provider).Scopes.ToHashSet();
        var issuedTokens = await cont.IdentityProviderTokens
            .Where(_ => _.IdentityId == relevantToken.Token.IdentityId && _.RefreshToken != null)
            .OrderByDescending(_ => _.Expires)
            .ToListAsync();
        var bestToken = issuedTokens
            .FirstOrDefault(_ => _.ToDomain(relevantToken.Provider).Scopes.ToHashSet().IsSupersetOf(scopesSet));
        if (bestToken?.RefreshToken == null)
            return null;
        var returnee = new MalarkeyRefreshTokenData(
            RefreshToken: bestToken.RefreshToken!,
            IdentityId: relevantToken.Token.IdentityId,
            IdentityProvider: relevantToken.Provider.ToDomain()
            );
        return returnee;
    }

    public Task<IdentityProviderToken> UpdateIdentityProviderToken(IdentityProviderToken token)
    {
        throw new NotImplementedException();
    }
}
