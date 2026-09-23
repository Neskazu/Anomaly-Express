using System;
using Unity.Netcode;
using UnityEngine;

namespace Nac.Network
{
    public class LocalLoopbackTransport : NetworkTransport
    {
        private bool isStarted;
        private bool isServer;

        public override ulong ServerClientId => 0;

        public override void DisconnectLocalClient()
        {
            isStarted = false;
            isServer = false;
        }

        public override void DisconnectRemoteClient(ulong clientId)
        {
        }

        public override ulong GetCurrentRtt(ulong clientId)
        {
            return 0;
        }

        public override void Initialize(NetworkManager networkManager = null)
        {
        }

        public override NetworkEvent PollEvent(out ulong clientId, out ArraySegment<byte> payload, out float receiveTime)
        {
            clientId = 0;
            payload = default;
            receiveTime = Time.realtimeSinceStartup;
            return NetworkEvent.Nothing;
        }

        public override void Send(ulong clientId, ArraySegment<byte> data, NetworkDelivery delivery)
        {
        }

        public override void Shutdown()
        {
            isStarted = false;
            isServer = false;
        }

        public override bool StartClient()
        {
            isStarted = true;
            isServer = false;
            return true;
        }

        public override bool StartServer()
        {
            isStarted = true;
            isServer = true;
            return true;
        }
    }
}
