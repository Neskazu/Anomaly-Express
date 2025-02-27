using UnityEngine;

namespace Player.Components
{
    public class PlayerAudioFootstep : MonoBehaviour
    {
        // Dependencies
        [SerializeField] private Rigidbody rb;
        [SerializeField] private AudioSource footAudioSource;

        // Settings
        [SerializeField] private float threshold = 0.01f;

        private void FixedUpdate()
        {
            if (rb.linearVelocity.magnitude > threshold && !footAudioSource.isPlaying)
                footAudioSource.Play();

            else if (rb.linearVelocity.magnitude < threshold && footAudioSource.isPlaying)
                footAudioSource.Stop();
        }
    }
}