using System;
using System.Collections.Generic;
using System.Linq;
using Nac.Singleton;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controls
{
    public class InputManager : Service<InputManager>
    {
        [SerializeField] private InputActionAsset inputActionAsset;

        private readonly List<InputActionReference> active = new();
        private readonly CompositeDisposable subscriptions = new();
        private readonly ReactiveProperty<InputPreset> preset = new();

        public ReadOnlyReactiveProperty<InputPreset> Preset => preset;

        #region Unity

        public override void OnDestroy()
        {
            inputActionAsset.Disable();
            subscriptions.Dispose();

            base.OnDestroy();
        }

        #endregion

        public void ActivatePreset(InputPreset inputPreset)
        {
            foreach (var actionReference in active)
            {
                actionReference.action.Disable();
            }

            active.Clear();

            var toEnable = inputPreset.Maps
                .SelectMany(m => m.Actions)
                .Union(inputPreset.ToEnable)
                .Except(inputPreset.ToDisable);

            foreach (var actionReference in toEnable)
            {
                actionReference.action.Enable();

                active.Add(actionReference);
            }

            preset.Value = inputPreset;
        }

        #region Subscriptions

        public IDisposable Subscribe<T>(InputActionReference reference, InputPhaseFlags phaseFlags, Action<T> action, bool resetOnDisable = false)
            where T : struct
        {
            var inputStream = reference.AsObservable()
                .Where(ctx => MatchesPhase(ctx.phase, phaseFlags) || (resetOnDisable && ctx.phase == InputActionPhase.Canceled))
                .Select(ctx =>
                {
                    if (ctx.phase == InputActionPhase.Canceled && (resetOnDisable || (phaseFlags & InputPhaseFlags.Canceled) != 0))
                    {
                        return default(T);
                    }

                    return ctx.ReadValue<T>();
                });

            var finalStream = inputStream;

            if (!resetOnDisable)
            {
                return finalStream
                    .Subscribe(action)
                    .AddTo(subscriptions);
            }

            var disabledStream = Observable.Create<T, InputActionReference>(reference, static (observer, refObj) =>
            {
                if (refObj == null || refObj.action == null) return Disposable.Empty;

                var inputAction = refObj.action;

                Action<object, InputActionChange> handler = (obj, change) =>
                {
                    if (change == InputActionChange.ActionDisabled && obj == inputAction ||
                        change == InputActionChange.ActionMapDisabled && obj == inputAction.actionMap)
                    {
                        observer.OnNext(default);
                    }
                };

                InputSystem.onActionChange += handler;

                return Disposable.Create(() => InputSystem.onActionChange -= handler);
            });

            finalStream = Observable.Merge(inputStream, disabledStream);

            return finalStream
                .Subscribe(action)
                .AddTo(subscriptions);
        }

        public IDisposable Subscribe(InputActionReference reference, InputPhaseFlags phaseFlags, Action action)
        {
            var disposable = reference.AsObservable()
                .Where(ctx => MatchesPhase(ctx.phase, phaseFlags))
                .Subscribe(_ => action())
                .AddTo(subscriptions);

            return disposable;
        }

        public IDisposable Subscribe(InputActionReference reference, Action<InputAction.CallbackContext> action)
        {
            var disposable = reference.AsObservable()
                .Subscribe(action)
                .AddTo(subscriptions);

            return disposable;
        }

        public IDisposable Subscribe(InputActionReference reference, Action action)
        {
            var disposable = reference.AsObservable()
                .Subscribe(_ => action())
                .AddTo(subscriptions);

            return disposable;
        }

        #endregion

        #region Observables

        public Observable<T> Observe<T>(InputActionReference reference, InputPhaseFlags phase, bool resetOnDisable = false)
            where T : struct
        {
            var stream = reference.AsObservable()
                .Where(ctx => MatchesPhase(ctx.phase, phase) || (resetOnDisable && ctx.phase == InputActionPhase.Canceled))
                .Select(ctx =>
                {
                    if (ctx.phase == InputActionPhase.Canceled && (resetOnDisable || (phase & InputPhaseFlags.Canceled) != 0))
                    {
                        return default;
                    }

                    return ctx.ReadValue<T>();
                });

            var finalStream = stream;

            if (!resetOnDisable)
            {
                return TrackSubscription(finalStream);
            }

            var disabledStream = Observable.Create<T, InputActionReference>(reference, static (observer, refObj) =>
            {
                if (refObj == null || refObj.action == null) return Disposable.Empty;

                var inputAction = refObj.action;

                Action<object, InputActionChange> handler = (obj, change) =>
                {
                    if (change == InputActionChange.ActionDisabled && obj == inputAction ||
                        change == InputActionChange.ActionMapDisabled && obj == inputAction.actionMap)
                    {
                        observer.OnNext(default);
                    }
                };

                InputSystem.onActionChange += handler;

                return Disposable.Create(() => InputSystem.onActionChange -= handler);
            });

            finalStream = Observable.Merge(stream, disabledStream);

            return TrackSubscription(finalStream);
        }

        public Observable<InputAction.CallbackContext> Observe(InputActionReference reference, InputPhaseFlags phase)
        {
            var stream = reference.AsObservable()
                .Where(ctx => MatchesPhase(ctx.phase, phase));

            return TrackSubscription(stream);
        }

        public Observable<InputAction.CallbackContext> Observe(InputActionReference reference)
        {
            return TrackSubscription(reference.AsObservable());
        }

        #endregion

        #region Utils

        private Observable<TSource> TrackSubscription<TSource>(Observable<TSource> source)
        {
            return Observable.Create<TSource, (Observable<TSource> src, CompositeDisposable subs)>(
                (source, subscriptions),
                static (observer, state) => state.src.Subscribe(observer).AddTo(state.subs)
            );
        }

        private static bool MatchesPhase(InputActionPhase unityPhase, InputPhaseFlags phaseFlagsFlags)
        {
            return unityPhase switch
            {
                InputActionPhase.Started => (phaseFlagsFlags & InputPhaseFlags.Started) != 0,
                InputActionPhase.Performed => (phaseFlagsFlags & InputPhaseFlags.Performed) != 0,
                InputActionPhase.Canceled => (phaseFlagsFlags & InputPhaseFlags.Canceled) != 0,
                _ => false
            };
        }

        #endregion
    }
}