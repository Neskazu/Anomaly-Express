using Camera;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private InputSystem_Actions inputSystem;
        [SerializeField] private Transform cameraHandler;
        [SerializeField] private Rigidbody rb;
        public static PlayerController LocalInstance { get; private set; }
        private Vector3 moveDir;
        public event Action<bool> OnMovementStateChanged;
        private bool isMoving;
        private float moveEpsilon = 0.1f;

         
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                LocalInstance = this;
                SetLocalCamera();
                inputSystem = new InputSystem_Actions();
                inputSystem.Enable();
                inputSystem.Player.Move.performed += OnMove;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!IsOwner)
            {
                return;
            }
            CheckMovementState();
        }

        private void CheckMovementState()
        {
            bool newMovingState = moveDir.sqrMagnitude > 0.01f;
            if (newMovingState != isMoving)
            {
                isMoving = newMovingState;
                OnMovementStateChanged?.Invoke(isMoving);
            }
        }

        private void FixedUpdate()
        {
            if (!IsOwner)
            {
                return;
            }

            HandleMovement();
        }

        public override void OnDestroy()
        {
            if (!IsOwner || inputSystem == null)
            {
                return;
            }

            inputSystem.Disable();
            inputSystem.Player.Move.performed -= OnMove;
        }

        private void SetLocalCamera()
        {
            UnityEngine.Camera.main.transform.SetParent(cameraHandler, false);
            UnityEngine.Camera.main.GetComponent<CameraController>().player = transform;
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            Vector2 inputMovement = context.ReadValue<Vector2>();
            moveDir = new Vector3(inputMovement.x, 0, inputMovement.y);
        }

        private void HandleMovement()
        {
            Vector3 movePos = ((transform.right * moveDir.x + transform.forward * moveDir.z).normalized) *
                              Time.fixedDeltaTime;
            rb.linearVelocity = new Vector3(movePos.x * moveSpeed, rb.linearVelocity.y, movePos.z * moveSpeed);
        }

    }
}