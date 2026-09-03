using System;
using Nac.Extensions;
using R3;
using Unity.Netcode;
using UnityEngine;

namespace Anomalies
{
    public abstract class AnomalyBase : NetworkBehaviour, IAnomaly
    {
        public static event Action OnAnomalyStateChanged;

        private readonly NetworkVariable<bool> _isActiveNet = new();

        [SerializeField]
        private string _id;

        public string Id => _id;

        public bool IsActive
        {
            get => _isActiveNet.Value;
            set
            {
                if (value) Activate();
                else Deactivate();
            }
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_id))
            {
                _id = Guid.NewGuid().ToString();
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
            OnDeactivate();

            _isActiveNet.OnValueChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(bool previousValue, bool newValue)
        {
            if (newValue)
            {
                OnActivate();
            }
            else
            {
                OnDeactivate();
            }

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

        protected virtual void OnUpdate()
        {
        }

        private void Update()
        {
            if (_isActiveNet.Value)
            {
                OnUpdate();
            }
        }
    }
}