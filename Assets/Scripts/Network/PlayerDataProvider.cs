using System;
using System.Collections;
using System.Collections.Generic;
using Session;
using Unity.Netcode;

namespace Network
{
    public class PlayerDataProvider : IDisposable, IEnumerable<PlayerData>
    {
        public event Action<PlayerData> OnChange;
        public event Action<PlayerData> OnAdd;
        public event Action<PlayerData> OnRemove;

        public int Count => players.Count;

        private readonly Session.Base.Session session;
        private readonly NetworkList<PlayerData> players;

        public PlayerData this[int index]
        {
            get => players[index];
            set => players[index] = value;
        }

        public PlayerDataProvider(Session.Base.Session session, NetworkList<PlayerData> players)
        {
            this.session = session;
            this.players = players;

            if (session is HostSession)
            {
                this.session.OnClientConnected += Add;
                this.session.OnClientDisconnected += Remove;
            }

            this.players.OnListChanged += Notify;
        }

        public void Dispose()
        {
            session.OnClientConnected -= Add;
            session.OnClientDisconnected -= Remove;

            players.OnListChanged -= Notify;
        }

        public void Change(PlayerData player)
        {
            for (var i = 0; i < players.Count; i++)
            {
                if (!players[i].Equals(player))
                    continue;

                players[i] = player;
                return;
            }

            throw new KeyNotFoundException($"Player with id {player.ClientId} does not exist");
        }

        public bool Find(ulong id, out int index, out PlayerData player)
        {
            for (var i = 0; i < players.Count; i++)
            {
                if (players[i].ClientId != id)
                    continue;

                player = players[i];
                index = i;
                return true;
            }

            index = -1;
            player = default;
            return false;
        }

        private void Add(ulong playerId)
        {
            var player = new PlayerData() { ClientId = playerId };

            players.Add(player);
        }

        private void Remove(ulong playerId)
        {
            var player = new PlayerData() { ClientId = playerId };

            players.Remove(player);
        }

        private void Notify(NetworkListEvent<PlayerData> changes)
        {
            switch (changes.Type)
            {
                case NetworkListEvent<PlayerData>.EventType.Add:
                case NetworkListEvent<PlayerData>.EventType.Insert:
                    OnAdd?.Invoke(changes.Value);
                    break;
                case NetworkListEvent<PlayerData>.EventType.Remove:
                case NetworkListEvent<PlayerData>.EventType.RemoveAt:
                    OnRemove?.Invoke(changes.Value);
                    break;
                case NetworkListEvent<PlayerData>.EventType.Value:
                    OnChange?.Invoke(changes.Value);
                    break;
                case NetworkListEvent<PlayerData>.EventType.Clear:
                case NetworkListEvent<PlayerData>.EventType.Full:
                default:
                    return;
            }
        }

        public IEnumerator<PlayerData> GetEnumerator()
        {
            return players.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}