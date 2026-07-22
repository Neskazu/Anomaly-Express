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

        private readonly Dictionary<Guid, List<IReactiveInput>> reactiveInputs = new();
        private readonly HashSet<Guid> active = new();

        #region Unity

        private void Awake()
        {
            if (Singleton)
            {
                Debug.LogError("Only one instance of InputManager is allowed.");
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
            Singleton = this;
        }

        #endregion

        #region Subscriptions

        public IDisposable Subscribe<T>(InputActionReference reference, ReactiveInputPhase phase, Action<T> action, bool resetOnDisable = false)
            where T : struct
        {
            var inputAction = inputActionAsset.FindAction(reference.action.id);
            var input = new ReactiveInput<T>(inputAction, phase, resetOnDisable);

            if (!reactiveInputs.TryGetValue(reference.action.id, out var reactiveInput))
            {
                reactiveInput = new List<IReactiveInput>();
                reactiveInputs.Add(reference.action.id, reactiveInput);
            }

            reactiveInput.Add(input);

            if (active.Contains(inputAction.id))
            {
                input.Enable();
            }

            return input.Subscribe(action.Invoke);
        }

        public IDisposable Subscribe(InputActionReference reference, ReactiveInputPhase phase, Action action)
        {
            var inputAction = inputActionAsset.FindAction(reference.action.id);
            var input = new ReactiveInput(reference, phase);

            if (!reactiveInputs.TryGetValue(reference.action.id, out var reactiveInput))
            {
                reactiveInput = new List<IReactiveInput>();
                reactiveInputs.Add(reference.action.id, reactiveInput);
            }

            reactiveInput.Add(input);

            if (active.Contains(inputAction.id))
            {
                input.Enable();
            }

            return input.Subscribe(_ => action.Invoke());
        }

        public IDisposable Subscribe(InputActionReference reference, Action<InputAction.CallbackContext> action)
        {
            var inputAction = inputActionAsset.FindAction(reference.action.id);
            var input = new ReactiveInput(reference, ReactiveInputPhase.All);

            if (!reactiveInputs.TryGetValue(reference.action.id, out var reactiveInput))
            {
                reactiveInput = new List<IReactiveInput>();
                reactiveInputs.Add(reference.action.id, reactiveInput);
            }

            reactiveInput.Add(input);

            if (active.Contains(inputAction.id))
            {
                input.Enable();
            }

            return input.Subscribe(action.Invoke);
        }

        public IDisposable Subscribe(InputActionReference reference, Action action)
        {
            var inputAction = inputActionAsset.FindAction(reference.action.id);
            var input = new ReactiveInput(reference, ReactiveInputPhase.All);

            if (!reactiveInputs.TryGetValue(reference.action.id, out var reactiveInput))
            {
                reactiveInput = new List<IReactiveInput>();
                reactiveInputs.Add(reference.action.id, reactiveInput);
            }

            if (active.Contains(inputAction.id))
            {
                input.Enable();
            }

            reactiveInput.Add(input);
            return input.Subscribe(_ => action.Invoke());
        }

        #endregion

        public void ActivatePreset(InputPreset inputPreset)
        {
            foreach (var id in active)
            {
                if (!reactiveInputs.TryGetValue(id, out var inputs)) continue;

                foreach (var input in inputs)
                {
                    input.Disable();
                }
            }

            active.Clear();

            var toEnable = inputPreset.Maps
                .SelectMany(m => m.Actions)
                .Select(iar => iar.action.id)
                .Union(inputPreset.ToEnableIds)
                .Except(inputPreset.ToDisableIds);

            foreach (var id in toEnable)
            {
                active.Add(id);

                if (!reactiveInputs.TryGetValue(id, out var inputs)) continue;

                foreach (var input in inputs)
                {
                    input.Enable();
                }
            }
        }
    }
}