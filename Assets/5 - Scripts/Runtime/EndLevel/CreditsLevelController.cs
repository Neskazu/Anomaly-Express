using System.Collections;
using KinematicCharacterController;
using Unity.Netcode;
using UnityEngine;
using Player.Components;

public class CreditsLevelController : NetworkBehaviour
{
    [SerializeField] private Transform _respawnPoint;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        StartCoroutine(EnableJump());
    }

    private IEnumerator EnableJump()
    {
        while (NetworkManager.Singleton.LocalClient?.PlayerObject == null)
            yield return null;

        var player = NetworkManager.Singleton.LocalClient.PlayerObject;
        var jump = player.GetComponent<PlayerJump>();

        if (jump != null)
            jump.IsJumpEnabled = true;
    }

    public void HandlePlayerFall(KinematicCharacterMotor motor)
    {
        if (_respawnPoint == null)
            return;

        motor.SetPositionAndRotation(_respawnPoint.position, _respawnPoint.rotation);
        motor.BaseVelocity = Vector3.zero;
    }
}