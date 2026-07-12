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

        [SerializeField] FantomBlockGuild neutralGuild;
        [SerializeField] FantomBlockGuild[] guilds;
        [SerializeField] LensComponent ownerLens;

        private static PlayerDataProvider Players => MultiplayerManager.Players;

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
            var player = Players.Get(owner);
            var guild = guilds[player.CharacterId];
            var color = guild.Color;

            ownerLens.UpdateColor(color.RGBA);

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
            var players = Players.ToList();
            var ids = players.Select(p => p.CharacterId).ToArray();

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
                var characterId = ids[i % ids.Length];

                foreach (var fantomComponent in guild.Components)
                {
                    fantomComponent.UpdateColorRpc(guild.Id);
                }

                Debug.Log($"Assigned Guild {guild.Id} to character id {characterId}");
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

        public FantomBlockGuild GetGuild(ulong guildID)
        {
            return guilds.First(g => g.Id == guildID);
        }
    }
}