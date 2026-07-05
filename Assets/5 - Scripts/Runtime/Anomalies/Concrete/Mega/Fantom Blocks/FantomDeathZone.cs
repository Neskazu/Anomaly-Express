using KinematicCharacterController;
using Managers;
using Unity.Netcode;
using UnityEngine;

namespace Anomalies
{
    public class FantomDeathZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var motor = other.GetComponent<KinematicCharacterMotor>()
                        ?? other.GetComponentInParent<KinematicCharacterMotor>()
                        ?? other.GetComponentInChildren<KinematicCharacterMotor>();

            if (motor == null)
            {
                Debug.LogWarning("Collider that fall in to death zone is not a player");
                return;
            }

            var no = motor.GetComponent<NetworkObject>()
                     ?? motor.GetComponentInParent<NetworkObject>()
                     ?? motor.GetComponentInChildren<NetworkObject>();

            if (no == null)
            {
                Debug.LogWarning("Found kinematic character but no NetworkObject is attached");
                return;
            }

            ResetPlayerPosition(motor);
        }

        private void ResetPlayerPosition(KinematicCharacterMotor motor)
        {
            motor.BaseVelocity = Vector3.zero;

            var point = GameManager.Instance.GetRandomSpawnPoint();

            if (point == null)
            {
                motor.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
            else
            {
                motor.SetPositionAndRotation(point.position, point.rotation);
            }
        }
    }
}