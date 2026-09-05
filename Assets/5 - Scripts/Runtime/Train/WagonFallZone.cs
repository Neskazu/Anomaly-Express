using Managers;
using Unity.Netcode;
using UnityEngine;
using KinematicCharacterController;
using Nac;

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
                    HandlePlayerFallServer(netObj.OwnerClientId);

                    ClientRpcParams clientRpcParams = new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new ulong[] { netObj.OwnerClientId }
                        }
                    };

                    TeleportPlayerClientRpc(netObj.NetworkObjectId, respawnPoint.position, respawnPoint.rotation, clientRpcParams);
                }
            }
        }

        private void HandlePlayerFallServer(ulong clientId)
        {
            int playerCount = NetworkManager.Singleton.ConnectedClients.Count;
            if (playerCount > 1)
            {
                var data = PlayersManager.Instance.GetPlayerData(clientId);

                if (!data.IsDead)
                {
                    GameManager.Instance.KillPlayerServerRpc(clientId);
                }
            }
        }

        [ClientRpc]
        private void TeleportPlayerClientRpc(ulong networkObjectId, Vector3 position, Quaternion rotation, ClientRpcParams clientRpcParams = default)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
            {
                var motor = netObj.GetComponent<KinematicCharacterMotor>() ?? netObj.GetComponentInChildren<KinematicCharacterMotor>();

                if (motor != null)
                {
                    motor.SetPositionAndRotation(position, rotation);
                    motor.BaseVelocity = Vector3.zero;
                }
            }
        }
    }
}