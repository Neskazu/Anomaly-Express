using UnityEngine;

public interface IAnomaly
{
    void Activate();

    void Deactivate();

    bool IsActive { get; set; }
}
