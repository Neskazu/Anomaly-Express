using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

namespace Anomalies.Concrete.Audio
{
    public class AnomalySilence : AnomalyBase
    {
        [Header("References")]
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private string masterParameterName;

        [Header("Settings")]
        [SerializeField] private float soundsFadeTime;

        private const float Mute = -80; 
        private DG.Tweening.Tween _tween;
        private float _initialMasterVolume;

        protected override void OnActivate()
        {
            mixer.GetFloat(masterParameterName, out _initialMasterVolume);

            _tween = mixer.DOSetFloat(masterParameterName, Mute, soundsFadeTime);
        }

        protected override void OnDeactivate()
        {
            _tween?.Kill();

            mixer.SetFloat(masterParameterName, _initialMasterVolume);
        }
    }
}