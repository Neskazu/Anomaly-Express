using System.Linq;
using Managers;
using Network;
using Scene;
using Unity.Netcode;

namespace Lobby
{
    public class LobbyController : NetworkBehaviour
    {
        private static PlayerDataProvider Players =>
            MultiplayerManager.Instance.Players;

        private void Awake()
        {
            if (NetworkManager.IsServer)
                Players.OnChange += IsAllReady;
        }

        public override void OnDestroy()
        {
            Players.OnChange -= IsAllReady;
        }

        private void IsAllReady(PlayerData _)
        {
            if (Players.Any(player => !player.IsReady))
                return;

            StartGame();
        }

        public void StartGame()
        {
            SceneLoader.LoadNetwork(SceneLoader.Scene.Game);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetReadyServerRpc(bool isReady, ServerRpcParams serverRpcParams = default)
        {
            var player = Players.First(player => player.ClientId == serverRpcParams.Receive.SenderClientId);

            player.IsReady = isReady;

            Players.Change(player);
        }
    }
}