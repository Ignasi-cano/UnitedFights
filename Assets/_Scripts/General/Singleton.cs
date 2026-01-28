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
                    _instance = (T)FindAnyObjectByType(typeof(T), FindObjectsInactive.Include);

                    if (_instance == null)
                    {
                        GameObject go = new GameObject(typeof(T).Name + " (Auto)");
                        _instance = go.AddComponent<T>();
                        Debug.Log($"[Singleton] Lazy-loaded new instance of {typeof(T).Name}");
                    }
                }
                return _instance;
            }
        }
    }

    public static bool HasInstance => _instance != null;

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            if (_instance.gameObject.name.Contains("(Auto)"))
            {
                Debug.Log($"[Singleton] Scene instance of {typeof(T).Name} on {gameObject.name} is overriding the (Auto) instance.");
                
                T oldInstance = _instance;
                _instance = this as T;
                Destroy(oldInstance.gameObject);
                
                Debug.Log($"[Singleton] {typeof(T).Name} successfully replaced (Auto) instance.");
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
