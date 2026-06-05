using KinematicCharacterController;
using UnityEngine;
using Player;

public class WaterRippleEmitter : MonoBehaviour
{
    [SerializeField] private float minSpeed = 0.5f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float minInterval = 0.12f;
    [SerializeField] private float baseStrength = 0.35f;
    [SerializeField] private float maxStrength = 1.25f;

    private float _nextRippleTime;

    private void OnTriggerStay(Collider other)
    {
        if (Time.time < _nextRippleTime)
            return;

        var player = other.GetComponentInParent<PlayerController>();
        if (player == null)
            return;

        if (!player.IsOwner)
            return;

        if (player.Motor == null || !player.Motor.GroundingStatus.IsStableOnGround)
            return;

        float speed = Vector3.ProjectOnPlane(player.Motor.BaseVelocity, player.Motor.CharacterUp).magnitude;
        if (speed < minSpeed)
            return;

        float t = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
        float strength = Mathf.Lerp(baseStrength, maxStrength, t);

        if (WaterRippleManager.Instance != null)
        {
            WaterRippleManager.Instance.SpawnRipple(player.transform.position, strength);
        }

        _nextRippleTime = Time.time + minInterval;
    }
}