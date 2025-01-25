using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Util;
public interface IMalarkeyCache<TKey, TValue> where TKey : notnull
{
    public Task Cache(TKey key, TValue value);

    public void CacheAndForget(TKey key, TValue value) => 
        _ = Cache(key, value);

    public bool TryGet(TKey key, out TValue? value);

    public Task<TValue?> Pop(TKey key);

    public Task PerformCleanup();

}

