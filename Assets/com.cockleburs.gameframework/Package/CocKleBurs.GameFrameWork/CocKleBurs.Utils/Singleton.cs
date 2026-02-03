using CockleBurs.GameFramework.Core;
using UnityEngine;


namespace CockleBurs.GameFramework.Utility
{
public abstract class Singleton<T> : Cherry where T : Cherry
{
   public static T _instance;
    private static bool _applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            return _instance;
        }
    }

    protected virtual void Awake()
    {
            if (_instance == null)
            {
                _instance = this as T;
            }
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        // 确保只销毁一次
        if (_instance == this)
        {
            _applicationIsQuitting = true;
            _instance = null;
        }
    }
}
}