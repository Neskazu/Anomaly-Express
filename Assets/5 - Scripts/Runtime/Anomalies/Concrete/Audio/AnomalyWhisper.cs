using DG.Tweening;
using Managers;
using Network;
using Player;
using UnityEngine;
using UnityEngine.Audio;

namespace Anomalies.Concrete.Audio
{
    public class AnomalyWhisper : AnomalyBase
    {
        [Header("References")]
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private string musicParameterName;
        [SerializeField] private string anomaliesParameterName;

        [Header("Settings")]
        [SerializeField] private float rollbackTime;
        [SerializeField] private float musicFadeTime;
        [SerializeField] private float anomalyAppearanceTime;
        [SerializeField] private float triggerVelocityMagnitude;

        private const float Mute = -80;
        private const float Unmute = 0;

        private bool _triggered;
        private DG.Tweening.Tween _tween;

        private float _initialMusicVolume;
        private float _musicVolume;
        private float _anomalyVolume;

        protected override void OnActivate()
        {
            MultiplayerManager.Players.OnUpdated += PlayerUpdated;

            mixer.GetFloat(musicParameterName, out _initialMusicVolume);
            mixer.SetFloat(anomaliesParameterName, Mute);

            source.Play();
        }

        protected override void OnDeactivate()
        {
            MultiplayerManager.Players.OnUpdated -= PlayerUpdated;

            _tween?.Kill();
            source.Stop();

            mixer.SetFloat(musicParameterName, _initialMusicVolume);
            mixer.SetFloat(anomaliesParameterName, Unmute);
        }

        private void PlayerUpdated(PlayerData data)
        {
            if (data.ClientId != PlayerController.LocalPlayerId)
                return;

            switch (_triggered)
            {
                case false when data.Velocity.magnitude <= triggerVelocityMagnitude:
                    Run();
                    break;
                case true when data.Velocity.magnitude >= triggerVelocityMagnitude:
                    Stop();
                    break;
            }
        }

        private void Run()
        {
            _tween?.Kill();

            _tween = DOTween.Sequence()
                .Join(mixer.DOSetFloat(musicParameterName, Mute, musicFadeTime))
                .Join(mixer.DOSetFloat(anomaliesParameterName, Unmute, anomalyAppearanceTime));

            _triggered = true;
        }

        private void Stop()
        {
            _tween?.Kill();

            _tween = DOTween.Sequence()
                .Join(mixer.DOSetFloat(musicParameterName, _initialMusicVolume, rollbackTime))
                .Join(mixer.DOSetFloat(anomaliesParameterName, Mute, rollbackTime));

            _triggered = false;
        }
    }
}