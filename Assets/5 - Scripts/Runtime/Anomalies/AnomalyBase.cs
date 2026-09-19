using System;
using Unity.Netcode;
using UnityEngine;

namespace Anomalies
{
    public abstract class AnomalyBase : NetworkBehaviour, IAnomaly
    {
        public event Action OnAnomalyStateChanged;
        public static event Action OnAnyAnomalyStateChanged;

        private readonly NetworkVariable<bool> _isActiveNet = new();

        [SerializeField]
        private string _id;

        public string Id => _id;

        public bool IsActive
        {
            get => _isActiveNet.Value;
            set
            {
                if (value)
                    Activate();
                else
                    Deactivate();
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

            // ѕримен€ем уже существующее состо€ние.
            ApplyState(_isActiveNet.Value);
        }

        public override void OnNetworkDespawn()
        {
            OnDeactivate();

            _isActiveNet.OnValueChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(bool previousValue, bool newValue)
        {
            ApplyState(newValue);
            OnAnomalyStateChanged?.Invoke();
        }

        private void ApplyState(bool active)
        {
            if (active)
                OnActivate();
            else
                OnDeactivate();

            OnStateApplied(active);

            OnAnomalyStateChanged?.Invoke();
            OnAnyAnomalyStateChanged?.Invoke();
        }

        protected virtual void OnStateApplied(bool active)
        {
        }

        public void Activate()
        {
            if (!IsServer || _isActiveNet.Value)
                return;

            _isActiveNet.Value = true;
        }

        public void Deactivate()
        {
            if (!IsServer || !_isActiveNet.Value)
                return;

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