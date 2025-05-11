using Unity.Netcode;

namespace Network.Players
{
    public class NetworkPlayersMediator : NetworkBehaviour
    {
        public NetworkList<PlayerData> Players { get; private set; }

        private void Awake()
        {
            Players = new NetworkList<PlayerData>();
        }
    }
}