using Managers;
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

        private static InputManager Input
            => InputManager.Singleton;

        private void OnEnable()
        {
            Input.OnMouseMove += OnMouseMove;
            
            UnityEngine.Camera.main?.transform.SetParent(head, false);

            if (lockCursor)
                Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDisable()
        {
            Input.OnMouseMove -= OnMouseMove;

            UnityEngine.Camera.main?.transform.SetParent(null);

            if (lockCursor)
                Cursor.lockState = CursorLockMode.None;
        }

        private void LateUpdate()
        {
            // Vertical rotation
            verticalRotation -= mouseAxis.y;
            verticalRotation = Mathf.Clamp(verticalRotation, -rotationLimit, rotationLimit);
            head.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

            // Horizontal rotation
            body.Rotate(Vector3.up * mouseAxis.x);
        }

        private void OnMouseMove(Vector2 value)
        {
            mouseAxis = value;
        }
    }
}