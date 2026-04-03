using Managers;
using UnityEngine;
using Player;
using Anomalies;

namespace Player.Components
{
    public class PlayerCamera : MonoBehaviour
    {
        // Dependencies
        [SerializeField] private Transform body;
        [SerializeField] private Transform head;

        // Settings
        [SerializeField] private float rotationLimit = 85f;
        [SerializeField] private bool lockCursor = true;

        private Vector2 mouseAxis;
        private float verticalRotation;
        private float horizontalRotation;

        public Transform Head => head;

        private static InputManager Input => InputManager.Singleton;

        private void OnEnable()
        {
            Input.OnMouseMove += OnMouseMove;

            if (lockCursor)
                Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDisable()
        {
            Input.OnMouseMove -= OnMouseMove;
            if (lockCursor)
                Cursor.lockState = CursorLockMode.None;
        }

        private void LateUpdate()
        {
            FollowPlayer();
            HandleRotation();
        }

        private void FollowPlayer()
        {
            Transform targetHead = head;

            if (SplitControlAnomaly.IsSplitActive && PlayerController.HostInstance != null)
            {
                targetHead = PlayerController.HostInstance.HeadTransform;
            }

            if (targetHead != null)
            {
                Camera.main.transform.position = targetHead.position;
            }
        }

        private void HandleRotation()
        {
            if (SplitControlAnomaly.IsSplitActive && PlayerController.HostInstance != null && PlayerController.LocalInstance != null)
            {
                if (PlayerController.LocalInstance.CurrentPermissions.CanRotate)
                {
                    ApplyLocalRotation();
                    PlayerController.LocalInstance.SendCameraSyncServerRpc(verticalRotation, horizontalRotation);
                }
                else
                {
                    verticalRotation = PlayerController.HostInstance.SharedCamPitch.Value;
                    horizontalRotation = PlayerController.HostInstance.SharedCamYaw.Value;

                    Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
                }
            }
            else
            {
                ApplyLocalRotation();
            }
        }

        private void ApplyLocalRotation()
        {
            verticalRotation -= mouseAxis.y;
            horizontalRotation += mouseAxis.x;
            verticalRotation = Mathf.Clamp(verticalRotation, -rotationLimit, rotationLimit);

            Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
        }

        private void OnMouseMove(Vector2 value)
        {
            mouseAxis = value;
        }
    }
}