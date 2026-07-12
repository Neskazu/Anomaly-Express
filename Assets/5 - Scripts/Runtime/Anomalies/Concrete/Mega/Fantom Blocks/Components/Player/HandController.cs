using Managers;
using Network.Players;
using Unity.Netcode;
using UnityEngine;

namespace Anomalies
{
    public class HandController : NetworkBehaviour
    {
        [SerializeField] private Transform handsRoot;
        [SerializeField] private GameObject[] handsPrefabs;

        private static PlayerDataProvider Players => MultiplayerManager.Players;

        protected override void OnNetworkPostSpawn()
        {
            base.OnNetworkPostSpawn();

            var owner = NetworkManager.Singleton.LocalClientId;
            var data = Players.Get(owner);

            foreach (Transform child in handsRoot)
            {
                Destroy(child.gameObject);
            }

            var handInstance = Instantiate(handsPrefabs[data.CharacterId], handsRoot);

            handInstance.transform.localPosition = Vector3.zero;
            handInstance.transform.localRotation = Quaternion.identity;
            handInstance.transform.localScale = Vector3.one;
        }
    }
}