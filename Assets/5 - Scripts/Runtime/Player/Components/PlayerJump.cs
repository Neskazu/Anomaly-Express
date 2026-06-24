using DG;
using DG.Tweening;
using Managers;
using Unity.Netcode;
using UnityEngine;

namespace Player.Components
{
    public class PlayerJump : MonoBehaviour
    {
        [SerializeField] private NetworkObject networkObject;
        [SerializeField] private PlayerAnimator playerAnimator;
        [SerializeField] private float jumpDelay = 0.1f;

        public bool IsJumpEnabled = true;
        public bool JumpRequested { get; set; }

        private DG.Tweening.Tween _jumpDelayTween;

        private void Start()
        {
            if (!networkObject.IsOwner) return;
            InputManager.Singleton.OnJump += OnJumpInput;
        }

        private void OnJumpInput()
        {
            var controller = PlayerController.LocalInstance;

            if (Anomalies.SplitControlAnomaly.IsSplitActive && controller != null)
            {
                if (!controller.CurrentPermissions.CanJump) return;
            }

            if (!IsJumpEnabled || controller == null) return;

            if (!controller.Motor.GroundingStatus.IsStableOnGround) return;

            if (_jumpDelayTween != null && _jumpDelayTween.IsActive()) return;

            playerAnimator?.TriggerJump();

            _jumpDelayTween = DOVirtual.DelayedCall(jumpDelay, () =>
            {
                JumpRequested = true;
            }).SetLink(gameObject);
        }

        public void SetJumpEnabled(bool value) => IsJumpEnabled = value;

        private void OnDestroy()
        {
            if (InputManager.Singleton != null)
                InputManager.Singleton.OnJump -= OnJumpInput;

            _jumpDelayTween?.Kill();
        }
    }
}