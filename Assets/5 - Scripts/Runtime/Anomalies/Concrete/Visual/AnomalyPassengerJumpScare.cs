using System.Collections;
using UnityEngine;
using Unity.Netcode;

namespace Anomalies.Concrete.Visual
{
    [DefaultExecutionOrder(200)]
    public class AnomalyPassengerJumpScare : AnomalyBase
    {
        [Header("Passengers")]
        [SerializeField] private GameObject normalPassenger;
        [SerializeField] private GameObject scaryPassenger;

        [Header("Head")]
        [SerializeField] private Transform headBone;

        [Header("Spawn")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float distanceFromPlayer = 0.8f;
        [SerializeField] private float fixedSpawnHeight = -1.9234f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [Tooltip("Резкий щелчок/хруст/удар при спавне (опционально)")]
        [SerializeField] private AudioClip impactSound;
        [Tooltip("Основной звук скримера (хрип/шипение/крик)")]
        [SerializeField] private AudioClip scareSound;

        [Header("Timing")]
        [SerializeField] private float minDelay = 0.5f;
        [SerializeField] private float maxDelay = 2f;
        [SerializeField] private float scareDuration = 1.2f;

        [Header("Creepy Head Settings")]
        [SerializeField] private Vector3 brokenNeckTilt = new Vector3(25f, 0f, 45f);
        [SerializeField] private float shakeAmount = 15f;
        [SerializeField] private float minTwitchInterval = 0.08f;
        [SerializeField] private float maxTwitchInterval = 0.35f;

        private bool _active;
        private bool _triggered;
        private Coroutine _routine;
        private Camera _camera;

        private Transform _cachedPlayer;

        private bool _isShaking;
        private float _twitchTimer;
        private Vector3 _currentTwitchOffset;
        private Quaternion _initialHeadLocalRot;

        protected override void OnActivate()
        {
            _active = true;
            _triggered = false;
            _camera = Camera.main;
        }

        protected override void OnDeactivate()
        {
            _active = false;
            _isShaking = false;

            if (_routine != null)
                StopCoroutine(_routine);

            if (audioSource != null && audioSource.isPlaying)
                audioSource.Stop();

            normalPassenger.SetActive(true);
            scaryPassenger.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_active || _triggered)
                return;

            var localPlayer = NetworkManager.Singleton.LocalClient?.PlayerObject;

            if (localPlayer == null)
                return;

            if (other.gameObject != localPlayer.gameObject)
                return;

            _cachedPlayer = localPlayer.transform;
            _triggered = true;
            _routine = StartCoroutine(JumpScareRoutine());
        }

        private IEnumerator JumpScareRoutine()
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            if (_camera == null)
                _camera = Camera.main;

            Transform cam = _camera.transform;

            if (spawnPoint != null)
            {
                scaryPassenger.transform.position = spawnPoint.position;
            }
            else
            {
                Vector3 flatForward = cam.forward;
                flatForward.y = 0f;

                if (flatForward.sqrMagnitude > 0.001f)
                {
                    flatForward.Normalize();
                }
                else if (_cachedPlayer != null)
                {
                    flatForward = _cachedPlayer.forward;
                    flatForward.y = 0f;
                    flatForward.Normalize();
                }

                Vector3 spawnPos = cam.position + flatForward * distanceFromPlayer;
                spawnPos.y = fixedSpawnHeight;

                scaryPassenger.transform.position = spawnPos;
            }

            Vector3 lookPos = cam.position;
            lookPos.y = scaryPassenger.transform.position.y;
            scaryPassenger.transform.LookAt(lookPos);

            normalPassenger.SetActive(false);
            scaryPassenger.SetActive(true);

            // ВОСПРОИЗВЕДЕНИЕ ЗВУКА
            if (audioSource != null)
            {
                if (impactSound != null)
                    audioSource.PlayOneShot(impactSound);

                if (scareSound != null)
                {
                    audioSource.clip = scareSound;
                    audioSource.Play();
                }
            }

            if (headBone != null)
            {
                _initialHeadLocalRot = headBone.localRotation;
            }

            _isShaking = true;
            _twitchTimer = 0f;

            yield return new WaitForSeconds(scareDuration);

            _isShaking = false;

            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            if (headBone != null)
            {
                headBone.localRotation = _initialHeadLocalRot;
            }

            normalPassenger.SetActive(true);
            scaryPassenger.SetActive(false);

            _active = false;
        }

        private void LateUpdate()
        {
            if (!_isShaking || headBone == null)
                return;

            _twitchTimer -= Time.deltaTime;

            if (_twitchTimer <= 0f)
            {
                _twitchTimer = Random.Range(minTwitchInterval, maxTwitchInterval);

                _currentTwitchOffset = new Vector3(
                    Random.Range(-shakeAmount, shakeAmount),
                    Random.Range(-shakeAmount, shakeAmount),
                    Random.Range(-shakeAmount, shakeAmount)
                );
            }

            headBone.localRotation = _initialHeadLocalRot
                                     * Quaternion.Euler(brokenNeckTilt)
                                     * Quaternion.Euler(_currentTwitchOffset);
        }
    }
}