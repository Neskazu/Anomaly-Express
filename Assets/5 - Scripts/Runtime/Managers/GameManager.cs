using System.Collections.Generic;
using Network.Players;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class GameManager : NetworkBehaviour
    {
        [SerializeField] private PlayerController playerPrefab;

        private static PlayerDataProvider Players => MultiplayerManager.Players;

        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (Instance == this)
            {
                Instance = null;
            }
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
                var playerControllerTransform = Instantiate(playerPrefab);

                playerControllerTransform
                    .GetComponent<NetworkObject>()
                    .SpawnAsPlayerObject(clientId, true);

                var data = Players.Get(clientId);
                playerControllerTransform.CharacterId.Value = data.CharacterId;
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