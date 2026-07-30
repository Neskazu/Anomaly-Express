using UnityEngine;

namespace Nac.Singleton
{
    public class Service<T> : MonoBehaviour
        where T : Service<T>
    {
        public static T Instance { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            if (Instance)
            {
                Debug.LogError($"[{nameof(Service<T>)}] Service is already running.");

                Destroy(this);
                return;
            }

            Debug.Log($"[{nameof(Service<T>)}] Service is running.");
            Instance = this as T;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Debug.Log($"[{nameof(Service<T>)}] Service shutting down.");
                Instance = null;
            }
        }
    }
}