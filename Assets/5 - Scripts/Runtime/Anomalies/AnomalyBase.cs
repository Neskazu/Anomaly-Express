using System;
using Unity.Netcode;
using UnityEngine;

namespace Anomalies
{
    public abstract class AnomalyBase : NetworkBehaviour, IAnomaly
    {
        public static event Action OnAnomalyStateChanged;

        private string _anomalyId;
        public string Id => _anomalyId;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_anomalyId))
            {
                _anomalyId = Guid.NewGuid().ToString();
            }
        }

        private readonly NetworkVariable<bool> _isActiveNet = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public bool IsActive
        {
            get => _isActiveNet.Value;
            set
            {
                if (value) Activate();
                else Deactivate();
            }
        }

        public override void OnNetworkSpawn()
        {
            _isActiveNet.OnValueChanged += HandleStateChanged;

            if (_isActiveNet.Value)
            {
                OnActivate();
                OnAnomalyStateChanged?.Invoke();
            }
        }

        public override void OnNetworkDespawn()
        {
            _isActiveNet.OnValueChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(bool previousValue, bool newValue)
        {
            if (newValue)
                OnActivate();
            else
                OnDeactivate();

            OnAnomalyStateChanged?.Invoke();
        }

        public void Activate()
        {
            if (!IsServer || _isActiveNet.Value) return;

            _isActiveNet.Value = true;
        }

        public void Deactivate()
        {
            if (!IsServer || !_isActiveNet.Value) return;

            _isActiveNet.Value = false;
        }

        protected abstract void OnActivate();
        protected abstract void OnDeactivate();
        protected virtual void OnUpdate() { }

        private void Update()
        {
            if (_isActiveNet.Value)
                OnUpdate();
        }
    }
}