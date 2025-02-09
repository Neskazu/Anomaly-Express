using System;
using UnityEngine;

namespace Managers
{
    [DisallowMultipleComponent]
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private Vector2 mouseSensitivity = Vector2.one * 100f;

        public static InputManager Singleton;

        public event Action<Vector2> OnMouseMove;
        public event Action<Vector2> OnMoveAxisChanged;

        private InputSystem_Actions inputActions;

        private void Awake()
        {
            if (Singleton)
                throw new InvalidOperationException("Only one instance of InputManager is allowed.");

            inputActions = new InputSystem_Actions();

            inputActions.Player.Move.performed
                += context => OnMoveAxisChanged?
                    .Invoke(context.ReadValue<Vector2>());

            inputActions.Player.Look.performed
                += context => OnMouseMove?
                    .Invoke(Vector2.Scale(context.ReadValue<Vector2>(), mouseSensitivity));

            Singleton = this;
        }

        private void OnEnable()
        {
            inputActions?.Enable();
        }

        private void OnDisable()
        {
            inputActions?.Disable();
        }

        private void OnDestroy()
        {
            inputActions?.Dispose();
        }
    }
}