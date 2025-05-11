using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace UI.Settings
{
    public class UiGraphicsSettings : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UniversalRenderPipelineAsset target;

        [Header("Controls")]
        [SerializeField] private Dropdown resolution;
        [SerializeField] private Toggle fullScreen;
        [SerializeField] private Toggle vsync;
        [SerializeField] private Slider brightness;
        [SerializeField] private Slider shadow;
        [SerializeField] private Slider antiAliasing;

        private void Awake()
        {
            //fullScreen.onValueChanged.AddListener(value => Screen.fullScreen = value);
            vsync.onValueChanged.AddListener(value => QualitySettings.vSyncCount = value ? 1 : 0);
            shadow.onValueChanged.AddListener(ShadowmapResolutionChanged);
            antiAliasing.onValueChanged.AddListener(MSAASampleCountChanged);
        }

        private void ShadowmapResolutionChanged(float value)
        {
            var selection = Mathf.RoundToInt(value);

            target.mainLightShadowmapResolution = selection switch
            {
                0 => target.additionalLightsShadowResolutionTierLow,
                1 => target.additionalLightsShadowResolutionTierMedium,
                _ => target.additionalLightsShadowResolutionTierHigh
            };
        }

        private void MSAASampleCountChanged(float value)
        {
            var selection = Mathf.RoundToInt(value);

            target.msaaSampleCount = selection switch
            {
                0 => 0,
                1 => 2,
                2 => 4,
                3 => 8,
                _ => target.msaaSampleCount
            };
        }
    }
}