using Controls;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Anomalies
{
    public class PhoneController : MonoBehaviour
    {
        [Header("Presets")]
        [SerializeField] private InputPreset defaultInputPreset;
        [SerializeField] private InputPreset phoneInputPreset;
        [SerializeField] private InputPreset photoModeInputPreset;

        [Header("Actions")]
        [SerializeField] private InputActionReference showAction;
        [SerializeField] private InputActionReference hideAction;
        [SerializeField] private InputActionReference goAction;
        [SerializeField] private InputActionReference backAction;

        private readonly CompositeDisposable disposable = new();

        private HandsController handsController;
        private LensComponent phoneLens;
        private bool active;

        public void Setup(HandsController controller)
        {
            handsController = controller;
            phoneLens = FantomBlocksController.Singleton.LocalPlayerLens;

            disposable.Clear();

            InputManager.Singleton
                .Subscribe(showAction, ReactiveInputPhase.Started, EnablePhone)
                .AddTo(disposable);

            InputManager.Singleton
                .Subscribe(hideAction, ReactiveInputPhase.Started, DisablePhone)
                .AddTo(disposable);

            InputManager.Singleton
                .Subscribe(goAction, ReactiveInputPhase.Started, EnablePhotoMode)
                .AddTo(disposable);

            InputManager.Singleton
                .Subscribe(backAction, ReactiveInputPhase.Started, DisablePhotoMode)
                .AddTo(disposable);
        }

        private void OnDestroy()
        {
            disposable.Dispose();
        }

        private void EnablePhone()
        {
            if (active)
            {
                return;
            }

            active = true;
            handsController.UpdatePhoneState(true);

            InputManager.Singleton.ActivatePreset(phoneInputPreset);
        }

        private void DisablePhone()
        {
            if (!active)
            {
                return;
            }

            DisablePhotoMode();

            active = false;
            handsController.UpdatePhoneState(false);

            InputManager.Singleton.ActivatePreset(defaultInputPreset);
        }

        private void EnablePhotoMode()
        {
            if (!active || phoneLens.enabled)
            {
                return;
            }

            phoneLens.enabled = true;
            handsController.UpdatePhotoModeState(true);

            InputManager.Singleton.ActivatePreset(photoModeInputPreset);
        }

        private void DisablePhotoMode()
        {
            if (!active || !phoneLens.enabled)
            {
                return;
            }

            phoneLens.enabled = false;
            handsController.UpdatePhotoModeState(false);

            InputManager.Singleton.ActivatePreset(phoneInputPreset);
        }
    }
}