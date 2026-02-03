using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Util for Creating Lazy Singleton for MonoBehaviour
/// </summary>
/// <typeparam name="T"></typeparam>
public class LazySingleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    private static readonly object _lock = new object(); // For thread safety

    /// <summary>
    /// The singleton instance of the class.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Find an existing instance in the scene
                        _instance = FindObjectOfType<T>();

                        if (_instance == null)
                        {
                            // Create a new instance if none exists
                            GameObject singletonObject = new GameObject(typeof(T).Name);
                            _instance = singletonObject.AddComponent<T>();
                        }
                    }
                }
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instance
        }
    }
}