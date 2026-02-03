using System;
using System.Collections.Generic;


namespace CockleBurs.GameFramework.Utility
{
public static class DelegatePool
{
    private static readonly Dictionary<Type, Delegate> _pool = 
        new Dictionary<Type, Delegate>();
    
    private static readonly object _lock = new object();

    public static T Get<T>(T factory) where T : Delegate
    {
        lock(_lock)
        {
            var type = typeof(T);
            if (!_pool.TryGetValue(type, out var cached))
            {
                cached = factory;
                _pool[type] = cached;
            }
            return (T)cached;
        }
    }

    public static void Reset<T>() where T : Delegate
    {
        lock(_lock)
        {
            _pool.Remove(typeof(T));
        }
    }

    public static void Clear()
    {
        lock(_lock)
        {
            _pool.Clear();
        }
    }
}
}