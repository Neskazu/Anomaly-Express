using Managers;
using System;
using UnityEngine;

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

        private static InputManager Input
            => InputManager.Singleton;

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
            if(head != null)
            Camera.main.transform.position = head.position;
        }

        private void HandleRotation()
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