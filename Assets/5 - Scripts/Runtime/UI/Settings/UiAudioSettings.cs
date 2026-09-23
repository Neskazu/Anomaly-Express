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
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider ambientSlider;
        [SerializeField] private Slider anomaliesSlider;

        public void Start()
        {
            Sync();

            masterSlider.OnValueChangedAsObservable().Subscribe(ApplyMaster).AddTo(this);
            musicSlider.OnValueChangedAsObservable().Subscribe(ApplyMusic).AddTo(this);
            ambientSlider.OnValueChangedAsObservable().Subscribe(ApplyAmbient).AddTo(this);
            anomaliesSlider.OnValueChangedAsObservable().Subscribe(ApplyAnomalies).AddTo(this);

            Config.Changed.Subscribe(_ => Sync()).AddTo(this);

            Config.Changed
                .Debounce(TimeSpan.FromSeconds(0.5f))
                .Subscribe(_ => SaveManager.SaveGame())
                .AddTo(this);
        }

        private void Sync()
        {
            mixer.SetFloat("Master", Mathf.Log10(Config.Master) * 20f);
            mixer.SetFloat("Music", Mathf.Log10(Config.Music) * 20f);
            mixer.SetFloat("Ambient", Mathf.Log10(Config.Ambient) * 20f);
            mixer.SetFloat("Anomalies", Mathf.Log10(Config.Anomalies) * 20f);

            masterSlider.SetValueWithoutNotify(Config.Master);
            musicSlider.SetValueWithoutNotify(Config.Music);
            ambientSlider.SetValueWithoutNotify(Config.Ambient);
            anomaliesSlider.SetValueWithoutNotify(Config.Anomalies);
        }

        private void ApplyMaster(float value) => Config.SetMasterVolume(value);
        private void ApplyMusic(float value) => Config.SetMusicVolume(value);
        private void ApplyAmbient(float value) => Config.SetAmbientVolume(value);
        private void ApplyAnomalies(float value) => Config.SetAnomalies(value);
    }
}