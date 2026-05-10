using KinematicCharacterController;
using Unity.Netcode;
using UnityEngine;

namespace Anomalies
{
    public class ParkourFallZone : MonoBehaviour
    {
        [SerializeField] private ParkourAnomaly _anomaly;

        private void OnTriggerEnter(Collider other)
        {
            if (_anomaly == null || !_anomaly.IsActive) return;

            var motor = other.GetComponent<KinematicCharacterMotor>() ?? other.GetComponentInParent<KinematicCharacterMotor>();

            if (motor != null)
            {
                var netObj = motor.GetComponent<NetworkObject>() ?? motor.GetComponentInParent<NetworkObject>();

                if (netObj != null && netObj.IsOwner)
                {
                    _anomaly.HandlePlayerFall(motor);
                }
            }
        }
    }
}