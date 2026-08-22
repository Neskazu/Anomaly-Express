using System;
using Controls;
using Nac.Extensions;
using Nac.Network;
using R3;
using R3.Triggers;
using Tween.Base;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class UiEsc : MonoBehaviour
    {
        private static readonly TimeSpan SafeThrottle = TimeSpan.FromMilliseconds(300);

        [SerializeField] private GameObject cursor;
        [SerializeField] private Button disconnectButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private UISoundPlayer uiSoundPlayer;
        [Space]
        [SerializeField] private InputActionReference escAction;
        [SerializeField] private InputPreset uiPreset;
        [Space]
        [SerializeField] private MonoTweenSequence showHideSequence;

        private InputPreset savedPreset;
        private bool isOpen;

        private void Awake()
        {
            disconnectButton
                .OnPointerEnterAsObservable()
                .Subscribe(uiSoundPlayer.PlayHover)
                .AddTo(this);

            disconnectButton
                .OnClickAsObservable()
                .Subscribe(OnDisconnectCallback)
                .AddTo(this);

            quitButton
                .OnClickAsObservable()
                .Subscribe(OnQuitCallback)
                .AddTo(this);

            quitButton
                .OnPointerEnterAsObservable()
                .Subscribe(uiSoundPlayer.PlayHover)
                .AddTo(this);
        }

        private void Start()
        {
            InputManager.Instance
                .Observe(escAction, InputPhaseFlags.Performed)
                .ThrottleFirst(SafeThrottle)
                .Subscribe(ShowHideCallback)
                .AddTo(this);
        }

        private void OnDisconnectCallback()
        {
            uiSoundPlayer.PlayClick();
            ShowHideCallback();

            NetworkController.Instance.Disconnect();
        }

        private void OnQuitCallback()
        {
            uiSoundPlayer.PlayClick();
            ShowHideCallback();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ShowHideCallback()
        {
            showHideSequence.Play(isOpen);

            isOpen = !isOpen;
            cursor.SetActive(isOpen);

            if (isOpen)
            {
                savedPreset = InputManager.Instance.Preset.CurrentValue;

                Cursor.lockState = CursorLockMode.None;
                InputManager.Instance.ActivatePreset(uiPreset);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                InputManager.Instance.ActivatePreset(savedPreset);
            }
        }
    }
}