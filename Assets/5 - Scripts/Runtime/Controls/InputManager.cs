using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controls
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Singleton;

        [SerializeField] private InputActionAsset inputActionAsset;

        private readonly Dictionary<Guid, Subject<InputAction.CallbackContext>> subjects = new();
        private readonly List<InputActionReference> active = new();

        #region Unity

        private void Awake()
        {
            if (Singleton)
            {
                Debug.LogError("Only one instance of InputManager is allowed.");
                Destroy(gameObject);
            }

            foreach (var action in inputActionAsset)
            {
                var subject = new Subject<InputAction.CallbackContext>();

                action.started += subject.OnNext;
                action.performed += subject.OnNext;
                action.canceled += subject.OnNext;

                subjects.Add(action.id, subject);
            }

            DontDestroyOnLoad(gameObject);
            Singleton = this;
        }

        private void OnDestroy()
        {
            foreach (var action in inputActionAsset)
            {
                var subject = subjects[action.id];

                action.started -= subject.OnNext;
                action.performed -= subject.OnNext;
                action.canceled -= subject.OnNext;

                subject.Dispose();
            }
        }

        #endregion

        #region Subscriptions

        public IDisposable Subscribe<T>(InputActionReference reference, InputActionPhase phase, Action<T> action)
            where T : struct
        {
            if (subjects.TryGetValue(reference.action.id, out var subject))
            {
                return subject
                    .Where(ctx => ctx.phase == phase)
                    .Subscribe(ctx => action.Invoke(ctx.ReadValue<T>()));
            }

            Debug.LogError($"InputAction [{reference.action.name}] not found in [{inputActionAsset.name}].");
            return null;
        }

        public IDisposable Subscribe(InputActionReference reference, InputActionPhase phase, Action action)
        {
            if (subjects.TryGetValue(reference.action.id, out var subject))
            {
                return subject
                    .Where(ctx => ctx.phase == phase)
                    .Subscribe(_ => action.Invoke());
            }

            Debug.LogError($"InputAction [{reference.action.name}] not found in [{inputActionAsset.name}].");
            return null;
        }

        public IDisposable Subscribe(InputActionReference reference, Action<InputAction.CallbackContext> action)
        {
            if (subjects.TryGetValue(reference.action.id, out var subject))
            {
                return subject.Subscribe(action.Invoke);
            }

            Debug.LogError($"InputAction [{reference.action.name}] not found in [{inputActionAsset.name}].");
            return null;
        }

        public IDisposable Subscribe(InputActionReference reference, Action action)
        {
            if (subjects.TryGetValue(reference.action.id, out var subject))
            {
                return subject.Subscribe(_ => action.Invoke());
            }

            Debug.LogError($"InputAction [{reference.action.name}] not found in [{inputActionAsset.name}].");
            return null;
        }

        #endregion

        public void ActivatePreset(InputPreset inputPreset)
        {
            foreach (var reference in active)
            {
                reference.action.Disable();
            }

            active.Clear();

            var toEnable = inputPreset.Maps
                .SelectMany(m => m.Actions)
                .Union(inputPreset.ToEnable)
                .Except(inputPreset.ToDisable);

            foreach (var reference in toEnable)
            {
                reference.action.Enable();

                active.Add(reference);
            }
        }
    }
}