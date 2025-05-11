using System.Collections.Generic;
using Mono;
using Network;
using Network.Players;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private PlayerController playerPrefab;

        private static PlayerDataProvider Players =>
            MultiplayerManager.Players;

        private void Awake()
        {
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
            }
        }

        private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode,
            List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                PlayerController playerControllerTransform = Instantiate(playerPrefab);

                playerControllerTransform
                    .GetComponent<NetworkObject>()
                    .SpawnAsPlayerObject(clientId, true);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void KillPlayerServerRpc(ulong clientId)
        {
            var data = Players.Get(clientId);

            data.IsDead = true;
            Players.Update(data);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RevivePlayerServerRpc(ulong clientId)
        {
            var data = Players.Get(clientId);

            data.IsDead = false;
            Players.Update(data);
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdatePlayerVelocityServerRpc(ulong clientId, Vector3 velocity)
        {
            var data = Players.Get(clientId);

            data.Velocity = velocity;
            Players.Update(data);
        }

        [ServerRpc(RequireOwnership = false)]
        public void PunchPlayerServerRpc(ulong clientId, Vector3 velocity)
        {
            var data = Players.Get(clientId);

            data.Punch += velocity;
            Players.Update(data);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ResetPlayerPunchVelocityServerRpc(ulong clientId)
        {
            var data = Players.Get(clientId);

            data.Punch = Vector3.zero;
            Players.Update(data);
        }
    }
}