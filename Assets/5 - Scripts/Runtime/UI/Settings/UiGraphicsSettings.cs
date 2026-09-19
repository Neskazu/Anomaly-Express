using System;
using System.Linq;
using Nac;
using Nac.Extensions;
using R3;
using SaveSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Settings
{
    public class UiGraphicsSettings : MonoBehaviour
    {
        private static GraphicsSettingsSave Graphics => SaveManager.Save.GraphicsSettings;

        [SerializeField] private TMP_Dropdown resolution;
        [SerializeField] private Toggle fullScreen;
        [SerializeField] private Toggle vsync;
        [SerializeField] private Slider brightness;
        [SerializeField] private Slider shadow;
        [SerializeField] private TMP_Dropdown antiAliasing;

        private void Awake()
        {
            resolution.options = Graphics
                .GetAllResolutions()
                .Select(r => new TMP_Dropdown.OptionData($"{r.width}x{r.height}"))
                .ToList();

            antiAliasing.options = Graphics
                .GetAllAntiAliasing()
                .Select(aa => new TMP_Dropdown.OptionData($"{aa}"))
                .ToList();
        }

        private void Start()
        {
            Sync();

            resolution.OnValueChangedAsObservable()
                .Subscribe(ApplyResolution)
                .AddTo(this);

            fullScreen.OnValueChangedAsObservable()
                .Subscribe(ApplyFullScreen)
                .AddTo(this);

            vsync.OnValueChangedAsObservable()
                .Subscribe(ApplyVSync)
                .AddTo(this);

            brightness.OnValueChangedAsObservable()
                .Subscribe(ApplyBrightness)
                .AddTo(this);

            shadow.OnValueChangedAsObservable()
                .Subscribe(ApplyShadow)
                .AddTo(this);

            antiAliasing.OnValueChangedAsObservable()
                .Subscribe(ApplyAntiAliasing)
                .AddTo(this);

            Graphics.OnSettingsChanged
                .Delay(TimeSpan.FromSeconds(1))
                .Subscribe(Sync)
                .AddTo(this);
        }

        private void Sync()
        {
            antiAliasing.SetValueWithoutNotify(Graphics.GetAntiAliasingIndex());
            resolution.SetValueWithoutNotify(Graphics.GetResolutionIndex());

            fullScreen.SetIsOnWithoutNotify(Graphics.GetFullScreen());
            vsync.SetIsOnWithoutNotify(Graphics.GetVsync());

            brightness.SetValueWithoutNotify(Graphics.GetBrightness());
            shadow.SetValueWithoutNotify(Graphics.GetShadow());

            resolution.RefreshShownValue();
            antiAliasing.RefreshShownValue();

            SaveManager.SaveGame();
        }

        private void ApplyResolution(int index)
        {
            Graphics.SetResolution(index);
        }

        private void ApplyFullScreen(bool isOn)
        {
            Graphics.SetFullScreen(isOn);
        }

        private void ApplyVSync(bool isOn)
        {
            Graphics.SetVsync(isOn);
        }

        private void ApplyBrightness(float val)
        {
            Graphics.SetBrightness(val);
        }

        private void ApplyShadow(float val)
        {
            Graphics.SetShadow(val);
        }

        private void ApplyAntiAliasing(int index)
        {
            Graphics.SetAntialiasing(index);
        }
    }
}