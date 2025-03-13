using System.Collections.Generic;
using Network;
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
            MultiplayerManager.Instance.Players;

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
            if (!Players.Find(clientId, out var index, out var player))
                return;

            player.IsDead = true;
            Players[index] = player;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RevivePlayerServerRpc(ulong clientId)
        {
            if (!Players.Find(clientId, out var index, out var player))
                return;

            player.IsDead = false;
            Players[index] = player;
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdatePlayerVelocityServerRpc(ulong clientId, Vector3 velocity)
        {
            if (!Players.Find(clientId, out var index, out var player))
                return;

            player.Velocity = velocity;
            Players[index] = player;
        }

        [ServerRpc(RequireOwnership = false)]
        public void PunchPlayerServerRpc(ulong clientId, Vector3 velocity)
        {
            if (!Players.Find(clientId, out var index, out var player))
                return;

            player.Punch += velocity;
            Players[index] = player;
        }

        [ServerRpc(RequireOwnership = false)]
        public void ResetPlayerPunchVelocityServerRpc(ulong clientId)
        {
            if (!Players.Find(clientId, out var index, out var player))
                return;

            player.Punch = Vector3.zero;
            Players[index] = player;
        }
    }
}