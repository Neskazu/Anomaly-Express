using Unity.Netcode;
using UnityEngine;

namespace Nac.Singleton
{
    public class NetworkSingleton<T> : NetworkBehaviour
        where T : NetworkSingleton<T>
    {
        public static T Instance { get; private set; }

        public static string Tag => typeof(T).Name;

        public virtual void Awake()
        {
            if (Instance)
            {
                Debug.LogError($"[{Tag}] Network instance already exists!");

                Destroy(this);
                return;
            }

            Debug.Log($"[{Tag}] Network instance created.");
            Instance = this as T;
        }

        public override void OnDestroy()
        {
            if (Instance == this)
            {
                Debug.Log($"[{Tag}] Network instance destroyed.");
                Instance = null;
            }

            base.OnDestroy();
        }
    }
}