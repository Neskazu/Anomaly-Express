using Unity.Netcode;
using UnityEngine;

namespace Nac.Singleton
{
    public class NetworkSingleton<T> : NetworkBehaviour
        where T : NetworkSingleton<T>
    {
        public static T Instance { get; private set; }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (Instance)
            {
                Debug.LogError($"[{nameof(NetworkSingleton<T>)}] Singleton is already initialized!");

                Destroy(this);
                return;
            }

            Debug.Log($"[{nameof(NetworkSingleton<T>)}] Singleton initialized.");
            Instance = this as T;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (Instance == this)
            {
                Debug.Log($"[{nameof(NetworkSingleton<T>)}] Singleton released.");
                Instance = null;
            }
        }
    }
}