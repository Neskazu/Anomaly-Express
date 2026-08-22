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

        [Header("Settings")]
        [SerializeField] private float cooldownTime = 1.0f;

        private readonly CompositeDisposable disposable = new();

        private HandsController handsController;
        private LensComponent phoneLens;
        private float cooldown;
        private bool active;

        public void Setup(HandsController controller)
        {
            handsController = controller;
            phoneLens = FantomBlocksController.Singleton.LocalPlayerLens;

            disposable.Clear();

            InputManager.Instance
                .Subscribe(showAction, InputPhaseFlags.Started, EnablePhone)
                .AddTo(disposable);

            InputManager.Instance
                .Subscribe(hideAction, InputPhaseFlags.Started, DisablePhone)
                .AddTo(disposable);

            InputManager.Instance
                .Subscribe(goAction, InputPhaseFlags.Started, EnablePhotoMode)
                .AddTo(disposable);

            InputManager.Instance
                .Subscribe(backAction, InputPhaseFlags.Started, DisablePhotoMode)
                .AddTo(disposable);
        }

        private void OnDestroy()
        {
            disposable.Dispose();
        }

        private void FixedUpdate()
        {
            if (cooldown >= Mathf.Epsilon)
            {
                cooldown = Mathf.Max(cooldown - Time.fixedDeltaTime, 0f);
            }
        }

        private void EnablePhone()
        {
            if (cooldown >= Mathf.Epsilon || active)
            {
                return;
            }

            cooldown = cooldownTime;
            active = true;
            handsController.UpdatePhoneState(true);

            InputManager.Instance.ActivatePreset(phoneInputPreset);
        }

        private void DisablePhone()
        {
            if (cooldown >= Mathf.Epsilon || !active)
            {
                return;
            }

            DisablePhotoMode();

            cooldown = cooldownTime;
            active = false;
            handsController.UpdatePhoneState(false);

            InputManager.Instance.ActivatePreset(defaultInputPreset);
        }

        private void EnablePhotoMode()
        {
            if (cooldown >= Mathf.Epsilon || !active || phoneLens.enabled)
            {
                return;
            }

            cooldown = cooldownTime;
            phoneLens.enabled = true;
            handsController.UpdatePhotoModeState(true);

            InputManager.Instance.ActivatePreset(photoModeInputPreset);
        }

        private void DisablePhotoMode()
        {
            if (cooldown >= Mathf.Epsilon || !active || !phoneLens.enabled)
            {
                return;
            }

            cooldown = cooldownTime;
            phoneLens.enabled = false;
            handsController.UpdatePhotoModeState(false);

            InputManager.Instance.ActivatePreset(phoneInputPreset);
        }
    }
}