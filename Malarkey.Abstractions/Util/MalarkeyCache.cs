using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Util;
public class MalarkeyCache<TKey, TValue> : IMalarkeyCache<TKey, TValue> 
    where TKey : notnull
    where TValue : class
{
    public int MinCacheSize = 100;

    private readonly int _capacity;
    private readonly SemaphoreSlim _updateLock = new SemaphoreSlim(1);
    private readonly Dictionary<TKey, CacheRecord> _underlying = new Dictionary<TKey, CacheRecord>();

    internal MalarkeyCache(int capacity) 
    {
        if (capacity < MinCacheSize)
            _capacity = MinCacheSize;
        else 
           _capacity = capacity; 
    }

    public async Task Cache(TKey key, TValue value) => 
        await LockedAction(() =>
        {
            var cacheTime = DateTime.Now;
            _underlying[key] = new CacheRecord(value, cacheTime);
            CheckCapacity();
        });


    public bool TryGet(TKey key, out TValue? value)
    {
        if (_underlying.TryGetValue(key, out var foundVal))
        {
            value = foundVal.Value;
            return true;
        }
        value = default;
        return false;
    }

    public Task<TValue?> Pop(TKey key) => Locked(() =>
    {
        var returnee = _underlying
           .TryGetValue(key, out var rec) ?
           ((TValue?)rec.Value) :
           null;
        _underlying.Remove(key);
        return returnee;
    });



    public Task PerformCleanup() => LockedAction(() => CheckCapacity());

        
    private void CheckCapacity()
    {
        if (_underlying.Count <= _capacity * 2)
            return;
        var currentSize = _underlying.Count;
        var toDelete = currentSize - _capacity;
        var keysToDelete = _underlying
            .OrderBy(_ => _.Value.CacheTime)
            .Select(_ => _.Key)
            .Take(toDelete)
            .ToHashSet();
        foreach(var key in keysToDelete)
            _underlying.Remove(key);
    }

    protected async Task<TOut> Locked<TOut>(Func<TOut> toPerform)
    {
        await _updateLock.WaitAsync();
        try
        {
            return toPerform();
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            _updateLock.Release();
        }
    }

    protected Task LockedAction(Action toPerform) => Locked(() =>
    {
        toPerform();
        return 0;
    } );

    


    private record CacheRecord(TValue Value, DateTime CacheTime);
}
