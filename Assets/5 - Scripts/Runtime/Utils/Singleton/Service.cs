using UnityEngine;

namespace Nac.Singleton
{
    public class Service<T> : MonoBehaviour
        where T : Service<T>
    {
        public static T Instance { get; private set; }

        public static string Tag => typeof(T).Name;

        public virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);

            if (Instance)
            {
                Debug.LogError($"[{Tag}] Service is already running.");

                Destroy(this);
                return;
            }

            Debug.Log($"[{Tag}] Service is running.");
            Instance = this as T;
        }

        public virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Debug.Log($"[{Tag}] Service shutting down.");
                Instance = null;
            }
        }
    }
}