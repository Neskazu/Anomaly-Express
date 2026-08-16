using Network;
using Scene;
using System.Linq;
using Nac;
using R3;
using Unity.Netcode;
using UnityEngine;

namespace Lobby
{
    public class LobbyController : NetworkBehaviour
    {
        [SerializeField] private SceneTransitionSequence toGame;

        private bool _gameStarted;

        private void Awake()
        {
            if (NetworkManager.IsServer)
            {
                PlayersManager.Instance.OnPlayerUpdated
                    .Subscribe(IsAllReady)
                    .AddTo(this);
            }
        }

        private void IsAllReady(PlayerData _)
        {
            if (_gameStarted)
            {
                return;
            }

            if (PlayersManager.Instance.Players.Any(player => !player.IsReady))
            {
                return;
            }

            StartGame();
            _gameStarted = true;
        }

        public async void StartGame()
        {
            await SceneTransitionManager.Instance.Play(toGame);
        }

        public void SetReady(bool isReady)
        {
            var clientId = NetworkManager.Singleton.LocalClientId;
            var data = PlayersManager.Instance.GetPlayerData(clientId);

            data.IsReady = isReady;

            PlayersManager.Instance.UpdatePlayerServerRpc(clientId, data);
        }
    }
}