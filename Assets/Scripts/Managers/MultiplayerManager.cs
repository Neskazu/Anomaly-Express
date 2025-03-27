using System.Linq;
using Network;
using Session;
using Session.Validators;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Managers
{
    public class MultiplayerManager : NetworkBehaviour
    {
        public static MultiplayerManager Instance { get; private set; }
        public PlayerDataProvider Players { get; private set; }

        private Session.Base.Session session;
        private NetworkList<PlayerData> playersData;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
            playersData = new NetworkList<PlayerData>();
        }

        public void Host(Settings settings)
        {
            NetworkManager.Singleton
                .GetComponent<UnityTransport>()
                .SetConnectionData(settings.Address, settings.Port);

            session = new HostSession(new PlayerLimit(4));

            Players = new PlayerDataProvider(session, playersData);
            Players.OnAdd += _ => SetNameServerRpc(settings.Name);

            session.Start();
        }

        public void Connect(Settings settings)
        {
            NetworkManager.Singleton
                .GetComponent<UnityTransport>()
                .SetConnectionData(settings.Address, settings.Port);

            session = new ClientSession();

            Players = new PlayerDataProvider(session, playersData);
            Players.OnAdd += _ => SetNameServerRpc(settings.Name);

            session.Start();
        }

        public void Disconnect()
        {
            Players.Dispose();
            session.Stop();
            session.Dispose();
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
        {
            var player = Players.First(player => player.ClientId == serverRpcParams.Receive.SenderClientId);

            player.PlayerName = playerName;

            Players.Change(player);
        }

        [ServerRpc(RequireOwnership = true)]
        private void KickServerRpc(ulong id, ServerRpcParams serverRpcParams = default)
        {
            // TODO: Добавить возможность локализации
            NetworkManager.Singleton.DisconnectClient(id, "You have been kicked.");
        }

        public struct Settings
        {
            public string Name;
            public ushort Port;
            public string Address;
        }
    }
}