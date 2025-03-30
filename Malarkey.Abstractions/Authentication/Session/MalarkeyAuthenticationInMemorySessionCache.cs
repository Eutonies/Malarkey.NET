using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Token;
using Malarkey.Abstractions.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Authentication.Session;
public class MalarkeyAuthenticationInMemorySessionCache : IMalarkeyAuthenticationSessionCache
{
    private static long _currentId = 0;
    private static readonly object _idLock = new {};
    private static long NextId()
    {
        lock (_idLock) return _currentId++;
    }

    public MalarkeyAuthenticationInMemorySessionCache() : this(10_000) { }

    public MalarkeyAuthenticationInMemorySessionCache(int capacity)
    {
        _capacity = capacity <= 0 ? 10_00 : capacity;
    }


    private readonly int _capacity;
    private readonly ConcurrentDictionary<long, CacheRecord> _cache = new ConcurrentDictionary<long, CacheRecord>();
    private readonly ConcurrentDictionary<string, CacheRecord> _cacheByState = new ConcurrentDictionary<string, CacheRecord>();


    public Task<MalarkeyAuthenticationSession> InitiateIdpSession(long sessionId, MalarkeyAuthenticationIdpSession idpSession)
    {
        var record = _cache[sessionId];
        var idpSessionId = NextId();
        record.Session = record.Session with
        {
            IdpSession = idpSession with { IdpSessionId = idpSessionId }
        };
        record.LastAccessed = DateTime.Now;
        return Task.FromResult(record.Session);
    }

    public Task<MalarkeyAuthenticationSession> InitiateSession(MalarkeyAuthenticationSession session)
    {
        var sessionId = NextId();
        session = session with { SessionId = sessionId };
        var record = new CacheRecord(sessionId, session.State) { Session = session};
        _cache[sessionId] = record;
        _cacheByState[session.State] = record;
        CheckCleanup();
        return Task.FromResult(record.Session);
    }

    public async Task<MalarkeyAuthenticationSession?> LoadByState(string state) => (await Touch(() => _cacheByState[state]))!.Session;

    public async Task<MalarkeyRefreshTokenData?> LoadRefreshTokenForAccessToken(string accessToken, string subject)
    {
        var relevantRecord = _cache.Values
            .FirstOrDefault(rec =>
               rec.CurrentIdentityProviderToken?.Pipe(
                  tok => tok.Token == accessToken && (rec.IdentityToken?.Identity?.Pipe(id => id.ProvidersIdForIdentity == subject) ?? false)) ?? false);
        if (relevantRecord != null && relevantRecord.IdentityToken != null)
        {
            var identityToken = relevantRecord.IdentityToken;
            var identity = identityToken.Identity;
            var returnee = new MalarkeyRefreshTokenData(
                RefreshToken: identity.IdentityProviderTokenToUse?.RefreshToken!,
                IdentityId: identity.IdentityId,
                IdentityProvider: identity.IdentityProvider
                );
            relevantRecord.LastAccessed = DateTime.Now;
            await Task.CompletedTask;
            return returnee;
        }
        return null;
    }

    public Task<IdentityProviderToken> UpdateIdentityProviderToken(IdentityProviderToken token)
    {
        var relevantRecord =_cache.Values.FirstOrDefault(_ => _.CurrentIdentityProviderToken?.Pipe(
              tok => tok.Provider == token.Provider && tok.Token == token.Token
            ) ?? false
        );
        if (relevantRecord != null && relevantRecord.IdentityToken != null)
        {
            relevantRecord.IdentityToken = relevantRecord.IdentityToken with
            {
                Identity = relevantRecord.IdentityToken.Identity.WithToken(token)
            };
            relevantRecord.LastAccessed = DateTime.Now;
        }
        return Task.FromResult(token);
    }

    public Task<MalarkeyAuthenticationSession> UpdateSession(long sessionId, MalarkeyIdentityProvider identityProvider)
    {
        var record = _cache[sessionId];
        record.Session = record.Session with { RequestedIdProvider = identityProvider };
        record.LastAccessed = DateTime.Now;
        return Task.FromResult(record.Session);
    }

    public Task<MalarkeyAuthenticationSession> UpdateSessionWithTokenInfo(MalarkeyAuthenticationSession session, MalarkeyProfileToken profileToken, MalarkeyIdentityToken identityToken)
    {
        var record = _cache[session.SessionId];
        record.Session = session;
        record.Token = profileToken;
        record.IdentityToken = identityToken;
        record.AuthenticatedAt = DateTime.Now;

        return Task.FromResult(record.Session);
    }

    private Task<CacheRecord?> Touch(Func<CacheRecord?> loader) => Touch(() => Task.FromResult(loader()));


    private async Task<CacheRecord?> Touch(Func<Task<CacheRecord?>> loader)
    {
        var loaded = await loader();
        if (loaded != null)
        {
            loaded.LastAccessed = DateTime.Now;
        }
        return loaded;
    }

    private SemaphoreSlim _cleanupLock = new SemaphoreSlim(1, 1);
    private void CheckCleanup()
    {
        var currentCount = _cache.Count;
        if (_cleanupLock.CurrentCount > 0 || currentCount < _capacity)
            return;
        Task.Run(async () =>
        {
            await _cleanupLock.WaitAsync();
            try
            {
                var oldest = _cache.Values
                   .OrderBy(_ => _.LastAccessed)
                   .Take(currentCount - _capacity)
                   .ToList();
                foreach(var toDel in oldest)
                {
                    _cache.TryRemove(toDel.SessionId, out _);
                    _cacheByState.TryRemove(toDel.State, out _);
                }
            }
            finally
            {
                _cleanupLock.Release();
            }
        });
    }



    private record CacheRecord(
        long SessionId, 
        string State
        )
    {
        public DateTime LastAccessed { get; set; } = DateTime.Now;
        public required MalarkeyAuthenticationSession Session { get; set; }
        public DateTime? AuthenticatedAt { get; set; }
        public IdentityProviderToken? CurrentIdentityProviderToken => IdentityToken?.Identity?.IdentityProviderTokenToUse;
        public MalarkeyProfileToken? Token { get; set; }
        public MalarkeyIdentityToken? IdentityToken { get; set; }


    }


}
