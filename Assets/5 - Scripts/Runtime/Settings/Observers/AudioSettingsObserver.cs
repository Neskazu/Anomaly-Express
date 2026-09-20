using Nac.Extensions;
using R3;
using SaveSystem;
using UnityEngine;
using UnityEngine.Audio;

namespace Nac
{
    public class AudioSettingsObserver : MonoBehaviour
    {
        private static AudioSettingSave Config => SaveManager.Save.AudioSetting;

        [SerializeField] private AudioMixer mixer;

        private void Awake()
        {
            SaveManager.OnLoaded
                .Subscribe(ApplySoundSettings)
                .AddTo(this);

            ApplySoundSettings();
        }

        private void ApplySoundSettings()
        {
            mixer.SetFloat("Master", Config.Master);
            mixer.SetFloat("Music", Config.Music);
            mixer.SetFloat("Effects", Config.Effects);
            mixer.SetFloat("Anomalies", Config.Anomalies);
        }
    }
}