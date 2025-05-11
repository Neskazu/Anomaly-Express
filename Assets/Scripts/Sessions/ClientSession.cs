using Network.Players;
using Unity.Netcode;

namespace Sessions
{
    public class ClientSession : Session
    {
        public ClientSession(NetworkPlayersMediator mediator) 
            : base(mediator) {}

        public override void Start()
        {
            NetworkManager.Singleton.StartClient();
        }

        public override void Stop()
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}