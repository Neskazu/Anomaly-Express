using Network;
using Network.Players;
using Sessions;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Serialization;

namespace Managers
{
    public class MultiplayerManager : NetworkBehaviour
    {
        public static MultiplayerManager Instance { get; private set; }
        public static Session Session { get; private set; }
        public static PlayerDataProvider Players => Session.Players;

        [SerializeField] private NetworkPlayersMediator playersMediatorPrefab;

        private NetworkPlayersMediator playersMediatorInstance;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }

        public void Host(Settings settings)
        {
            NetworkManager.Singleton
                .GetComponent<UnityTransport>()
                .SetConnectionData(settings.Address, settings.Port);

            playersMediatorInstance = Instantiate(playersMediatorPrefab, transform);
            Session = new HostSession(playersMediatorInstance);
            Session.Start();
        }

        public void Connect(Settings settings)
        {
            NetworkManager.Singleton
                .GetComponent<UnityTransport>()
                .SetConnectionData(settings.Address, settings.Port);

            playersMediatorInstance = Instantiate(playersMediatorPrefab, transform);
            Session = new ClientSession(playersMediatorInstance);
            Session.Start();
        }

        public void Disconnect()
        {
            Session.Stop();
            Destroy(playersMediatorInstance.gameObject);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetNameServerRpc(string newName, ServerRpcParams serverRpcParams = default)
        {
            var data = Players.Get(serverRpcParams.Receive.SenderClientId);

            data.PlayerName = newName;
            Players.Update(data);
        }

        [ServerRpc(RequireOwnership = true)]
        public void KickServerRpc(ulong id, ServerRpcParams serverRpcParams = default)
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