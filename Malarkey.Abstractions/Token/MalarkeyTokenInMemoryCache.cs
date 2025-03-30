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
public class MalarkeyTokenInMemoryCache : IMalarkeyTokenCache
{

    public MalarkeyTokenInMemoryCache() : this(10_000) { }

    public MalarkeyTokenInMemoryCache(int capacity)
    {
        _capacity = capacity <= 0 ? 10_00 : capacity;
    }

    public Task<MalarkeyIdentityToken> SaveToken(MalarkeyIdentityToken token)
    {
        var tokenId = Guid.NewGuid();
        token = token.WithId(tokenId);
        var record = new CacheRecord(tokenId, token);
        _cache[tokenId] = record;
        CheckCleanup();
        return Task.FromResult(token);
    }

    public Task<MalarkeyProfileToken> SaveToken(MalarkeyProfileToken token)
    {
        var tokenId = Guid.NewGuid();
        token = token.WithId(tokenId);
        var record = new CacheRecord(tokenId, token);
        _cache[tokenId] = record;
        CheckCleanup();
        return Task.FromResult(token);
    }



    private readonly int _capacity;
    private readonly ConcurrentDictionary<Guid, CacheRecord> _cache = new ConcurrentDictionary<Guid, CacheRecord>();




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
                    _cache.TryRemove(toDel.TokenId, out _);
                }
            }
            finally
            {
                _cleanupLock.Release();
            }
        });
    }


    private record CacheRecord(
        Guid TokenId, 
        MalarkeyToken Token
        )
    {
        public DateTime LastAccessed { get; set; } = DateTime.Now;


    }


}
