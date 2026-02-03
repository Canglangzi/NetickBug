using UnityEngine;

/// <summary>
/// A lazy singleton MonoBehaviour that is created only when first accessed
/// and persists across scenes.
/// </summary>
/// <typeparam name="T"></typeparam>
public class LazyPersistentSingleton<T> : MonoBehaviour where T : Component
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
                        DontDestroyOnLoad(singletonObject); // Make this instance persist across scenes
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
                DontDestroyOnLoad(gameObject); // Ensure this instance persists across scenes
            }
            else if (_instance != this)
            {
                // Optionally log a warning here if desired
                Debug.LogWarning($"Instance of {typeof(T).Name} already exists. Destroying the new instance.");
                Destroy(gameObject); // Optionally destroy the new instance
            }
        }
    }
}