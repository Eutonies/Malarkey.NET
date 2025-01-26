using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Malarkey.Abstractions.Util;
public class MalarkeySynchronizer
{
    private readonly SemaphoreSlim _locksLock = new SemaphoreSlim(1);
    private readonly Dictionary<string, SemaphoreSlim> _locks = new Dictionary<string, SemaphoreSlim>();
    private readonly Dictionary<string, int> _usageCount = new Dictionary<string, int>();
    private const string DefaultActionName = "DEFAULT";


    public Task PerformLockedActionAsync<TLock>(Func<Task> toPerform) =>
        PerformLockedActionAsync(typeof(TLock), DefaultActionName, toPerform);

    public Task PerformLockedActionAsync<TLock>(string action, Func<Task> toPerform) =>
        PerformLockedActionAsync(typeof(TLock), action, toPerform);

    public Task PerformLockedActionAsync(Type lockType, string action, Func<Task> toPerform) =>
        PerformLockedAsync(lockType, action, async () =>
        {
            await toPerform();
            return 0;
        });


    public Task<TOut> PerformLockedAsync<TOut, TLock>(Func<Task<TOut>> toPerform) =>
        PerformLockedAsync(typeof(TLock), DefaultActionName, toPerform);

    public Task<TOut> PerformLockedAsync<TOut, TLock>(string action, Func<Task<TOut>> toPerform) =>
        PerformLockedAsync(typeof(TLock), action, toPerform);


    public async Task<TOut> PerformLockedAsync<TOut>(Type lockType, string action, Func<Task<TOut>> toPerform)
    {
        TOut returnee;
        var relLock = await LockFor(lockType, action);
        await relLock.WaitAsync();
        try
        {
            returnee = await toPerform();
        }
        finally
        {
            relLock.Release();
        }
        await ReleaseLock(lockType, action);
        return returnee;
    }

    public Task PerformLockedAction<TLock>(Action toPerform) =>
        PerformLockedAction(typeof(TLock), DefaultActionName, toPerform);

    public Task PerformLockedAction<TLock>(string action, Action toPerform) =>
        PerformLockedAction(typeof(TLock), action, toPerform);

    public Task PerformLockedAction(Type lockType, string action, Action toPerform) =>
        PerformLocked<int>(lockType, action, () =>
        {
            toPerform();
            return 0;
        });


    public Task<TOut> PerformLocked<TOut, TLock>(Func<TOut> toPerform) =>
        PerformLocked<TOut>(typeof(TLock), DefaultActionName, toPerform);


    public Task<TOut> PerformLocked<TOut, TLock>(string action, Func<TOut> toPerform) =>
        PerformLocked<TOut>(typeof(TLock), action, toPerform);


    public async Task<TOut> PerformLocked<TOut>(Type lockType, string action, Func<TOut> toPerform)
    {
        TOut returnee;
        var relLock = await LockFor(lockType, action);
        await relLock.WaitAsync();
        try
        {
            returnee = toPerform();
        }
        finally
        {
            relLock.Release();
        }
        await ReleaseLock(lockType, action);
        return returnee;
    }



    private async Task<SemaphoreSlim> LockFor(Type type, string action)
    {
        var lockName = LockName(type, action);
        SemaphoreSlim relLock;
        await _locksLock.WaitAsync();
        try
        {
            if(_locks.TryGetValue(lockName, out var lo))
            {
                relLock = lo;
                _usageCount[lockName] = _usageCount[lockName] + 1;
            }
            else
            {
                relLock = new SemaphoreSlim(1);
                _locks[lockName] = relLock;
                _usageCount[lockName] = 1;
            }
        }
        finally
        {
            _locksLock.Release();
        }
        return relLock;
    }

    private async Task ReleaseLock(Type type, string action)
    {
        var lockName = LockName(type, action);
        await _locksLock.WaitAsync();
        try
        {
            var curCount = _usageCount[lockName] - 1;
            if(curCount == 0)
            {
                _usageCount.Remove(lockName);
                _locks.Remove(lockName);
            }
            else
            {
                _usageCount[lockName] = curCount;
            }
        }
        finally
        {
            _locksLock.Release();
        }
    }

    private static string LockName(Type type, string action) => $"{type.FullName}_{action}";



}
