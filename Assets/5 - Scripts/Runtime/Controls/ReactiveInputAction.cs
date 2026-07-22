using System;
using R3;
using UnityEngine.InputSystem;

namespace Controls
{
    public interface IReactiveInput : IDisposable
    {
        void Enable();
        void Disable();
    }

    public class ReactiveInput : Observable<InputAction.CallbackContext>, IReactiveInput
    {
        private readonly InputAction action;
        private readonly ReactiveInputPhase phase;

        private readonly Subject<InputAction.CallbackContext> subject = new();

        public ReactiveInput(InputAction action, ReactiveInputPhase phase)
        {
            this.action = action;
            this.phase = phase;

            if (phase.HasFlag(ReactiveInputPhase.Started))
            {
                this.action.started += ReadContext;
            }

            if (phase.HasFlag(ReactiveInputPhase.Performed))
            {
                this.action.performed += ReadContext;
            }

            if (phase.HasFlag(ReactiveInputPhase.Canceled))
            {
                this.action.canceled += ReadContext;
            }
        }

        public void Dispose()
        {
            if (phase.HasFlag(ReactiveInputPhase.Started))
            {
                action.started -= ReadContext;
            }

            if (phase.HasFlag(ReactiveInputPhase.Performed))
            {
                action.performed -= ReadContext;
            }

            if (phase.HasFlag(ReactiveInputPhase.Canceled))
            {
                action.canceled -= ReadContext;
            }

            subject.Dispose();
        }

        private void ReadContext(InputAction.CallbackContext ctx)
        {
            subject.OnNext(ctx);
        }

        public void Enable()
        {
            action.Enable();
        }

        public void Disable()
        {
            action.Disable();
        }

        protected override IDisposable SubscribeCore(Observer<InputAction.CallbackContext> observer)
        {
            return subject.Subscribe(observer.OnNext);
        }
    }

    public class ReactiveInput<T> : Observable<T>, IReactiveInput
        where T : struct
    {
        private readonly InputAction action;
        private readonly ReactiveInputPhase phase;
        private readonly bool resetOnDisable;

        private readonly Subject<T> subject = new();

        public ReactiveInput(InputAction action, ReactiveInputPhase phase, bool resetOnDisable = false)
        {
            this.action = action;
            this.phase = phase;
            this.resetOnDisable = resetOnDisable;

            if (phase.HasFlag(ReactiveInputPhase.Started))
            {
                this.action.started += ReadContext;
            }

            if (phase.HasFlag(ReactiveInputPhase.Performed))
            {
                this.action.performed += ReadContext;
            }

            if (phase.HasFlag(ReactiveInputPhase.Canceled))
            {
                this.action.canceled += ReadContext;
            }
        }

        public void Dispose()
        {
            if (phase.HasFlag(ReactiveInputPhase.Started))
            {
                action.started -= ReadContext;
            }

            if (phase.HasFlag(ReactiveInputPhase.Performed))
            {
                action.performed -= ReadContext;
            }

            if (phase.HasFlag(ReactiveInputPhase.Canceled))
            {
                action.canceled -= ReadContext;
            }

            subject.Dispose();
        }

        private void ReadContext(InputAction.CallbackContext ctx)
        {
            subject.OnNext(ctx.ReadValue<T>());
        }

        public void Enable()
        {
            action.Enable();
        }

        public void Disable()
        {
            if (resetOnDisable)
            {
                subject.OnNext(default);
            }

            action.Disable();
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            return subject.Subscribe(observer.OnNext);
        }
    }
}