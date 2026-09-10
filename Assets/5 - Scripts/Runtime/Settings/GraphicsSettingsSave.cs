using System;
using R3;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Nac
{
    [Serializable]
    public class GraphicsSettingsSave : ISerializationCallbackReceiver
    {
        [SerializeField] private AntialiasingMode antialiasingMode;
        [SerializeField] private MsaaQuality msaaQuality;
        [SerializeField] private float brightness;

        [NonSerialized] private ReactiveProperty<AntialiasingMode> reactiveAntialiasing = new(AntialiasingMode.None);
        [NonSerialized] private ReactiveProperty<MsaaQuality> reactiveMsaaQuality = new(MsaaQuality.Disabled);
        [NonSerialized] private ReactiveProperty<float> reactiveBrightness = new(0.0f);

        public ReadOnlyReactiveProperty<AntialiasingMode> Antialiasing => reactiveAntialiasing;

        public ReadOnlyReactiveProperty<MsaaQuality> Msaa =>
            reactiveMsaaQuality
                .CombineLatest(reactiveAntialiasing, (msaa, aa) => aa == AntialiasingMode.None ? msaa : MsaaQuality.Disabled)
                .ToReadOnlyReactiveProperty();

        public ReadOnlyReactiveProperty<float> Brightness => reactiveBrightness;

        public void SetAntialiasing(AntialiasingMode antialiasing)
        {
            reactiveAntialiasing.Value = antialiasing;
        }

        public void SetMsaaQuality(MsaaQuality quality)
        {
            reactiveMsaaQuality.Value = quality;
        }

        public void SetBrightness(float val)
        {
            reactiveBrightness.Value = Mathf.Clamp(val, -1f, 1f);
        }

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
    }
}