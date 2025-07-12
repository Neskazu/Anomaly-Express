using System;
using R3;
using UnityEngine;
using UnityEngine.Audio;

namespace Anomalies.Concrete
{
    public class AnomalyMusic : AnomalyBase
    {
        [Header("References")]
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioMixer globalMixer;
        [SerializeField] private string musicParameterName;
        [SerializeField] private string anomaliesParameterName;

        [Header("Settings")]
        [SerializeField] private float deltaTime;
        [SerializeField] private float deltaMusicVolume;
        [SerializeField] private float deltaAnomalyVolume;

        private IDisposable _subscription;
        private float _initialMusicVolume;
        private float _musicVolume;
        private float _anomalyVolume;

        private const float Mute = -80;
        private const float Unmute = 0;

        protected override void OnActivate()
        {
            globalMixer.GetFloat(musicParameterName, out _initialMusicVolume);
            globalMixer.SetFloat(anomaliesParameterName, Mute);

            _musicVolume = _initialMusicVolume;
            _anomalyVolume = Mute;

            source.Play();

            _subscription = Observable
                .Interval(TimeSpan.FromSeconds(deltaTime))
                .Subscribe(_ => VolumeUpdate())
                .AddTo(this);
        }

        protected override void OnDeactivate()
        {
            _subscription.Dispose();
            source.Stop();

            globalMixer.SetFloat(musicParameterName, _initialMusicVolume);
            globalMixer.SetFloat(anomaliesParameterName, Unmute);
        }

        private void VolumeUpdate()
        {
            _musicVolume = Mathf.Clamp(_musicVolume - deltaMusicVolume, Mute, Unmute);
            _anomalyVolume = Mathf.Clamp(_anomalyVolume + deltaAnomalyVolume, Mute, Unmute);

            globalMixer.SetFloat(musicParameterName, _musicVolume);
            globalMixer.SetFloat(anomaliesParameterName, _anomalyVolume);

            if (_anomalyVolume >= Unmute && _musicVolume <= Mute)
                _subscription.Dispose();
        }
    }
}