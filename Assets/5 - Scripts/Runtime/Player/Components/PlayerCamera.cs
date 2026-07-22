using System;
using UnityEngine;
using Anomalies;
using Controls;
using R3;
using UnityEngine.InputSystem;

namespace Player.Components
{
    public class PlayerCamera : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference action;

        [Header("References")]
        [SerializeField] private Transform body;
        [SerializeField] private Transform head;

        [Header("Head Bob Settings")]
        [SerializeField] private bool useHeadBob = true;
        [SerializeField] private float bobFrequency = 4f;
        [SerializeField] private float bobHorizontalAmplitude = 0.08f;
        [SerializeField] private float bobVerticalAmplitude = 0.08f;
        [SerializeField] private float bobSmoothing = 10f;

        [Header("Body Camera Sway")]
        [SerializeField] private bool useBodySway = true;
        [SerializeField] private float swayMultiplier = 0.3f;
        [SerializeField] private float swaySmoothness = 12f;
        [SerializeField] private float maxRoll = 1.2f;
        [SerializeField] private float maxPitchOffset = 0.5f;

        [SerializeField] private float rotationLimit = 85f;
        [SerializeField] private bool lockCursor = true;

        private IDisposable subscription;

        private Vector2 mouseAxis;
        private float verticalRotation;
        private float horizontalRotation;

        private float _bobTimer;
        private Vector3 _currentBobOffset;

        private float _currentRoll;
        private float _currentPitchOffset;

        private Camera _cam;
        private Transform _camTransform;

        public Transform Head => head;
        private static InputManager Input => InputManager.Singleton;

        private Vector3 _previousLocalVelocity;

        private void Awake()
        {
            _cam = Camera.main;
            if (_cam != null)
                _camTransform = _cam.transform;
        }

        private void OnEnable()
        {
            subscription = InputManager.Singleton
                .Subscribe<Vector2>(action, ReactiveInputPhase.Performed, OnMouseMove, true)
                .AddTo(this);

            if (lockCursor) Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDisable()
        {
            subscription.Dispose();

            if (lockCursor) Cursor.lockState = CursorLockMode.None;
        }

        private void LateUpdate()
        {
            if (_camTransform == null)
            {
                _cam = Camera.main;
                if (_cam != null) _camTransform = _cam.transform;
                else return;
            }

            FollowPlayer();
            HandleRotation();
        }

        private void FollowPlayer()
        {
            Transform targetHead = head;

            if (SplitControlAnomaly.IsSplitActive && PlayerController.HostInstance != null)
                targetHead = PlayerController.HostInstance.HeadTransform;

            if (targetHead == null)
                return;

            Vector3 targetPosition = targetHead.position;

            if (useHeadBob)
                targetPosition += CalculateHeadBobOffset();

            _camTransform.position = targetPosition;
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

                float x = Mathf.Cos(_bobTimer / 2f) * bobHorizontalAmplitude;
                float y = Mathf.Sin(_bobTimer) * bobVerticalAmplitude;

                Vector3 targetOffset = (_camTransform.right * x) + (_camTransform.up * y);
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
                    ApplyRotationWithBodySway();
                }
            }
            else
            {
                ApplyLocalRotation();
                if (PlayerController.LocalInstance != null && PlayerController.LocalInstance.IsOwner)
                    PlayerController.LocalInstance.SendCameraSyncServerRpc(verticalRotation, horizontalRotation);
            }
        }

        private void ApplyLocalRotation()
        {
            verticalRotation -= mouseAxis.y;
            horizontalRotation += mouseAxis.x;

            verticalRotation = Mathf.Clamp(verticalRotation, -rotationLimit, rotationLimit);

            ApplyRotationWithBodySway();
        }

        private void ApplyRotationWithBodySway()
        {
            float targetRoll = 0f;
            float targetPitchOffset = 0f;

            if (useBodySway && PlayerController.LocalInstance != null && body != null)
            {
                Vector3 velocity = PlayerController.LocalInstance.Motor.Velocity;
                Vector3 localVelocity = body.InverseTransformDirection(velocity);

                Vector3 acceleration = (localVelocity - _previousLocalVelocity) / Time.deltaTime;
                _previousLocalVelocity = localVelocity;
                targetRoll = Mathf.Clamp(-acceleration.x * 0.02f, -maxRoll, maxRoll);
                targetPitchOffset = Mathf.Clamp(-acceleration.z * 0.01f, -maxPitchOffset, maxPitchOffset);

                if (acceleration.magnitude < 0.5f)
                {
                    targetRoll = 0f;
                    targetPitchOffset = 0f;
                }
            }

            _currentRoll = Mathf.Lerp(_currentRoll, targetRoll, Time.deltaTime * swaySmoothness);
            _currentPitchOffset = Mathf.Lerp(_currentPitchOffset, targetPitchOffset, Time.deltaTime * swaySmoothness);

            _camTransform.localRotation = Quaternion.Euler(
                verticalRotation + _currentPitchOffset,
                horizontalRotation,
                _currentRoll
            );
        }

        private void OnMouseMove(Vector2 value)
        {
            mouseAxis = value;
        }
    }
}