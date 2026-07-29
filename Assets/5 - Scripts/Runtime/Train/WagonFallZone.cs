using Managers;
using Unity.Netcode;
using UnityEngine;
using KinematicCharacterController;

namespace Train
{
    public class WagonFallZone : NetworkBehaviour
    {
        [SerializeField] private Transform respawnPoint;

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;

            var motor = other.GetComponent<KinematicCharacterMotor>() ?? other.GetComponentInParent<KinematicCharacterMotor>();

            if (motor != null)
            {
                var netObj = motor.GetComponent<NetworkObject>() ?? motor.GetComponentInParent<NetworkObject>();

                if (netObj != null)
                {
                    HandlePlayerFall(netObj.OwnerClientId, motor);
                }
            }
        }

        private void HandlePlayerFall(ulong clientId, KinematicCharacterMotor motor)
        {
            if (respawnPoint != null)
            {
                motor.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);
                motor.BaseVelocity = Vector3.zero;
            }

            int playerCount = NetworkManager.Singleton.ConnectedClients.Count;
            if (playerCount > 1)
            {
                var data = MultiplayerManager.Players.Get(clientId);
                if (!data.IsDead)
                {
                    GameManager.Instance.KillPlayerServerRpc(clientId);
                }
            }
        }
    }
}