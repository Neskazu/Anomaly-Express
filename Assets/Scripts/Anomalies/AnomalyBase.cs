using UnityEngine;

namespace Anomalies
{
    public abstract class AnomalyBase : MonoBehaviour, IAnomaly
    {
        public bool IsActive { get; set; } = false;

        public void Activate()
        {
            if (IsActive) return;
            IsActive = true;
            OnActivate();
        }

        public void Deactivate()
        {
            if (!IsActive) return;
            OnDeactivate();
            IsActive = false;
        }

        protected abstract void OnActivate();
        protected abstract void OnDeactivate();
        protected virtual void OnUpdate() { }

        private void Update()
        {
            if (IsActive)
                OnUpdate();
        }
    }
}