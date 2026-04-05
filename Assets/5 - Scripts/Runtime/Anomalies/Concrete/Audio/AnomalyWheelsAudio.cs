using DG.Tweening;
using Managers;
using UnityEngine;

namespace Anomalies.Concrete.Audio
{
    public class AnomalyWheelsAudio : AnomalyBase, IAudioAnomaly
    {
        [Header("References")]
        [SerializeField] private AudioSource anomalyAudioSource;

        [Header("Settings")]
        [SerializeField] private float changeTime = 2.0f;

        private AudioSource _wheelsAudioSource;
        private DG.Tweening.Tween _tween;

        protected override void OnActivate()
        {
            _wheelsAudioSource = SceneObjectsManager.Instance.WheelsAudio;

            anomalyAudioSource.volume = 0.0f;
            anomalyAudioSource.Play();

            _tween?.Kill();

            _tween = DOTween.Sequence()
                .Join(DOTween.To(() => anomalyAudioSource.volume, x => anomalyAudioSource.volume = x, 1.0f, changeTime))
                .Join(DOTween.To(() => _wheelsAudioSource.volume, x => _wheelsAudioSource.volume = x, 0.0f, changeTime));
        }

        protected override void OnDeactivate()
        {
            _tween?.Kill();

            anomalyAudioSource.Stop();
            anomalyAudioSource.volume = 0.0f;

            _wheelsAudioSource.volume = 1.0f;
        }
    }
}