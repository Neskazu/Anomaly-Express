using System.Collections;
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

        private Camera cam;
        private Volume volume;
        private UniversalAdditionalCameraData settigns;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            volume = GetComponent<Volume>();

            if (cam)
            {
                settigns = cam.GetComponent<UniversalAdditionalCameraData>();
                if (settigns) settigns.renderPostProcessing = true;
            }

            // Подписки оставляем, они будут работать штатно при смене настроек в меню
            Graphics.Msaa.Subscribe(ApplyMsaa).AddTo(this);
            Graphics.Antialiasing.Subscribe(ApplyAA).AddTo(this);
            Graphics.Brightness.Subscribe(ApplyBrightness).AddTo(this);
        }

        private IEnumerator Start()
        {
            // Ждем 0.1 секунды, пока URP соберет Volume Stack, а Netcode спавнит объекты.
            // Это гарантирует, что мы накатим настройки поверх дефолтных значений сцены.
            yield return new WaitForSeconds(0.1f);

            ApplyBrightness(Graphics.Brightness.CurrentValue);
            ApplyMsaa(Graphics.Msaa.CurrentValue);
            ApplyAA(Graphics.Antialiasing.CurrentValue);
        }

        private void ApplyMsaa(MsaaQuality quality)
        {
            if (cam && quality == MsaaQuality.Disabled)
            {
                cam.allowMSAA = false;
                return;
            }

            if (cam) cam.allowMSAA = true;
            QualitySettings.antiAliasing = (int)quality;
            if (settigns) settigns.antialiasing = AntialiasingMode.None;
        }

        private void ApplyAA(AntialiasingMode mode)
        {
            if (settigns) settigns.antialiasing = mode;
        }

        private void ApplyBrightness(float val)
        {
            if (!volume || !volume.profile) return;

            if (volume.profile.TryGet(out ColorAdjustments ca))
            {
                ca.active = true;
                ca.postExposure.Override(val);
            }
        }
    }
}