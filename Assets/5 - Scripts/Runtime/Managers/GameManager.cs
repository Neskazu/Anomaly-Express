using System.Collections.Generic;
using Network.Players;
using Player;
using R3;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class GameManager : NetworkBehaviour
    {
        [SerializeField] private PlayerController playerPrefab;
        [SerializeField] private Transform[] spawnPoints;

        private static PlayerDataProvider Players => MultiplayerManager.Players;

        public static GameManager Instance { get; private set; }

        private readonly Subject<ulong> onLocalPlayerSpawn = new();

        private int nextSpawnIndex = 0;

        public Observable<ulong> OnLocalPlayerSpawn => onLocalPlayerSpawn;

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
            if (!IsServer)
                return;

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

                var data = Players.Get(clientId);
                player.CharacterId.Value = data.CharacterId;

                OnLocalPlayerSpawnRpc(clientId, RpcTarget.Single(clientId, RpcTargetUse.Temp));
            }
        }

        private Pose NextSpawnPoint()
        {
            if (spawnPoints.Length == 0)
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