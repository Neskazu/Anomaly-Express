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

        private static PlayerDataProvider Players =>
            MultiplayerManager.Players;

        private void Awake()
        {
            if (NetworkManager.IsServer)
                MultiplayerManager.Players.OnUpdated += IsAllReady;
        }

        private void Start()
        {
            MultiplayerManager.Instance.SetNameServerRpc("Name");
        }

        public override void OnDestroy()
        {
            MultiplayerManager.Players.OnUpdated -= IsAllReady;
        }

        private void IsAllReady(PlayerData _)
        {
            if (Players.Any(player => !player.IsReady))
                return;

            StartGame();
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

            var availableCharacters = new List<int> { 0, 1, 2, 3 }
             .OrderBy(_ => UnityEngine.Random.value)
             .ToList();
            for (int i = 0; i < players.Count; i++)
            {
                var data = players[i];
                data.CharacterId = availableCharacters[i];

                Players.Update(data);
            }
        }
    }
}