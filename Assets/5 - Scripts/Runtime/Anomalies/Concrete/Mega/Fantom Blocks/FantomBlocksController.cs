using System.Collections.Generic;
using System.Linq;
using Managers;
using Network.Players;
using Player.Components;
using R3;
using Unity.Netcode;
using UnityEngine;

namespace Anomalies
{
    public class FantomBlocksController : AnomalyBase
    {
        public static FantomBlocksController Singleton { get; private set; }

        [SerializeField] private FantomBlockGuild[] guilds;
        [SerializeField] private LensComponent localLens;
        [SerializeField] private AnomalyLevitatingObjects anomalyLevitatingObjects;

        private static PlayerDataProvider Players => MultiplayerManager.Players;
        public LensComponent LocalPlayerLens => localLens;

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
            }

            GameManager.Instance.OnLocalPlayerSpawn
                .Subscribe(EnableJump)
                .AddTo(this);

            Singleton = this;
        }

        private void Start()
        {
            if (IsServer)
            {
                Activate();
                anomalyLevitatingObjects.Activate();
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (Singleton == this)
            {
                Singleton = null;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            var owner = NetworkManager.Singleton.LocalClientId;
            var info = InfoManager.Instance.GetCharacter(owner);

            localLens.UpdateColor(info.Color);
            localLens.enabled = false;

            if (!IsServer)
            {
                return;
            }

            NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer || NetworkManager.Singleton == null)
            {
                return;
            }

            NetworkManager.Singleton.OnClientConnectedCallback -= ClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= ClientDisconnected;
        }

        private void ClientConnected(ulong _)
        {
            DistributeGuilds();
        }

        private void ClientDisconnected(ulong _)
        {
            DistributeGuilds();
        }

        private void DistributeGuilds()
        {
            var ids = NetworkManager.Singleton.ConnectedClients.Keys.ToArray();

            if (ids.Length == 0)
            {
                Debug.LogWarning("No players available to receive guilds.");
                return;
            }

            if (guilds == null || guilds.Length == 0)
            {
                Debug.LogWarning("No guilds available to distribute.");
                return;
            }

            for (var i = 0; i < guilds.Length; i++)
            {
                var guild = guilds[i];
                var clientId = ids[i % ids.Length];
                var characterId = Players.Get(clientId).CharacterId;
                var character = InfoManager.Instance.GetCharacter(characterId);

                guild.SetOwner(clientId);
                guild.SetColor(character.Color);
            }
        }

        protected override void OnActivate()
        {
            DistributeGuilds();
        }

        protected override void OnDeactivate()
        {
        }

        private void EnableJump(ulong id)
        {
            var player = NetworkManager.Singleton.LocalClient.PlayerObject;
            var jumpComp = player.GetComponent<PlayerJump>();

            if (jumpComp != null)
            {
                jumpComp.IsJumpEnabled = true;
            }
        }

        public IEnumerable<FantomBlockGuild> GetGuilds(ulong ownerId)
        {
            return guilds.Where(g => g.OwnerId == ownerId);
        }
    }
}