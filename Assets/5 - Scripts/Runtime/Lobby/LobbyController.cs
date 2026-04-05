using System;
using System.Linq;
using Managers;
using Mono;
using Network;
using Network.Players;
using Scene;
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
            await SceneTransitionController.Instance.Play(toGame);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetReadyServerRpc(bool isReady, ServerRpcParams serverRpcParams = default)
        {
            var data = Players.Get(serverRpcParams.Receive.SenderClientId);

            data.IsReady = isReady;
            Players.Update(data);
        }
    }
}