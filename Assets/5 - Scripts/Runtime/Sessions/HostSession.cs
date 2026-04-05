using Network;
using Network.Players;
using Unity.Netcode;

namespace Sessions
{
    public sealed class HostSession : Session
    {
        public HostSession(NetworkPlayersMediator mediator) 
            : base(mediator) { }

        public override void Start()
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += OnApprovalCallback;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedCallback;

            NetworkManager.Singleton.StartHost();
        }

        public override void Stop()
        {
            NetworkManager.Singleton.ConnectionApprovalCallback -= OnApprovalCallback;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedCallback;

            NetworkManager.Singleton.Shutdown();
        }

        private void OnApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved = true;
        }

        private void OnClientConnectedCallback(ulong id)
        {
            Mediator.Players.Add(new PlayerData { ClientId = id });
        }

        private void OnClientDisconnectedCallback(ulong id)
        {
            Mediator.Players.RemoveAt(Players.IndexOf(id));
        }
    }
}