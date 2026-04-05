using Managers;
using Unity.Netcode.Components;
using UnityEngine;

namespace Player.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(NetworkRigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        // Dependencies
        [SerializeField] private Rigidbody rb;

        // Settings
        [SerializeField] private float speed = 10f;

        private Vector2 axis;

        private static InputManager Input
            => InputManager.Singleton;

        private void OnEnable()
        {
            Input.OnMoveAxisChanged += ChangeAxis;
        }

        private void OnDisable()
        {
            Input.OnMoveAxisChanged -= ChangeAxis;
        }

        private void ChangeAxis(Vector2 newAxis)
        {
            axis = newAxis;
        }

        private void FixedUpdate()
        {
            var velocity = (transform.right * axis.x + transform.forward * axis.y) * speed;
            velocity.y = rb.linearVelocity.y;

            rb.linearVelocity = velocity;
        }
    }
}