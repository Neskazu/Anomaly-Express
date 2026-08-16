using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
    {
        private ulong owner;

        public FixedString64Bytes PlayerName;
        public ulong CharacterId;

        public bool IsReady;
        public bool IsDead;

        public Vector3 Velocity;
        public Vector3 Punch;

        public ulong Owner => owner;

        public PlayerData(ulong clientId, ulong characterId)
        {
            owner = clientId;

            PlayerName = new FixedString64Bytes("Player");
            CharacterId = characterId;

            IsReady = false;
            IsDead = false;

            Velocity = Vector3.zero;
            Punch = Vector3.zero;
        }

        public bool Equals(PlayerData other)
        {
            return owner == other.owner;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref owner);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref CharacterId);
            serializer.SerializeValue(ref IsReady);
            serializer.SerializeValue(ref IsDead);
            serializer.SerializeValue(ref Velocity);
            serializer.SerializeValue(ref Punch);
        }
    }
}