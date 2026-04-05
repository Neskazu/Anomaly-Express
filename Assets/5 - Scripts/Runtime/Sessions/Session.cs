using System;
using Network.Players;
using Unity.Netcode;

namespace Sessions
{
    public abstract class Session : IDisposable
    {
        public PlayerDataProvider Players { get; private set; }
        protected NetworkPlayersMediator Mediator { get; private set; }

        protected Session(NetworkPlayersMediator mediator)
        {
            Mediator = mediator;
            Players = new PlayerDataProvider(Mediator.Players);
        }

        public void Dispose()
        {
            if (NetworkManager.Singleton)
                Stop();
        }

        public abstract void Start();
        public abstract void Stop();
    }
}