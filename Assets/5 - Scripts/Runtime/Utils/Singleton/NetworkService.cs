using Unity.Netcode;
using UnityEngine;

namespace Nac.Singleton
{
    public class NetworkService<T> : NetworkBehaviour
        where T : NetworkService<T>
    {
        public static T Instance { get; private set; }

        public virtual void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (Instance)
            {
                Debug.LogError($"[{nameof(NetworkService<T>)}] Service is already running.");

                Destroy(this);
                return;
            }

            Debug.Log($"[{nameof(NetworkService<T>)}] Service is running.");
            Instance = this as T;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (Instance == this)
            {
                Debug.Log($"[{nameof(NetworkService<T>)}] Service shutting down.");
                Instance = null;
            }
        }
    }
}