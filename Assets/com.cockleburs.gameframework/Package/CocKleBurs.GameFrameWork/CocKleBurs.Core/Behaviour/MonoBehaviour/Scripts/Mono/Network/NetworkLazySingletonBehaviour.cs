using UnityEngine;
using Netick;
using Netick.Unity;

/// <summary>
/// A lazy singleton NetworkBehaviour that is created only when first accessed.
/// </summary>
/// <typeparam name="T"></typeparam>
public class NetworkLazySingletonBehaviour<T> : NetworkBehaviour where T : Component
{
    private static T _instance;
    private static readonly object _lock = new object(); // For thread safety

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        _instance = singletonObject.AddComponent<T>();
                    }
                }

                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        lock (_lock)
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
            else if (_instance != this)
            {
                Destroy(gameObject); // Optionally destroy the new instance
            }
        }
    }
}