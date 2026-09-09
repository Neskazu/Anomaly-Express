using KinematicCharacterController;
using UnityEngine;

namespace Player.Components
{
    public class PlayerAudioWindfall : MonoBehaviour
    {
        [SerializeField] private KinematicCharacterMotor motor;
        [SerializeField] private AudioSource windAudioSource;

        [SerializeField] private float minFallSpeed = -5f;
        [SerializeField] private float maxFallSpeed = -30f;

        [Header("Audio Settings")]
        [SerializeField] private float maxVolume = 0.1f;
        [SerializeField] private float fadeInSpeed = 0.5f;
        [SerializeField] private float fadeOutSpeed = 5f;

        private void Update()
        {
            float verticalVelocity = motor.BaseVelocity.y;
            float targetVolume = 0f;

            if (verticalVelocity < minFallSpeed && !motor.GroundingStatus.IsStableOnGround)
            {
                float fallIntensity = Mathf.InverseLerp(minFallSpeed, maxFallSpeed, verticalVelocity);

                targetVolume = fallIntensity * maxVolume;

                if (!windAudioSource.isPlaying)
                    windAudioSource.Play();
            }

            float currentFadeSpeed = targetVolume > windAudioSource.volume ? fadeInSpeed : fadeOutSpeed;

            windAudioSource.volume = Mathf.Lerp(windAudioSource.volume, targetVolume, Time.deltaTime * currentFadeSpeed);

            if (targetVolume == 0f && windAudioSource.volume < 0.01f && windAudioSource.isPlaying)
            {
                windAudioSource.Stop();
                windAudioSource.volume = 0f;
            }
        }
    }
}