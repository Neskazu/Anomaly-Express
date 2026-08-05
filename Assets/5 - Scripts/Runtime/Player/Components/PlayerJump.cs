using Controls;
using DG.Tweening;
using R3;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Components
{
    public class PlayerJump : NetworkBehaviour
    {
        [SerializeField] private InputActionReference action;
        [SerializeField] private PlayerAnimator playerAnimator;
        [SerializeField] private float jumpDelay = 0.1f;

        [Header("Jump Forgiveness")]
        public float jumpBufferTime = 0.07f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip jumpSound;
        [SerializeField][Range(0.8f, 1.0f)] private float minPitch = 0.95f;
        [SerializeField][Range(1.0f, 1.2f)] private float maxPitch = 1.05f;

        public bool IsJumpEnabled = true;
        public bool JumpRequested { get; set; }

        private float _lastJumpInputTime = -100f;
        private DG.Tweening.Tween _jumpDelayTween;

        private void Start()
        {
            if (!IsOwner) return;

            InputManager.Singleton
                .Subscribe(action, ReactiveInputPhase.Started, OnJumpInput)
                .AddTo(this);
        }

        private void OnJumpInput()
        {
            _lastJumpInputTime = Time.time;
        }

        public void ProcessJumpBuffer(bool hasPermissions, bool isGrounded)
        {
            if (!IsJumpEnabled || !hasPermissions)
            {
                return;
            }

            var hasBufferedJump = (Time.time - _lastJumpInputTime) <= jumpBufferTime;

            if (hasBufferedJump && isGrounded)
            {
                if (_jumpDelayTween != null && _jumpDelayTween.IsActive())
                {
                    return;
                }

                _lastJumpInputTime = -100f;
                ExecuteJumpSequence();
            }
        }

        private void ExecuteJumpSequence()
        {
            playerAnimator?.TriggerJump();

            PlaySoundWithRandomPitch();
            if (NetworkManager.Singleton.IsConnectedClient || IsServer)
            {
                PlayJumpSoundServerRpc();
            }

            _jumpDelayTween = DOVirtual
                .DelayedCall(jumpDelay, () => { JumpRequested = true; })
                .SetLink(gameObject);
        }

        private void PlaySoundWithRandomPitch()
        {
            if (audioSource != null && jumpSound != null)
            {
                audioSource.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
                audioSource.PlayOneShot(jumpSound);
            }
        }

        [ServerRpc]
        private void PlayJumpSoundServerRpc() => PlayJumpSoundClientRpc();

        [ClientRpc]
        private void PlayJumpSoundClientRpc()
        {
            if (IsOwner) return;
            PlaySoundWithRandomPitch();
        }

        public void SetJumpEnabled(bool value) => IsJumpEnabled = value;

        public override void OnDestroy()
        {
            base.OnDestroy();
            _jumpDelayTween?.Kill();
        }
    }
}