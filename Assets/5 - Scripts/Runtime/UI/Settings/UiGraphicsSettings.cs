using System;
using System.Collections.Generic;
using System.Linq;
using Nac;
using R3;
using SaveSystem;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace UI.Settings
{
    public class UiGraphicsSettings : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Dropdown resolution;
        [SerializeField] private Toggle fullScreen;
        [SerializeField] private Toggle vsync;
        [SerializeField] private Slider brightness;
        [SerializeField] private Slider shadow;
        [SerializeField] private TMP_Dropdown antiAliasing;

        private UniversalRenderPipelineAsset target;
        private List<Resolution> resolutions;

        private GraphicsSettingsSave Graphics => SaveManager.Save.GraphicsSettings;

        private void Awake()
        {
            target = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;

            resolutions = Screen.resolutions
                .DistinctBy(r => new { r.width, r.height })
                .ToList();

            resolution.options = resolutions
                .Select(r => new TMP_Dropdown.OptionData($"{r.width}x{r.height}"))
                .ToList();

            antiAliasing.options = new List<TMP_Dropdown.OptionData>()
            {
                new("None"),
                new("MSAAx2"),
                new("MSAAx4"),
                new("MSAAx8"),
                new("FXAA"),
                new("SMAA"),
                new("TAA"),
            };
        }

        private void Start()
        {
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
        }

        private void OnEnable()
        {
            var currentResIndex = resolutions.FindIndex(r =>
                r.width == Screen.width &&
                r.height == Screen.height);

            resolution.value = currentResIndex != -1 ? currentResIndex : resolutions.Count - 1;
            resolution.RefreshShownValue();

            brightness.value = Graphics.Brightness.CurrentValue;

            switch (Graphics.Antialiasing.CurrentValue)
            {
                case AntialiasingMode.FastApproximateAntialiasing:
                    antiAliasing.value = 4;
                    break;
                case AntialiasingMode.SubpixelMorphologicalAntiAliasing:
                    antiAliasing.value = 5;
                    break;
                case AntialiasingMode.TemporalAntiAliasing:
                    antiAliasing.value = 6;
                    break;
                case AntialiasingMode.None:
                default:
                    switch (Graphics.Msaa.CurrentValue)
                    {
                        case MsaaQuality._2x:
                            antiAliasing.value = 1;
                            break;
                        case MsaaQuality._4x:
                            antiAliasing.value = 2;
                            break;
                        case MsaaQuality._8x:
                            antiAliasing.value = 3;
                            break;
                        case MsaaQuality.Disabled:
                        default:
                            antiAliasing.value = 0;
                            break;
                    }

                    break;
            }

            antiAliasing.RefreshShownValue();
            fullScreen.isOn = Screen.fullScreen;
        }

        private void ApplyResolution(int index)
        {
            if (index >= 0 && index < resolutions.Count)
            {
                var res = resolutions[index];
                Screen.SetResolution(res.width, res.height, Screen.fullScreen);
            }
        }

        private void ApplyFullScreen(bool isOn)
        {
            Screen.fullScreen = isOn;
        }

        private void ApplyVSync(bool isOn)
        {
            QualitySettings.vSyncCount = isOn ? 1 : 0;
            PlayerPrefs.SetInt("VSync", isOn ? 1 : 0);
        }

        private void ApplyBrightness(float val)
        {
            Graphics.SetBrightness(val);
            SaveManager.SaveGame();
        }

        private void ApplyShadow(float val)
        {
            if (target != null)
            {
                target.shadowDistance = val;
            }

            PlayerPrefs.SetFloat("Shadow", val);
        }

        private void ApplyAntiAliasing(int index)
        {
            switch (index)
            {
                default:
                    Graphics.SetAntialiasing(AntialiasingMode.None);
                    Graphics.SetMsaaQuality(MsaaQuality.Disabled);
                    break;
                case 1:
                    Graphics.SetAntialiasing(AntialiasingMode.None);
                    Graphics.SetMsaaQuality(MsaaQuality._2x);
                    break;
                case 2:
                    Graphics.SetAntialiasing(AntialiasingMode.None);
                    Graphics.SetMsaaQuality(MsaaQuality._4x);
                    break;
                case 3:
                    Graphics.SetAntialiasing(AntialiasingMode.None);
                    Graphics.SetMsaaQuality(MsaaQuality._8x);
                    break;
                case 4:
                    Graphics.SetAntialiasing(AntialiasingMode.FastApproximateAntialiasing);
                    Graphics.SetMsaaQuality(MsaaQuality.Disabled);
                    break;
                case 5:
                    Graphics.SetAntialiasing(AntialiasingMode.SubpixelMorphologicalAntiAliasing);
                    Graphics.SetMsaaQuality(MsaaQuality.Disabled);
                    break;
                case 6:
                    Graphics.SetAntialiasing(AntialiasingMode.TemporalAntiAliasing);
                    Graphics.SetMsaaQuality(MsaaQuality.Disabled);
                    break;
            }

            SaveManager.SaveGame();
        }
    }
}