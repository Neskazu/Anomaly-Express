using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Network.Players
{
    public class PlayerDataProvider : IEnumerable<PlayerData>
    {
        public Action<PlayerData> OnUpdated;
        public Action<PlayerData> OnConnected;
        public Action<PlayerData> OnDisconnected;

        private readonly NetworkList<PlayerData> _players;

        public PlayerDataProvider(in NetworkList<PlayerData> players)
        {
            _players = players;
            _players.OnListChanged += Updated;
        }

        public PlayerData Get(ulong id)
        {
            return this.FirstOrDefault(data => data.ClientId == id);
        }

        public int IndexOf(ulong id)
        {
            for (var i = 0; i < _players.Count; i++)
                if (_players[i].ClientId == id)
                    return i;

            return -1;
        }

        public void Update(PlayerData data)
        {
            _players[IndexOf(data.ClientId)] = data;
        }

        private void Updated(NetworkListEvent<PlayerData> changeEvent)
        {
            switch (changeEvent.Type)
            {
                case NetworkListEvent<PlayerData>.EventType.Add:
                case NetworkListEvent<PlayerData>.EventType.Insert:
                    OnConnected?.Invoke(changeEvent.Value);
                    break;
                case NetworkListEvent<PlayerData>.EventType.Remove:
                case NetworkListEvent<PlayerData>.EventType.RemoveAt:
                    OnDisconnected?.Invoke(changeEvent.Value);
                    break;
                case NetworkListEvent<PlayerData>.EventType.Value:
                    OnUpdated?.Invoke(changeEvent.Value);
                    return;
                case NetworkListEvent<PlayerData>.EventType.Clear:
                case NetworkListEvent<PlayerData>.EventType.Full:
                default:
                    break;
            }
        }

        public IEnumerator<PlayerData> GetEnumerator()
        {
            return _players.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public PlayerData this[int i]
        {
            get => _players[i];
        }
    }
}