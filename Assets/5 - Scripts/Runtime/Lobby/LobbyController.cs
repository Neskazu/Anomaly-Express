using Managers;
using Network;
using Network.Players;
using Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Lobby
{
    public class LobbyController : NetworkBehaviour
    {
        [SerializeField] private SceneTransitionSequence toGame;
        private bool _gameStarted;
        private static PlayerDataProvider Players =>
            MultiplayerManager.Players;

        private void Awake()
        {
            if (NetworkManager.IsServer)
            {
                MultiplayerManager.Players.OnUpdated += IsAllReady;
                MultiplayerManager.Players.OnConnected += OnPlayersChanged;
                MultiplayerManager.Players.OnDisconnected += OnPlayersChanged;
            }
        }

        private void Start()
        {
            MultiplayerManager.Instance.SetNameServerRpc("Name");
            AssignCharacters();
        }

        public override void OnDestroy()
        {
            MultiplayerManager.Players.OnUpdated -= IsAllReady;
            MultiplayerManager.Players.OnDisconnected -= OnPlayersChanged;
            MultiplayerManager.Players.OnConnected -= OnPlayersChanged;

        }

        private void IsAllReady(PlayerData _)
        {
            if (_gameStarted)
                return;
            if (Players.Any(player => !player.IsReady))
                return;

            StartGame();
            _gameStarted = true;
        }

        public async void StartGame()
        {
            AssignCharacters();
            await SceneTransitionController.Instance.Play(toGame);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetReadyServerRpc(bool isReady, ServerRpcParams serverRpcParams = default)
        {
            var data = Players.Get(serverRpcParams.Receive.SenderClientId);

            data.IsReady = isReady;
            Players.Update(data);
        }
        private void AssignCharacters()
        {
            var players = Players.ToList();

            var allCharacters = new List<int> { 0, 1, 2, 3 };

            var takenCharacters = players
                .Where(p => p.CharacterId >= 0)
                .Select(p => p.CharacterId)
                .ToHashSet();

            var freeCharacters = allCharacters
                .Where(id => !takenCharacters.Contains(id))
                .OrderBy(_ => UnityEngine.Random.value)
                .ToList();

            int freeIndex = 0;

            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];

                if (player.CharacterId >= 0)
                    continue;

                if (freeIndex >= freeCharacters.Count)
                    break;

                player.CharacterId = freeCharacters[freeIndex++];
                Players.Update(player);
            }
        }
        private void OnPlayersChanged(PlayerData _)
        {
            AssignCharacters();
        }
    }
}