using Controls;
using DG.Tweening;
using R3;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Components
{
    public class PlayerJump : MonoBehaviour
    {
        [SerializeField] private InputActionReference action;
        [SerializeField] private NetworkObject networkObject;
        [SerializeField] private PlayerAnimator playerAnimator;
        [SerializeField] private float jumpDelay = 0.1f;

        [Header("Jump Forgiveness")]
        public float jumpBufferTime = 0.07f;

        public bool IsJumpEnabled = true;
        public bool JumpRequested { get; set; }

        private float _lastJumpInputTime = -100f;
        private DG.Tweening.Tween _jumpDelayTween;

        private void Start()
        {
            if (!networkObject.IsOwner)
            {
                return;
            }

            InputManager.Singleton
                .Subscribe(action, ReactiveInputPhase.Started, OnJumpInput)
                .AddTo(this);
        }

        private void OnJumpInput()
        {
            _lastJumpInputTime = Time.time;
        }

        public void ProcessJumpBuffer(bool hasPermissions)
        {
            if (!IsJumpEnabled || !hasPermissions)
            {
                return;
            }

            var hasBufferedJump = (Time.time - _lastJumpInputTime) <= jumpBufferTime;

            if (hasBufferedJump)
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

            _jumpDelayTween = DOVirtual
                .DelayedCall(jumpDelay, () => { JumpRequested = true; })
                .SetLink(gameObject);
        }

        public void SetJumpEnabled(bool value) => IsJumpEnabled = value;

        private void OnDestroy()
        {
            _jumpDelayTween?.Kill();
        }
    }
}