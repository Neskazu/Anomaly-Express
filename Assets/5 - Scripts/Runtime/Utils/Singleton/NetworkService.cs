using Unity.Netcode;
using UnityEngine;

namespace Nac.Singleton
{
    public class NetworkService<T> : NetworkBehaviour
        where T : NetworkService<T>
    {
        public static T Instance { get; private set; }

        public static string Tag => typeof(T).Name;

        public virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);

            if (Instance)
            {
                Debug.LogError($"[{Tag}] Network service is already running.");

                Destroy(this);
                return;
            }

            Debug.Log($"[{Tag}] Network service is running.");
            Instance = this as T;
        }

        public override void OnDestroy()
        {
            if (Instance == this)
            {
                Debug.Log($"[{Tag}] Network service shutting down.");
                Instance = null;
            }

            base.OnDestroy();
        }
    }
}