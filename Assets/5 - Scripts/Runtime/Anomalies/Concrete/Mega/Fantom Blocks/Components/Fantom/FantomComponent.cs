using System;
using System.Collections.Generic;
using R3;
using Unity.Netcode;
using UnityEngine;

namespace Anomalies
{
    public class FantomComponent : NetworkBehaviour
    {
        [SerializeField] private SightSensorComponent sensor;
        [SerializeField] private FantomVisualComponent visual;

        // Shared
        private readonly NetworkVariable<int> detected = new();

        // Local
        private readonly ReactiveProperty<bool> revealed = new();
        private IDisposable subscription;

        public ReadOnlyReactiveProperty<bool> Revealed => revealed;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            subscription = sensor.Detected
                .Subscribe(DetectedCallback)
                .AddTo(this);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            subscription?.Dispose();
        }

        private void DetectedCallback(bool value)
        {
            UpdateCounterRpc(value);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void UpdateCounterRpc(bool value, RpcParams rpcParams = default)
        {
            if (value)
            {
                detected.Value++;
            }
            else
            {
                detected.Value = Mathf.Max(detected.Value - 1, 0);
            }

            UpdateStateRpc(detected.Value > 0);
        }

        [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
        private void UpdateStateRpc(bool state)
        {
            revealed.Value = state;
        }

        [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
        public void UpdateColorRpc(ulong guildID)
        {
            var controller = FantomBlocksController.Singleton;
            var guild = controller.GetGuild(guildID);
            
            if (guild.Color.RGBA.a < 0.001)
            {
                return;
            }

            visual.SetColor(guild.Color.Gradient);
            sensor.SetColor(guild.Color.RGBA);
        }
    }
}