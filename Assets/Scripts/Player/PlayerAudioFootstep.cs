using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class PlayerAudioFootstep : NetworkBehaviour
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