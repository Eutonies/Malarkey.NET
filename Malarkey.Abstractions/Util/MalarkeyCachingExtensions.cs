using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Util;
public static class MalarkeyCachingExtensions
{
    private static readonly HashSet<(Type, Type)> _registeredCachings = new HashSet<(Type, Type)>();
    public static IServiceCollection AddMalarkeyCaching<TKey, TValue>(this IServiceCollection services, int capacity = 2_000)
        where TKey : notnull
        where TValue : class
    {
        var cachingKey = (typeof(TKey), typeof(TValue));
        if(_registeredCachings.Contains(cachingKey))
            return services;
        lock (_registeredCachings)
        {
            if (_registeredCachings.Contains(cachingKey))
                return services;
            var instance = new MalarkeyCache<TKey, TValue>(capacity);
            services.AddSingleton<IMalarkeyCache<TKey, TValue>>((_) => instance);
            _registeredCachings.Add(cachingKey);
        }
        return services;
    }


}
