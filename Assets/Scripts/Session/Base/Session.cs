using System;
using Unity.Netcode;

namespace Session.Base
{
    /// <summary>
    /// Абстрактная сетевая сессия, которая может быть реализована как клиент, сервер или гибрид.
    /// </summary>
    public abstract class Session : IDisposable
    {
        public event Action<ulong> OnClientConnected;
        public event Action<ulong> OnClientDisconnected;

        protected readonly NetworkManager Network;

        protected Session()
        {
            Network = NetworkManager.Singleton;

            Network.OnClientConnectedCallback += OnClientConnectedCallback;
            Network.OnClientDisconnectCallback += OnClientDisconnectedCallback;
        }

        public virtual void Dispose()
        {
            Network.OnClientConnectedCallback -= OnClientConnected;
            Network.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        public abstract void Start();
        public abstract void Stop();

        protected virtual void OnClientConnectedCallback(ulong clientId)
        {
            OnClientConnected?.Invoke(clientId);
        }

        protected virtual void OnClientDisconnectedCallback(ulong clientId)
        {
            OnClientDisconnected?.Invoke(clientId);
        }
    }
}