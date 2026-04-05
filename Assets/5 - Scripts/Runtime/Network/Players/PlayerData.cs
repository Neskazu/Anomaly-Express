using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
    {
        public ulong ClientId;
        public FixedString64Bytes PlayerName;

        public bool IsReady;
        public bool IsDead;

        public Vector3 Velocity;
        public Vector3 Punch;

        public bool Equals(PlayerData other)
        {
            return ClientId == other.ClientId;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref IsReady);
            serializer.SerializeValue(ref IsDead);
            serializer.SerializeValue(ref Velocity);
            serializer.SerializeValue(ref Punch);
        }
    }
}