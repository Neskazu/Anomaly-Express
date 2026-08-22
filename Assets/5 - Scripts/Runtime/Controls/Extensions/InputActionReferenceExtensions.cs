using R3;
using UnityEngine.InputSystem;

namespace Controls
{
    public static class InputActionReferenceExtensions
    {
        /// <summary>
        /// Creates an observable stream for all callback phases of an InputActionReference.
        /// </summary>
        public static Observable<InputAction.CallbackContext> AsObservable(this InputActionReference reference)
        {
            return Observable.Create<InputAction.CallbackContext, InputActionReference>(reference, static (observer, refObj) =>
            {
                if (refObj == null || refObj.action == null)
                {
                    observer.OnCompleted();
                    return Disposable.Empty;
                }

                var action = refObj.action;

                action.started += OnEvent;
                action.performed += OnEvent;
                action.canceled += OnEvent;

                return Disposable.Create(() =>
                {
                    action.started -= OnEvent;
                    action.performed -= OnEvent;
                    action.canceled -= OnEvent;
                });

                void OnEvent(InputAction.CallbackContext context) => observer.OnNext(context);
            });
        }

        /// <summary>
        /// Creates an observable stream filtered to a specific input phase.
        /// </summary>
        public static Observable<InputAction.CallbackContext> AsObservable(this InputActionReference reference, InputActionPhase phase)
        {
            return reference.AsObservable().Where(ctx => ctx.phase == phase);
        }
    }
}