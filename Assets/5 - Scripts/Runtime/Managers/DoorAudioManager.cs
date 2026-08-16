using System.Collections;
using UnityEngine;

namespace Train
{
    [RequireComponent(typeof(AudioSource))]
    public class DoorAudioManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DoorController doorController;
        [SerializeField] private AudioSource audioSource;

        [Header("Audio Settings")]
        [SerializeField] private AudioClip doorSound;

        [SerializeField] private float closeSoundDelay = 0.5f;

        [Header("Shake Settings")]
        [SerializeField] private float minPitch = 0.8f;
        [SerializeField] private float maxPitch = 1.2f;

        [SerializeField] private float timeBetweenShakes = 0.15f;

        private void Awake()
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (doorController == null) doorController = GetComponentInParent<DoorController>();

            audioSource.spatialBlend = 1f;
        }

        private void OnEnable()
        {
            if (doorController != null)
            {
                doorController.OnDoorStateChanged += HandleDoorStateChanged;
                doorController.OnDoorShaken += HandleDoorShaken;
            }
        }

        private void OnDisable()
        {
            if (doorController != null)
            {
                doorController.OnDoorStateChanged -= HandleDoorStateChanged;
                doorController.OnDoorShaken -= HandleDoorShaken;
            }
        }

        private void HandleDoorStateChanged(DoorController door, bool isOpen)
        {
            if (doorSound == null) return;

            StopAllCoroutines();

            if (isOpen)
            {
                audioSource.pitch = 1f;
                audioSource.PlayOneShot(doorSound);
            }
            else
            {
                StartCoroutine(PlaySoundWithDelayRoutine(closeSoundDelay));
            }
        }

        private void HandleDoorShaken()
        {
            if (doorSound == null) return;

            StopAllCoroutines();
            StartCoroutine(PlayShakeSoundsRoutine());
        }

        private IEnumerator PlaySoundWithDelayRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            audioSource.pitch = 1f; 
            audioSource.PlayOneShot(doorSound);
        }

        private IEnumerator PlayShakeSoundsRoutine()
        {
            for (int i = 0; i < 3; i++)
            {
                audioSource.pitch = Random.Range(minPitch, maxPitch);
                audioSource.PlayOneShot(doorSound);

                yield return new WaitForSeconds(timeBetweenShakes);
            }

            audioSource.pitch = 1f;
        }
    }
}