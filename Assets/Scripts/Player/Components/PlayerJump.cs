using Managers;
using Unity.Netcode;
using UnityEngine;

namespace Player.Components
{
    public class PlayerJump : MonoBehaviour
    {
        [SerializeField] private NetworkObject networkObject;
        public bool IsJumpEnabled { get; private set; } = false;
        public bool JumpRequested { get; set; } 

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

            JumpRequested = true;
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