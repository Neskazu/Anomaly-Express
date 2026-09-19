using System;
using R3;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Nac
{
    [Serializable]
    public class GraphicsSettingsSave : ISerializationCallbackReceiver
    {
        private static List<Resolution> resolutions = Screen.resolutions
            .GroupBy(res => new { res.width, res.height })
            .Select(group => group.Last())
            .ToList();

        private static List<string> antiAliasingNames = new()
        {
            "None",
            "MSAAx2",
            "MSAAx4",
            "MSAAx8",
            "FXAA",
            "SMAA",
            "TAA",
        };

        [SerializeField] private AntialiasingMode antialiasingMode;
        [SerializeField] private MsaaQuality msaaQuality;
        [SerializeField] private float brightness;

        [NonSerialized] private ReactiveProperty<AntialiasingMode> reactiveAntialiasing = new(AntialiasingMode.None);
        [NonSerialized] private ReactiveProperty<MsaaQuality> reactiveMsaaQuality = new(MsaaQuality.Disabled);
        [NonSerialized] private ReactiveProperty<float> reactiveBrightness = new(0.0f);

        [NonSerialized] private readonly Subject<Unit> onSettingsChanged = new();

        public R3.Observable<Unit> OnSettingsChanged => onSettingsChanged;

        public ReadOnlyReactiveProperty<AntialiasingMode> Antialiasing => reactiveAntialiasing;

        public ReadOnlyReactiveProperty<MsaaQuality> Msaa =>
            reactiveMsaaQuality
                .CombineLatest(reactiveAntialiasing, (msaa, aa) => aa == AntialiasingMode.None ? msaa : MsaaQuality.Disabled)
                .ToReadOnlyReactiveProperty();

        public ReadOnlyReactiveProperty<float> Brightness => reactiveBrightness;

        public int GetAntiAliasingIndex()
        {
            switch (Antialiasing.CurrentValue)
            {
                case AntialiasingMode.FastApproximateAntialiasing:
                    return 4;
                case AntialiasingMode.SubpixelMorphologicalAntiAliasing:
                    return 5;
                case AntialiasingMode.TemporalAntiAliasing:
                    return 6;
                case AntialiasingMode.None:
                default:
                    switch (Msaa.CurrentValue)
                    {
                        case MsaaQuality._2x:
                            return 1;
                        case MsaaQuality._4x:
                            return 2;
                        case MsaaQuality._8x:
                            return 3;
                        case MsaaQuality.Disabled:
                        default:
                            return 0;
                    }
            }
        }

        public int GetResolutionIndex()
        {
            var index = resolutions.FindIndex(r =>
                r.height == Screen.height &&
                r.width == Screen.width);

            if (index == -1)
            {
                var currentPixels = Screen.width * Screen.height;
                var closestDifference = int.MaxValue;
                var closestIndex = 0;

                for (var i = 0; i < resolutions.Count; i++)
                {
                    var resPixels = resolutions[i].width * resolutions[i].height;
                    var difference = Mathf.Abs(resPixels - currentPixels);

                    if (difference < closestDifference)
                    {
                        closestDifference = difference;
                        closestIndex = i;
                    }
                }

                return closestIndex;
            }

            return index;
        }

        public bool GetFullScreen()
        {
            return Screen.fullScreenMode == FullScreenMode.FullScreenWindow;
        }

        public bool GetVsync()
        {
            return QualitySettings.vSyncCount == 1;
        }

        public float GetShadow()
        {
            return PlayerPrefs.GetFloat("Shadow", 0);
        }

        public float GetBrightness()
        {
            return Brightness.CurrentValue;
        }

        public void SetResolution(int index)
        {
            if (index >= resolutions.Count)
            {
                return;
            }

            var resolution = resolutions[index];

            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);

            onSettingsChanged.OnNext(Unit.Default);
        }

        public void SetFullScreen(bool isFullScreen)
        {
            Screen.SetResolution(Screen.width, Screen.height, isFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            Screen.fullScreen = isFullScreen;

            onSettingsChanged.OnNext(Unit.Default);
        }

        public void SetAntialiasing(int index)
        {
            switch (index)
            {
                default:
                    reactiveAntialiasing.Value = AntialiasingMode.None;
                    reactiveMsaaQuality.Value = MsaaQuality.Disabled;
                    break;
                case 1:
                    reactiveAntialiasing.Value = AntialiasingMode.None;
                    reactiveMsaaQuality.Value = MsaaQuality._2x;
                    break;
                case 2:
                    reactiveAntialiasing.Value = AntialiasingMode.None;
                    reactiveMsaaQuality.Value = MsaaQuality._4x;
                    break;
                case 3:
                    reactiveAntialiasing.Value = AntialiasingMode.None;
                    reactiveMsaaQuality.Value = MsaaQuality._8x;
                    break;
                case 4:
                    reactiveAntialiasing.Value = AntialiasingMode.FastApproximateAntialiasing;
                    reactiveMsaaQuality.Value = MsaaQuality.Disabled;
                    break;
                case 5:
                    reactiveAntialiasing.Value = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    reactiveMsaaQuality.Value = MsaaQuality.Disabled;
                    break;
                case 6:
                    reactiveAntialiasing.Value = AntialiasingMode.TemporalAntiAliasing;
                    reactiveMsaaQuality.Value = MsaaQuality.Disabled;
                    break;
            }

            onSettingsChanged.OnNext(Unit.Default);
        }

        public void SetBrightness(float val)
        {
            reactiveBrightness.Value = Mathf.Clamp(val, -1f, 1f);

            onSettingsChanged.OnNext(Unit.Default);
        }

        public void SetShadow(float value)
        {
            var target = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;

            if (target != null)
            {
                target.shadowDistance = value;
            }

            PlayerPrefs.SetFloat("Shadow", value);

            onSettingsChanged.OnNext(Unit.Default);
        }

        public void SetVsync(bool isOn)
        {
            QualitySettings.vSyncCount = isOn ? 1 : 0;
            PlayerPrefs.SetInt("VSync", isOn ? 1 : 0);

            onSettingsChanged.OnNext(Unit.Default);
        }

        public List<Resolution> GetAllResolutions()
        {
            return resolutions;
        }

        public List<string> GetAllAntiAliasing()
        {
            return antiAliasingNames;
        }

        #region ISerializationCallbackReceiver

        public void OnBeforeSerialize()
        {
            antialiasingMode = reactiveAntialiasing.CurrentValue;
            msaaQuality = reactiveMsaaQuality.CurrentValue;
            brightness = reactiveBrightness.CurrentValue;
        }

        public void OnAfterDeserialize()
        {
            reactiveAntialiasing.Value = antialiasingMode;
            reactiveMsaaQuality.Value = msaaQuality;
            reactiveBrightness.Value = brightness;
        }

        #endregion
    }
}