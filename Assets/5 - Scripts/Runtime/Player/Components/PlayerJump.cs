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
        [SerializeField]
        public bool IsJumpEnabled;
        public bool JumpRequested { get; set; }
        [SerializeField]
        private float jumpDelay = 0.1f;

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

            if (IsJumpEnabled)
            {
                playerAnimator?.TriggerJump();

                DOVirtual.DelayedCall(jumpDelay, () =>
                {
                    if (this != null)
                    {
                        JumpRequested = true;
                    }
                }).SetTarget(this);
            }
        }

        public void SetJumpEnabled(bool value)
        {
            IsJumpEnabled = value;
        }

        private void OnDestroy()
        {
            if (InputManager.Singleton != null)
                InputManager.Singleton.OnJump -= OnJumpInput;
        }
    }
}