using Anomalies;
using KinematicCharacterController;
using Unity.Netcode;
using UnityEngine;

public class SplitFallZone : MonoBehaviour
{
    [SerializeField] private SplitControlAnomaly _anomaly;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("fall1");
        if (_anomaly == null || !_anomaly.IsActive) return;
        Debug.Log("fall2");
        var motor = other.GetComponent<KinematicCharacterMotor>() ?? other.GetComponentInParent<KinematicCharacterMotor>();

        if (motor != null)
        {
            Debug.Log("fall3");
            var netObj = motor.GetComponent<NetworkObject>() ?? motor.GetComponentInParent<NetworkObject>();

            if (netObj != null && netObj.IsOwner)
            {
                Debug.Log("fall3");
                _anomaly.HandlePlayerFall(motor);
            }
        }
    }
}
