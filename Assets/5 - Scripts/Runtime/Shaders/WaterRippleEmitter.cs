using KinematicCharacterController;
using UnityEngine;
using UnityEngine.Audio;
using Player;
using Core.Audio;

public class WaterRippleEmitter : MonoBehaviour
{
    [Header("Ripple Settings")]
    [SerializeField] private float minSpeed = 0.5f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float minInterval = 0.12f;
    [SerializeField] private float baseStrength = 0.35f;
    [SerializeField] private float maxStrength = 1.25f;

    [Header("Audio Settings - General")]
    [SerializeField] private AudioClip splashSound;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("3D Audio Distance")]
    [SerializeField] private float audioMinDistance = 2f;
    [SerializeField] private float audioMaxDistance = 15f;

    [Header("Audio Settings - Landing")]
    [SerializeField] private float minFallSpeed = 2f;
    [SerializeField] private float maxFallSpeed = 15f;
    [SerializeField] private float minSplashVolume = 0.3f;
    [SerializeField] private float maxSplashVolume = 1.0f;
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.2f;

    [Header("Audio Settings - Walking")]
    [SerializeField] private float walkingVolume = 0.15f;
    [SerializeField] private float stepSoundInterval = 0.35f;

    private float _nextRippleTime;
    private float _nextStepSoundTime;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (player == null || player.Motor == null)
            return;

        float fallSpeed = Mathf.Abs(Vector3.Dot(player.Motor.BaseVelocity, player.Motor.CharacterUp));

        if (fallSpeed < minFallSpeed)
            return;

        float t = Mathf.InverseLerp(minFallSpeed, maxFallSpeed, fallSpeed);

        if (splashSound != null)
        {
            float volume = Mathf.Lerp(minSplashVolume, maxSplashVolume, t);
            float pitch = Random.Range(minPitch, maxPitch);

            AudioUtility.Play3D(
                splashSound,
                player.transform.position,
                sfxMixerGroup,
                volume,
                pitch,
                audioMinDistance,
                audioMaxDistance
            );
        }

        if (WaterRippleManager.Instance != null)
        {
            float splashRippleStrength = Mathf.Lerp(baseStrength, maxStrength * 1.5f, t);
            WaterRippleManager.Instance.SpawnRipple(player.transform.position, splashRippleStrength);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        // ”¡–¿À» !player.IsOwner
        if (player == null || player.Motor == null || !player.Motor.GroundingStatus.IsStableOnGround)
            return;

        float speed = Vector3.ProjectOnPlane(player.Motor.BaseVelocity, player.Motor.CharacterUp).magnitude;
        if (speed < minSpeed)
            return;

        if (Time.time >= _nextStepSoundTime)
        {
            if (splashSound != null)
            {
                float pitch = Random.Range(1.1f, 1.4f);

                AudioUtility.Play3D(
                    splashSound,
                    player.transform.position,
                    sfxMixerGroup,
                    walkingVolume,
                    pitch,
                    audioMinDistance,
                    audioMaxDistance
                );
            }
            _nextStepSoundTime = Time.time + stepSoundInterval;
        }

        if (Time.time >= _nextRippleTime)
        {
            float t = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
            float strength = Mathf.Lerp(baseStrength, maxStrength, t);

            if (WaterRippleManager.Instance != null)
            {
                WaterRippleManager.Instance.SpawnRipple(player.transform.position, strength);
            }
            _nextRippleTime = Time.time + minInterval;
        }
    }
}