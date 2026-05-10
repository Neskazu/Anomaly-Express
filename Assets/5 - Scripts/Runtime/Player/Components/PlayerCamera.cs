//saved old camera+ added bob(need refactoring)

using Managers;
using UnityEngine;
using Player;
using Anomalies;

namespace Player.Components
{
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] private Transform body;
        [SerializeField] private Transform head;

        [Header("Head Bob Settings")]
        [SerializeField] private bool useHeadBob = true;
        [SerializeField] private float bobFrequency = 4f; 
        [SerializeField] private float bobHorizontalAmplitude = 0.08f;
        [SerializeField] private float bobVerticalAmplitude = 0.08f;
        [SerializeField] private float bobSmoothing = 10f;

        [SerializeField] private float rotationLimit = 85f;
        [SerializeField] private bool lockCursor = true;

        private Vector2 mouseAxis;
        private float verticalRotation;
        private float horizontalRotation;

        // Для расчета Bob
        private float _bobTimer;
        private Vector3 _currentBobOffset;

        public Transform Head => head;
        private static InputManager Input => InputManager.Singleton;

        private void OnEnable()
        {
            Input.OnMouseMove += OnMouseMove;
            if (lockCursor) Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDisable()
        {
            Input.OnMouseMove -= OnMouseMove;
            if (lockCursor) Cursor.lockState = CursorLockMode.None;
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
                Vector3 targetPosition = targetHead.position;

                if (useHeadBob)
                {
                    targetPosition += CalculateHeadBobOffset();
                }

                Camera.main.transform.position = targetPosition;
            }
        }

        private Vector3 CalculateHeadBobOffset()
        {
            var pc = PlayerController.LocalInstance;
            if (pc == null || !pc.Motor.GroundingStatus.IsStableOnGround)
            {
                _bobTimer = 0;
                _currentBobOffset = Vector3.Lerp(_currentBobOffset, Vector3.zero, Time.deltaTime * bobSmoothing);
                return _currentBobOffset;
            }
            float speed = pc.Motor.Velocity.magnitude;
            if (speed < 0.1f)
            {
                _bobTimer = 0;
                _currentBobOffset = Vector3.Lerp(_currentBobOffset, Vector3.zero, Time.deltaTime * bobSmoothing);
            }
            else
            {
                _bobTimer += Time.deltaTime * speed * bobFrequency;
                float x = Mathf.Cos(_bobTimer / 2) * bobHorizontalAmplitude;
                float y = Mathf.Sin(_bobTimer) * bobVerticalAmplitude;

                Vector3 targetOffset = (Camera.main.transform.right * x) + (Camera.main.transform.up * y);
                _currentBobOffset = Vector3.Lerp(_currentBobOffset, targetOffset, Time.deltaTime * bobSmoothing);
            }

            return _currentBobOffset;
        }
        private void HandleRotation()
        {
            bool isSplit = SplitControlAnomaly.IsSplitActive && PlayerController.HostInstance != null;

            if (isSplit)
            {
                if (PlayerController.LocalInstance.CurrentPermissions.CanRotate)
                {
                    ApplyLocalRotation();

                    PlayerController.HostInstance.SendCameraSyncServerRpc(verticalRotation, horizontalRotation);
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
                if (PlayerController.LocalInstance != null && PlayerController.LocalInstance.IsOwner)
                {
                    PlayerController.LocalInstance.SendCameraSyncServerRpc(verticalRotation, horizontalRotation);
                }
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






//using Managers;
//using UnityEngine;
//using Player;
//using Anomalies;

//namespace Player.Components
//{
//    public class PlayerCamera : MonoBehaviour
//    {
//        // Dependencies
//        [SerializeField] private Transform body;
//        [SerializeField] private Transform head;

//        // Settings
//        [SerializeField] private float rotationLimit = 85f;
//        [SerializeField] private bool lockCursor = true;

//        private Vector2 mouseAxis;
//        private float verticalRotation;
//        private float horizontalRotation;

//        public Transform Head => head;

//        private static InputManager Input => InputManager.Singleton;

//        private void OnEnable()
//        {
//            Input.OnMouseMove += OnMouseMove;

//            if (lockCursor)
//                Cursor.lockState = CursorLockMode.Locked;
//        }

//        private void OnDisable()
//        {
//            Input.OnMouseMove -= OnMouseMove;
//            if (lockCursor)
//                Cursor.lockState = CursorLockMode.None;
//        }

//        private void LateUpdate()
//        {
//            FollowPlayer();
//            HandleRotation();
//        }

//        private void FollowPlayer()
//        {
//            Transform targetHead = head;

//            if (SplitControlAnomaly.IsSplitActive && PlayerController.HostInstance != null)
//            {
//                targetHead = PlayerController.HostInstance.HeadTransform;
//            }

//            if (targetHead != null)
//            {
//                Camera.main.transform.position = targetHead.position;
//            }
//        }

//        private void HandleRotation()
//        {
//            if (SplitControlAnomaly.IsSplitActive && PlayerController.HostInstance != null && PlayerController.LocalInstance != null)
//            {
//                if (PlayerController.LocalInstance.CurrentPermissions.CanRotate)
//                {
//                    ApplyLocalRotation();
//                    PlayerController.LocalInstance.SendCameraSyncServerRpc(verticalRotation, horizontalRotation);
//                }
//                else
//                {
//                    verticalRotation = PlayerController.HostInstance.SharedCamPitch.Value;
//                    horizontalRotation = PlayerController.HostInstance.SharedCamYaw.Value;

//                    Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
//                }
//            }
//            else
//            {
//                ApplyLocalRotation();
//            }
//        }

//        private void ApplyLocalRotation()
//        {
//            verticalRotation -= mouseAxis.y;
//            horizontalRotation += mouseAxis.x;
//            verticalRotation = Mathf.Clamp(verticalRotation, -rotationLimit, rotationLimit);

//            Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
//        }

//        private void OnMouseMove(Vector2 value)
//        {
//            mouseAxis = value;
//        }
//    }
//}