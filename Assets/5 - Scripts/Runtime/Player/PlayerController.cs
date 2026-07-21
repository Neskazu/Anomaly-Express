using Anomalies;
using KinematicCharacterController;
using Managers;
using Player.Components;
using Player.Input;
using System.Collections.Generic;
using Controls;
using R3;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerController : NetworkBehaviour, ICharacterController
    {
        [SerializeField] private Rigidbody rb;

        [SerializeField] private NetworkObject networkObject;
        [SerializeField] private Behaviour[] localComponents;
        [SerializeField] private PlayerJump jumpComponent;
        [SerializeField] private float jumpForce = 10f;

        [Header("Inputs")]
        [SerializeField] private InputActionReference movementActionReference;

        //visual
        [SerializeField] private GameObject[] characterPrefabs;
        public Transform MeshRoot;

        public KinematicCharacterMotor Motor;
        [Header("Stable Movement")]
        public float MaxStableMoveSpeed = 10f;
        public float StableMovementSharpness = 15;
        public float OrientationSharpness = 10;

        [Header("Air Movement")]
        public float MaxAirMoveSpeed = 10f;
        public float AirAccelerationSpeed = 5f;
        public float Drag = 0.1f;
        public float AirScalableForwardSpeed = 10f;

        [Header("Misc")] public bool RotationObstruction;
        public Vector3 Gravity = new Vector3(0, -30f, 0);

        private Vector2 _rawInput;
        private Vector3 _moveInputVector;
        private Vector3 _lookInputVector;

        public Vector3 PunchVelocity;

        public static ulong LocalPlayerId { get; private set; }

        private static InputManager InputManager
            => InputManager.Singleton;

        [SerializeField] private Transform cameraTransform;

        // --- split control ---
        public Transform HeadTransform;
        public static PlayerController HostInstance { get; private set; }
        public static PlayerController LocalInstance { get; private set; }
        public PlayerPermissions CurrentPermissions => _currentPermissions;

        public NetworkVariable<float> SharedCamPitch = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> SharedCamYaw = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> CharacterId = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<Quaternion> NetworkBodyRotation = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> NetworkSpeed = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public Transform CameraTransform => cameraTransform;
        private float _lastSentPitch;
        private float _lastSentYaw;

        private Dictionary<ulong, Vector2> _sharedMoveInputs = new Dictionary<ulong, Vector2>();
        private bool _sharedJumpInput = false;
        private PlayerPermissions _currentPermissions;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private bool isDead;
#endif

        private void OnEnable()
        {
            AnomalyBase.OnAnomalyStateChanged += RefreshPermissions;

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnNetworkChanged;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnNetworkChanged;
            }

            RefreshPermissions();
        }

        private void OnDisable()
        {
            AnomalyBase.OnAnomalyStateChanged -= RefreshPermissions;

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnNetworkChanged;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnNetworkChanged;
            }
        }

        public override void OnNetworkSpawn()
        {
            CharacterId.OnValueChanged += OnCharacterChanged;
            ApplyCharacter(CharacterId.Value);
        }

        private void Start()
        {
            if (OwnerClientId == NetworkManager.ServerClientId)
            {
                HostInstance = this;
            }

            if (IsOwner)
            {
                LocalInstance = this;
                LocalPlayerId = OwnerClientId;

                foreach (var localComponent in localComponents)
                {
                    localComponent.enabled = true;
                }

                Motor.CharacterController = this;
                cameraTransform = Camera.main.transform;

                InputManager
                    .Subscribe<Vector2>(movementActionReference, InputActionPhase.Performed, HandleInput)
                    .AddTo(this);
            }

            RefreshPermissions();
        }

        private void HandleInput(Vector2 vector)
        {
            var x = _currentPermissions.CanMoveHorizontal ? vector.x : 0f;
            var y = _currentPermissions.CanMoveVertical ? vector.y : 0f;

            _rawInput = new Vector2(x, y);
        }

        private void HandleMoveDirection()
        {
            Vector3 moveInputVector = new Vector3(_rawInput.x, 0f, _rawInput.y);
            moveInputVector = Vector3.ClampMagnitude(moveInputVector, 1f);

            if (cameraTransform)
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
            if (!IsOwner) return;

            // jump buffer
            if (jumpComponent != null)
            {
                bool hasPermissions = !SplitControlAnomaly.IsSplitActive || CurrentPermissions.CanJump;
                jumpComponent.ProcessJumpBuffer(hasPermissions);
            }

            if (SplitControlAnomaly.IsSplitActive)
            {
                if (!IsServer)
                {
                    bool wantJump = jumpComponent != null && jumpComponent.JumpRequested;
                    HostInstance.SendMovementInputServerRpc(_rawInput, wantJump);
                    if (jumpComponent != null) jumpComponent.JumpRequested = false;
                    NetworkBodyRotation.Value = Motor.TransientRotation;
                    return;
                }
                else
                {
                    Vector2 combinedMove = _rawInput;
                    bool combinedJump = (jumpComponent != null && jumpComponent.JumpRequested);

                    foreach (var move in _sharedMoveInputs.Values)
                        combinedMove += move;

                    combinedMove = new Vector2(Mathf.Clamp(combinedMove.x, -1f, 1f), Mathf.Clamp(combinedMove.y, -1f, 1f));
                    if (_sharedJumpInput) combinedJump = true;

                    ApplyCombinedInput(combinedMove, combinedJump);

                    _sharedJumpInput = false;
                    _sharedMoveInputs.Clear();
                }
            }
            else
            {
                HandleMoveDirection();
                if (cameraTransform != null)
                {
                    float pitch = cameraTransform.localEulerAngles.x;
                    if (pitch > 180) pitch -= 360;
                    float yaw = cameraTransform.eulerAngles.y;

                    if (Mathf.Abs(pitch - _lastSentPitch) > 1f || Mathf.Abs(yaw - _lastSentYaw) > 1f)
                    {
                        SendCameraSyncServerRpc(pitch, yaw);
                        _lastSentPitch = pitch;
                        _lastSentYaw = yaw;
                    }

                    float currentSpeed = Motor.BaseVelocity.magnitude;
                    if (Mathf.Abs(NetworkSpeed.Value - currentSpeed) > 0.1f)
                    {
                        NetworkSpeed.Value = currentSpeed;
                    }

                    if (Quaternion.Angle(NetworkBodyRotation.Value, Motor.TransientRotation) > 0.1f)
                    {
                        NetworkBodyRotation.Value = Motor.TransientRotation;
                    }
                }
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD

            //if (Input.GetKeyDown(KeyCode.G) && networkObject.IsOwner)
            //{
            //    if (!isDead)
            //        GameManager.Instance.KillPlayerServerRpc(networkObject.OwnerClientId);
            //    else
            //        GameManager.Instance.RevivePlayerServerRpc(networkObject.OwnerClientId);

            //    isDead = !isDead;
            //}
#endif
        }

        [Header("Rotation Limits")]
        public float MaxLookAngle = 70f;

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (SplitControlAnomaly.IsSplitActive)
            {
                if (!IsServer) return;
                currentRotation = Quaternion.Euler(0f, SharedCamYaw.Value, 0f);
                return;
            }

            if (!_currentPermissions.CanRotate) return;

            Vector3 cameraDir = _lookInputVector;
            Vector3 moveDir = _moveInputVector;

            if (moveDir.sqrMagnitude > 0.01f)
            {
                Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, moveDir, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;
                currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
            }
            else
            {
                float angleBetween = Vector3.SignedAngle(Motor.CharacterForward, cameraDir, Motor.CharacterUp);
                if (Mathf.Abs(angleBetween) > MaxLookAngle)
                {
                    Quaternion targetRot = Quaternion.LookRotation(cameraDir, Motor.CharacterUp);
                    currentRotation = Quaternion.Slerp(currentRotation, targetRot, 1 - Mathf.Exp(-OrientationSharpness * deltaTime));
                }
            }
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (SplitControlAnomaly.IsSplitActive && !IsServer)
            {
                currentVelocity = Vector3.zero;
                return;
            }

            Vector3 targetMovementVelocity;

            if (Motor.GroundingStatus.IsStableOnGround)
            {
                // Handle jump logic: check if jump is requested AND enabled for the current location
                if (jumpComponent != null && jumpComponent.JumpRequested && jumpComponent.IsJumpEnabled)
                {
                    // 1. Forcefully unground the motor to initiate jump
                    Motor.ForceUnground();

                    // 2. Apply upward impulse
                    currentVelocity += Motor.CharacterUp * jumpForce;

                    // 3. Immediately consume the jump request
                    jumpComponent.JumpRequested = false;
                }
                else
                {
                    // Reset jump request if grounded but not jumping to prevent late triggers
                    if (jumpComponent != null) jumpComponent.JumpRequested = false;

                    // Reorient source velocity on current ground slope
                    currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

                    Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
                    Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized * _moveInputVector.magnitude;

                    targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

                    // Smooth movement velocity
                    currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
                }
            }
            else
            {
                // Handle air movement logic
                if (_moveInputVector.sqrMagnitude > 0f)
                {
                    targetMovementVelocity = _moveInputVector * MaxAirMoveSpeed;

                    // Prevent climbing on unstable slopes with air movement
                    if (Motor.GroundingStatus.FoundAnyGround)
                    {
                        Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                        targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
                    }

                    Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
                    currentVelocity += velocityDiff * (AirAccelerationSpeed * deltaTime);
                }

                // Apply gravity and drag
                currentVelocity += Gravity * deltaTime;
                currentVelocity *= (1f / (1f + (Drag * deltaTime)));

                // Reset jump request if airborne to prevent mid-air activation
                if (jumpComponent != null) jumpComponent.JumpRequested = false;
            }

            // Handle external impulses (PunchVelocity)
            if (PunchVelocity.magnitude > 0.1f)
            {
                Motor.ForceUnground();
                currentVelocity += (PunchVelocity) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                currentVelocity += (_moveInputVector * AirScalableForwardSpeed);
                GameManager.Instance.ResetPlayerPunchVelocityServerRpc(networkObject.OwnerClientId);
            }

            GameManager.Instance.UpdatePlayerVelocityServerRpc(networkObject.OwnerClientId, Motor.Velocity);
        }

        // --- split control ---
        private void OnNetworkChanged(ulong id) => RefreshPermissions();

        private void RefreshPermissions()
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening) return;

            var clientIds = NetworkManager.Singleton.ConnectedClientsIds;

            int myIndex = -1;
            for (int i = 0; i < clientIds.Count; i++)
            {
                if (clientIds[i] == NetworkManager.Singleton.LocalClientId)
                {
                    myIndex = i;
                    break;
                }
            }

            bool isSplit = SplitControlAnomaly.IsSplitActive;
            _currentPermissions = ControlMapper.GetPermissions(myIndex, clientIds.Count, isSplit);

            if (networkObject.IsOwner)
            {
                if (isSplit && !IsServer)
                {
                    if (MeshRoot) MeshRoot.gameObject.SetActive(false);
                    Motor.SetCapsuleCollisionsActivation(false);
                }
                else
                {
                    if (MeshRoot) MeshRoot.gameObject.SetActive(true);
                    Motor.SetCapsuleCollisionsActivation(true);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendCameraSyncServerRpc(float pitch, float yaw)
        {
            this.SharedCamPitch.Value = pitch;
            this.SharedCamYaw.Value = yaw;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendMovementInputServerRpc(Vector2 move, bool jump, ServerRpcParams rpcParams = default)
        {
            _sharedMoveInputs[rpcParams.Receive.SenderClientId] = move;
            if (jump) _sharedJumpInput = true;
        }

        private void ApplyCombinedInput(Vector2 rawMove, bool forceJump)
        {
            Vector3 moveInputVector = new Vector3(rawMove.x, 0f, rawMove.y);
            moveInputVector = Vector3.ClampMagnitude(moveInputVector, 1f);

            if (cameraTransform)
            {
                Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(cameraTransform.forward, Motor.CharacterUp).normalized;

                if (cameraPlanarDirection.sqrMagnitude == 0f)
                    cameraPlanarDirection = Vector3.ProjectOnPlane(cameraTransform.up, Motor.CharacterUp).normalized;

                Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);
                moveInputVector = cameraPlanarRotation * moveInputVector;
                _lookInputVector = cameraPlanarDirection;
            }

            _moveInputVector = moveInputVector;
            if (forceJump && jumpComponent != null)
                jumpComponent.JumpRequested = true;
        }

        //visual
        private void OnCharacterChanged(int oldId, int newId)
        {
            ApplyCharacter(newId);
        }

        private void ApplyCharacter(int id)
        {
            foreach (Transform child in MeshRoot)
                Destroy(child.gameObject);

            if (id < 0 || id >= characterPrefabs.Length) return;

            GameObject newCharacter = Instantiate(characterPrefabs[id], MeshRoot);
            if (IsOwner)
            {
                SetShadowsOnly(newCharacter);
            }

            var playerAnimator = GetComponent<PlayerAnimator>();
            if (playerAnimator != null)
            {
                playerAnimator.SetupNewCharacter(newCharacter);
            }
            UpdateVisuals();
        }

        private void SetShadowsOnly(GameObject target)
        {
            var renderers = target.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
        }
        private void UpdateVisuals()
        {
            bool hide = Anomalies.SplitControlAnomaly.IsSplitActive;
            if ( hide )
            {
                var renderers = MeshRoot.GetComponentsInChildren<Renderer>();
                foreach (var r in renderers)
                {
                    r.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                }
            }
        }
        //-------------------------
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

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            hitCollider.GetComponentInParent<IKccHitReceiver>()?.OnKccHit(Motor);
        }
    }
}