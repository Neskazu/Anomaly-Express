using R3;
using SaveSystem;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Nac
{
    public class GraphicSettignsObserver : MonoBehaviour
    {
        private static GraphicsSettingsSave Graphics => SaveManager.Save.GraphicsSettings;

        [SerializeField] private Camera cam;
        [SerializeField] private Volume volume;

        private UniversalAdditionalCameraData settigns;
        private ColorAdjustments colorAdjustments;

        private void Awake()
        {
            settigns = cam.GetComponent<UniversalAdditionalCameraData>();

            if (volume)
            {
                volume.profile.TryGet(out colorAdjustments);
            }

            Graphics.Msaa
                .Subscribe(ApplyMsaa)
                .AddTo(this);

            Graphics.Antialiasing
                .Subscribe(ApplyAA)
                .AddTo(this);

            Graphics.Brightness
                .Subscribe(ApplyBrightness)
                .AddTo(this);
        }

        private void ApplyMsaa(MsaaQuality quality)
        {
            if (cam && quality == MsaaQuality.Disabled)
            {
                cam.allowMSAA = false;
                return;
            }

            cam.allowMSAA = true;
            QualitySettings.antiAliasing = (int)quality;
            settigns.antialiasing = AntialiasingMode.None;
        }

        private void ApplyAA(AntialiasingMode mode)
        {
            if (!settigns)
            {
                return;
            }

            settigns.antialiasing = mode;
        }

        private void ApplyBrightness(float val)
        {
            if (!colorAdjustments)
            {
                return;
            }

            colorAdjustments.postExposure.value = val;
        }

        private void OnValidate()
        {
            if (cam == null)
                cam = GetComponent<Camera>();

            if (volume == null)
                volume = GetComponent<Volume>();
        }
    }
}