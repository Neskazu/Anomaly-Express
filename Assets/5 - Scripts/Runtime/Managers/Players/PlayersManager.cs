using System;
using System.Collections.Generic;
using System.Linq;
using Nac.Extensions;
using Nac.Network;
using Nac.Singleton;
using Network;
using Player;
using R3;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Nac
{
    public class PlayersManager : NetworkService<PlayersManager>
    {
        [SerializeField] private PlayersManagerConfig config;

        private readonly NetworkList<PlayerData> players = new();
        private readonly Dictionary<ulong, CharacterData> characters = new();

        private readonly Subject<PlayerData> onPlayerUpdated = new();
        private readonly Subject<PlayerData> onPlayerConnected = new();
        private readonly Subject<PlayerData> onPlayerDisconnected = new();

        private CompositeDisposable disposables;

        public ReadOnlyNetworkList<PlayerData> Players => new(players);

        public Observable<PlayerData> OnPlayerUpdated => onPlayerUpdated;
        public Observable<PlayerData> OnPlayerConnected => onPlayerConnected;
        public Observable<PlayerData> OnPlayerDisconnected => onPlayerDisconnected;

        #region Unity

        public override void Awake()
        {
            base.Awake();

            players.OnListChanged += OnPlayerListChanged;
        }

        public override void OnDestroy()
        {
            players.OnListChanged -= OnPlayerListChanged;

            onPlayerUpdated.Dispose();
            onPlayerConnected.Dispose();
            onPlayerDisconnected.Dispose();
 
            base.OnDestroy();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                players.Clear();
                characters.Clear();

                disposables?.Dispose();
                disposables = new CompositeDisposable();

                NetworkController.Instance.OnClientConnected
                    .Subscribe(OnPlayerConnectedCallback)
                    .AddTo(disposables);

                NetworkController.Instance.OnClientDisconnected
                    .Subscribe(OnPlayerDisconnectedCallback)
                    .AddTo(disposables);

                foreach (var player in NetworkManager.ConnectedClients)
                {
                    OnPlayerConnectedCallback(player.Key);
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            disposables?.Dispose();
            disposables = null;

            foreach (var player in players)
            {
                onPlayerDisconnected.OnNext(player);
            }

            if (IsServer)
            {
                players.Clear();
            }

            characters.Clear();
        }

        #endregion

        #region API

        public PlayerData GetPlayerData(ulong clientId)
        {
            foreach (var player in players)
            {
                if (player.Owner == clientId)
                {
                    return player;
                }
            }

            Debug.LogError($"[{Tag}] Player {clientId} data not found.");
            return default;
        }

        public CharacterData GetPlayerCharacter(ulong clientId)
        {
            if (characters.TryGetValue(clientId, out var character))
            {
                return character;
            }

            foreach (var player in players)
            {
                if (player.Owner != clientId)
                {
                    continue;
                }

                var chara = config.Characters.FirstOrDefault(cd => cd.Id == player.CharacterId);
                if (!chara)
                {
                    break;
                }

                characters.Add(clientId, chara);

                Debug.Log($"[{Tag}] Cached {clientId} character {chara.Id} for player {clientId}.");
                return chara;
            }

            Debug.LogError($"[{Tag}] Player {clientId} missing character");
            return null;
        }

        public CharacterData GetCharacter(ulong characterId)
        {
            return config.Characters.FirstOrDefault(cd => cd.Id == characterId);
        }

        #endregion

        #region RPC

        [ServerRpc(RequireOwnership = false)]
        public void UpdatePlayerServerRpc(ulong clientId, PlayerData data)
        {
            var index = players.IndexOf(data);

            if (index < 0)
            {
                Debug.LogError($"Player {clientId} data not found.");
                return;
            }

            players[index] = data;
        }

        #endregion

        #region Callbacks

        private void OnPlayerConnectedCallback(ulong clientId)
        {
            var occupied = characters.Values.ToArray();
            var available = config.Characters
                .Except(occupied)
                .ToArray();

            if (available.Length <= 0)
            {
                Debug.LogError($"[{Tag}] No free characters available! Fallback to random character.");
            }

            var character = available.Length > 0
                ? available[Random.Range(0, available.Length)]
                : occupied[Random.Range(0, occupied.Length)];

            var player = new PlayerData(clientId, character.Id);

            players.Add(player);
            characters.Add(clientId, character);
            Debug.Log($"[{Tag}] Character {character.Id} assigned to player {clientId}.");
        }

        private void OnPlayerDisconnectedCallback(ulong clientId)
        {
            for (var i = players.Count - 1; i >= 0; i--)
            {
                if (players[i].Owner == clientId)
                {
                    players.RemoveAt(i);
                    characters.Remove(clientId);
                    return;
                }
            }

            Debug.LogError($"[{Tag}] Cleanup failure; Player {clientId} disconnected, but data assigned to it doesn't exist");
        }

        private void OnPlayerListChanged(NetworkListEvent<PlayerData> changeEvent)
        {
            switch (changeEvent.Type)
            {
                case NetworkListEvent<PlayerData>.EventType.Insert:
                case NetworkListEvent<PlayerData>.EventType.Add:
                    onPlayerConnected.OnNext(changeEvent.Value);
                    break;
                case NetworkListEvent<PlayerData>.EventType.Remove:
                case NetworkListEvent<PlayerData>.EventType.RemoveAt:
                    onPlayerDisconnected.OnNext(changeEvent.Value);
                    break;
                case NetworkListEvent<PlayerData>.EventType.Value:
                    onPlayerUpdated.OnNext(changeEvent.Value);
                    break;
                case NetworkListEvent<PlayerData>.EventType.Clear:
                case NetworkListEvent<PlayerData>.EventType.Full:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}