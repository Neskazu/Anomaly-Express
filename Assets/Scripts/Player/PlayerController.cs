using KinematicCharacterController;
using Managers;
using System;
using Unity.Netcode;
using UnityEngine;


namespace Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerController : MonoBehaviour,ICharacterController 
    {
        [SerializeField] private NetworkObject networkObject;
        [SerializeField] private Behaviour[] localComponents;
        public KinematicCharacterMotor Motor;
        [Header("Stable Movement")]
        public float MaxStableMoveSpeed = 10f;
        public float StableMovementSharpness = 15;
        public float OrientationSharpness = 10;

        [Header("Air Movement")]
        public float MaxAirMoveSpeed = 10f;
        public float AirAccelerationSpeed = 5f;
        public float Drag = 0.1f;

        [Header("Misc")]
        public bool RotationObstruction;
        public Vector3 Gravity = new Vector3(0, -30f, 0);
        public Transform MeshRoot;

        private Vector2 _rawInput;
        private Vector3 _moveInputVector;
        private Vector3 _lookInputVector;

        private static InputManager inputManager
            => InputManager.Singleton;
        [SerializeField] private Transform cameraTransform;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private bool isDead;
#endif
        private void Start()
        {
            if (!networkObject.IsOwner)
                return;

            foreach (Behaviour localComponent in localComponents)
                localComponent.enabled = true;
            //KCC
            Motor.CharacterController = this;
            inputManager.OnMoveAxisChanged += HandleInput;
            cameraTransform = Camera.main.transform;
        }
        private void HandleInput(Vector2 vector)
        {
            _rawInput = vector;
        }
        private void HandleMoveDirection()
        {
            Vector3 moveInputVector = new Vector3(_rawInput.x, 0f, _rawInput.y);
            moveInputVector = Vector3.ClampMagnitude(moveInputVector, 1f);

            if (cameraTransform != null)
            {
                Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(cameraTransform.forward, Motor.CharacterUp).normalized;
                if (cameraPlanarDirection.sqrMagnitude == 0f)
                {
                    cameraPlanarDirection = Vector3.ProjectOnPlane(cameraTransform.up, Motor.CharacterUp).normalized;
                }
                Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);
                moveInputVector = cameraPlanarRotation * moveInputVector;
                _lookInputVector = cameraPlanarDirection;
            }
            _moveInputVector = moveInputVector;
        }

        private void Update()
        {
            if (!networkObject.IsOwner)
                return;
            HandleMoveDirection();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Input.GetKeyDown(KeyCode.G) && networkObject.IsOwner)
            {
                if (!isDead)
                    GameManager.Instance.KillPlayerServerRpc(networkObject.OwnerClientId);
                else
                    GameManager.Instance.RevivePlayerServerRpc(networkObject.OwnerClientId);

                isDead = !isDead;
            }
#endif
        }
        // ICharacterController implementation
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (_lookInputVector != Vector3.zero && OrientationSharpness > 0f)
            {
                // Smoothly interpolate from current to target look direction
                Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

                // Set the current rotation (which will be used by the KinematicCharacterMotor)
                currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
            }
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            Vector3 targetMovementVelocity = Vector3.zero;
            if (Motor.GroundingStatus.IsStableOnGround)
            {
                // Reorient source velocity on current ground slope (this is because we don't want our smoothing to cause any velocity losses in slope changes)
                currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

                // Calculate target velocity
                Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
                Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized * _moveInputVector.magnitude;
                targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

                // Smooth movement Velocity
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
            }
            else
            {
                // Add move input
                if (_moveInputVector.sqrMagnitude > 0f)
                {
                    targetMovementVelocity = _moveInputVector * MaxAirMoveSpeed;

                    // Prevent climbing on un-stable slopes with air movement
                    if (Motor.GroundingStatus.FoundAnyGround)
                    {
                        Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                        targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
                    }

                    Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
                    currentVelocity += velocityDiff * AirAccelerationSpeed * deltaTime;
                }

                // Gravity
                currentVelocity += Gravity * deltaTime;

                // Drag
                currentVelocity *= (1f / (1f + (Drag * deltaTime)));
            }
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
        }

        public void PostGroundingUpdate(float deltaTime)
        {
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }
    }
}