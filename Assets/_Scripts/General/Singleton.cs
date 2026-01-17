using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _applicationIsQuitting = false;
    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    // Find including inactive to prevent creating duplicates if object is just disabled
                    _instance = (T)FindObjectOfType(typeof(T), true);

                    if (_instance == null)
                    {
                        GameObject go = new GameObject(typeof(T).Name + " (Auto)");
                        _instance = go.AddComponent<T>();
                        Debug.Log($"[Singleton] Lazy-loaded new instance of {typeof(T).Name}");
                    }
                    else
                    {
                        Debug.Log($"[Singleton] Found existing instance of {typeof(T).Name} in scene.");
                    }
                }
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            // NEW: If the existing instance was auto-created but THIS one is in the scene (has data)
            // we should let the scene one override it.
            if (_instance.gameObject.name.Contains("(Auto)"))
            {
                Debug.Log($"[Singleton] Scene instance of {typeof(T).Name} on {gameObject.name} is overriding the (Auto) instance.");
                Destroy(_instance.gameObject);
                _instance = this as T;
            }
            else
            {
                Debug.LogWarning($"[Singleton] Duplicate {typeof(T).Name} on {gameObject.name} found! Destroying this one. Current instance is on {_instance.gameObject.name}");
                Destroy(gameObject);
                return;
            }
        }
        
        if (_instance == null)
        {
            _instance = this as T;
            Debug.Log($"[Singleton] {typeof(T).Name} successfully initialized on {gameObject.name}");
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
        _instance = null;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}

public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }
}
