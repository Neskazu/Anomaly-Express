using System;
using System.Linq;
using Managers;
using Nac;
using R3;
using Unity.Netcode;
using UnityEngine;

namespace Anomalies
{
    public class FantomComponent : NetworkBehaviour
    {
        [SerializeField] private SensorComponent sensor;
        [SerializeField] private FantomVisualComponent visual;

        [SerializeField] private GameObject collision;

        // Shared
        private readonly NetworkVariable<int> detected = new();

        // Local
        private readonly ReactiveProperty<bool> revealed = new();
        private readonly CompositeDisposable compositeDisposable = new();

        public ReadOnlyReactiveProperty<bool> Revealed => revealed;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            sensor.Detected
                .Subscribe(DetectedCallback)
                .AddTo(compositeDisposable);

            revealed
                .Subscribe(UpdateCollision)
                .AddTo(compositeDisposable);
        }

        public override void OnNetworkDespawn()
        {
            compositeDisposable?.Dispose();

            base.OnNetworkDespawn();
        }

        private void DetectedCallback(bool value)
        {
            UpdateCounterRpc(value);
        }

        private void UpdateCollision(bool state)
        {
            if (state)
            {
                var controller = FantomBlocksController.Singleton;
                var localClientId = NetworkManager.Singleton.LocalClientId;
                var guilds = controller.GetGuilds(localClientId);
                var components = guilds.SelectMany(g => g.Components);

                if (components.Contains(this))
                {
                    return;
                }

                collision.SetActive(true);
                return;
            }

            collision.SetActive(false);
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
        private void UpdateStateRpc(bool state, RpcParams rpcParams = default)
        {
            revealed.Value = state;
        }

        [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
        public void UpdateColorRpc(ulong clientId)
        {
            var info = PlayersManager.Instance.GetPlayerCharacter(clientId);

            visual.SetColor(info.Gradient);
            if (sensor is SightSensorComponent sightSensorComponent)
            {
                sightSensorComponent.SetColor(info.Color);
            }
        }
    }
}