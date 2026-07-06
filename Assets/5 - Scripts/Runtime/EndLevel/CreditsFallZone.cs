using KinematicCharacterController;
using Unity.Netcode;
using UnityEngine;

public class CreditsFallZone : MonoBehaviour
{
    [SerializeField] private CreditsLevelController _level;

    private void OnTriggerEnter(Collider other)
    {
        var motor = other.GetComponent<KinematicCharacterMotor>() ??
                    other.GetComponentInParent<KinematicCharacterMotor>();

        if (motor == null)
            return;

        var netObj = motor.GetComponent<NetworkObject>() ??
                     motor.GetComponentInParent<NetworkObject>();

        if (netObj != null && netObj.IsOwner)
        {
            _level.HandlePlayerFall(motor);
        }
    }
}