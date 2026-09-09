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
            bool isGrounded = motor.GroundingStatus.IsStableOnGround;

            if (speed > threshold && isGrounded && !footAudioSource.isPlaying)
            {
                footAudioSource.Play();
            }
            else if ((speed <= threshold || !isGrounded) && footAudioSource.isPlaying)
            {
                footAudioSource.Stop();
            }
        }
    }
}