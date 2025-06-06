using UnityEngine;

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
    public abstract void OnActivate();
    public abstract void OnDeactivate();
    public virtual void OnUpdate() { }
    private void Update()
    {
        if (IsActive)
            OnUpdate();
    }
}

