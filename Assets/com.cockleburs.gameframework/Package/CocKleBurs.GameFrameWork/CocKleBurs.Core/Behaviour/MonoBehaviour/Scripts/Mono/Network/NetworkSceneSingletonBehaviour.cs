using UnityEngine;
using Netick;
using Netick.Unity;

/// <summary>
/// A singleton NetworkBehaviour that is created for each scene and does not persist across scenes.
/// </summary>
/// <typeparam name="T"></typeparam>
public class NetworkSceneSingletonBehaviour<T> : NetworkBehaviour where T : Component
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