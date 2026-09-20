using System;
using Nac.Extensions;
using R3;
using SaveSystem;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Nac
{
    public class UiAudioSettings : MonoBehaviour
    {
        private static AudioSettingSave Config => SaveManager.Save.AudioSetting;

        [SerializeField] private AudioMixer mixer;
        [Space]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider effectSlider;
        [SerializeField] private Slider anomaliesSlider;

        public void Start()
        {
            Sync();

            masterSlider
                .OnValueChangedAsObservable()
                .Subscribe(ApplyMaster)
                .AddTo(this);

            musicSlider
                .OnValueChangedAsObservable()
                .Subscribe(ApplyMusic)
                .AddTo(this);

            effectSlider
                .OnValueChangedAsObservable()
                .Subscribe(ApplyEffects)
                .AddTo(this);

            anomaliesSlider
                .OnValueChangedAsObservable()
                .Subscribe(ApplyAnomalies)
                .AddTo(this);

            Config.Changed
                .Subscribe(Sync)
                .AddTo(this);
        }

        private void Sync()
        {
            mixer.SetFloat("Master", Config.Master);
            mixer.SetFloat("Music", Config.Music);
            mixer.SetFloat("Effects", Config.Effects);
            mixer.SetFloat("Anomalies", Config.Anomalies);

            masterSlider.SetValueWithoutNotify(Config.Master);
            musicSlider.SetValueWithoutNotify(Config.Music);
            effectSlider.SetValueWithoutNotify(Config.Effects);
            anomaliesSlider.SetValueWithoutNotify(Config.Anomalies);
        }

        private void ApplyMaster(float value)
        {
            Config.SetMasterVolume(value);
            SaveManager.SaveGame();
        }

        private void ApplyMusic(float value)
        {
            Config.SetMusicVolume(value);
            SaveManager.SaveGame();
        }

        private void ApplyEffects(float value)
        {
            Config.SetEffectsVolume(value);
            SaveManager.SaveGame();
        }

        private void ApplyAnomalies(float value)
        {
            Config.SetAnomalies(value);
            SaveManager.SaveGame();
        }
    }
}