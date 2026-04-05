using KinematicCharacterController;
using UnityEngine;

namespace Player.Components
{
    public class PlayerAudioFootstep : MonoBehaviour
    {
        // Dependencies
        [SerializeField] private KinematicCharacterMotor motor;
        [SerializeField] private AudioSource footAudioSource;

        // Settings
        [SerializeField] private float threshold = 0.1f;

        private void FixedUpdate()
        {
            float speed = motor.BaseVelocity.magnitude;

            if (speed > threshold && !footAudioSource.isPlaying)
                footAudioSource.Play();
            else if (speed < threshold && footAudioSource.isPlaying)
                footAudioSource.Stop();
        }
    }
}