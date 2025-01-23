using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Authentication;
public class MalarkeyAuthenticationRequestContinuationCache
{
    private const int MaxEntriesInCache = 5000;
    private readonly SemaphoreSlim _updateLock = new SemaphoreSlim(1);
    private readonly Dictionary<string, CacheEntry> _entries = [];

    public async Task<string> Cache(HttpRequest request)
    {
        string returnee = "";
        await WithUpdate(async () =>
        {
            var state = Guid.NewGuid().ToString();
            while(_entries.ContainsKey(state)) 
                state = Guid.NewGuid().ToString();
            if(_entries.Count > MaxEntriesInCache)
            {
                var numberToRemove = MaxEntriesInCache / 10;
                var toRemove = _entries
                   .OrderBy(_ => _.Value.CachedAt)
                   .Take(numberToRemove)
                   .ToList();
                foreach (var toRem in toRemove)
                    _entries.Remove(toRem.Key);
            }
            var insertee = MalarkeyAuthenticationRequestContinuation.From(request);
            _entries[state] = new CacheEntry(Continuation: insertee, CachedAt: DateTime.Now);
            returnee = state;
        });
        return returnee;
    }

    public MalarkeyAuthenticationRequestContinuation? Pop(string state)
    {
        var returnee = _entries.GetValueOrDefault(state)?.Continuation;
        if(returnee != null)
            _ = WithUpdate(async () => _entries.Remove(state));
        return returnee;
    }

    public bool TryPop(string state, out MalarkeyAuthenticationRequestContinuation? cont)
    {
        cont = Pop(state);
        return cont != null;
    }



    private async Task WithUpdate(Func<Task> updater)
    {
        await _updateLock.WaitAsync();
        try
        {
            await updater();
        }
        finally
        {
            _updateLock.Release();
        }
    }



    private record CacheEntry(MalarkeyAuthenticationRequestContinuation Continuation, DateTime CachedAt);

}
