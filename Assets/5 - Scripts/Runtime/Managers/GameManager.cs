using System.Collections.Generic;
using Nac;
using Nac.Singleton;
using Network;
using Player;
using R3;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class GameManager : NetworkService<GameManager>
    {
        [SerializeField] private PlayerController playerPrefab;
        [SerializeField] private Transform[] spawnPoints;

        private readonly Subject<ulong> onLocalPlayerSpawn = new();

        private int nextSpawnIndex = 0;

        public Observable<ulong> OnLocalPlayerSpawn => onLocalPlayerSpawn;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;

            PlayersManager.Instance.OnPlayerConnected
                .Subscribe(OnPlayerConnectedCallback)
                .AddTo(this);
        }

        public override void OnDestroy()
        {
            if (!IsServer) return;

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
            }

            base.OnDestroy();
        }

        private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode,
            List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (!IsServer)
            {
                return;
            }

            foreach (var clientId in clientsCompleted)
            {
                if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
                {
                    if (client.PlayerObject != null && client.PlayerObject.IsSpawned)
                    {
                        continue;
                    }
                }

                var pos = NextSpawnPoint();
                var player = Instantiate(playerPrefab, pos.position, pos.rotation);
                var netObj = player.GetComponent<NetworkObject>();
                netObj.SpawnAsPlayerObject(clientId, true);

                var data = PlayersManager.Instance.GetPlayerData(clientId);
                player.CharacterId.Value = data.CharacterId;

                OnLocalPlayerSpawnRpc(clientId, RpcTarget.Single(clientId, RpcTargetUse.Temp));
            }
        }

        private void OnPlayerConnectedCallback(PlayerData data)
        {
            var pos = NextSpawnPoint();
            var player = Instantiate(playerPrefab, pos.position, pos.rotation);
            var netObj = player.GetComponent<NetworkObject>();
            netObj.SpawnAsPlayerObject(data.Owner, true);

            player.CharacterId.Value = data.CharacterId;

            OnLocalPlayerSpawnRpc(data.Owner, RpcTarget.Single(data.Owner, RpcTargetUse.Temp));
        }

        private Pose NextSpawnPoint()
        {
            if (spawnPoints == null || spawnPoints.Length <= 0 || spawnPoints[nextSpawnIndex] == null)
            {
                return default;
            }

            var pos = new Pose(spawnPoints[nextSpawnIndex].position, spawnPoints[nextSpawnIndex].rotation);

            nextSpawnIndex++;
            if (nextSpawnIndex >= spawnPoints.Length)
            {
                nextSpawnIndex = 0;
            }

            return pos;
        }

        public Transform GetRandomSpawnPoint()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                return null;
            }

            var randomIndex = Random.Range(0, spawnPoints.Length);

            return spawnPoints[randomIndex];
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void OnLocalPlayerSpawnRpc(ulong id, RpcParams rpcParameters = default)
        {
            onLocalPlayerSpawn.OnNext(id);
        }

        [ServerRpc(RequireOwnership = false)]
        public void KillPlayerServerRpc(ulong clientId)
        {
            var data = PlayersManager.Instance.GetPlayerData(clientId);

            data.IsDead = true;

            PlayersManager.Instance.UpdatePlayerServerRpc(clientId, data);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RevivePlayerServerRpc(ulong clientId)
        {
            var data = PlayersManager.Instance.GetPlayerData(clientId);

            data.IsDead = false;

            PlayersManager.Instance.UpdatePlayerServerRpc(clientId, data);
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdatePlayerVelocityServerRpc(ulong clientId, Vector3 velocity)
        {
            var data = PlayersManager.Instance.GetPlayerData(clientId);

            data.Velocity = velocity;

            PlayersManager.Instance.UpdatePlayerServerRpc(clientId, data);
        }

        [ServerRpc(RequireOwnership = false)]
        public void PunchPlayerServerRpc(ulong clientId, Vector3 velocity)
        {
            var data = PlayersManager.Instance.GetPlayerData(clientId);

            data.Punch += velocity;

            PlayersManager.Instance.UpdatePlayerServerRpc(clientId, data);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ResetPlayerPunchVelocityServerRpc(ulong clientId)
        {
            var data = PlayersManager.Instance.GetPlayerData(clientId);

            data.Punch = Vector3.zero;

            PlayersManager.Instance.UpdatePlayerServerRpc(clientId, data);
        }
    }
}