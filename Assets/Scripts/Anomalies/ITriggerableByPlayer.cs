using UnityEngine;

public interface ITriggerableByPlayer : IAnomaly
{
    void OnPlayerEnterZone();


    void OnPlayerExitZone();
}
