using UnityEngine;

/// <summary>
/// A singleton MonoBehaviour that only exists within the current scene.
/// </summary>
/// <typeparam name="T"></typeparam>
public class SceneSingleton<T> : MonoBehaviour where T : Component
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
                // Optionally log a warning here if desired
                Debug.LogWarning($"Instance of {typeof(T).Name} already exists in the scene. Destroying the new instance.");
                Destroy(gameObject); // Optionally destroy the new instance
            }
        }
    }
}