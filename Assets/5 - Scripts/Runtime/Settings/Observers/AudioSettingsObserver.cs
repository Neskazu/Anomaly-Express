using Nac.Extensions;
using R3;
using SaveSystem;
using UnityEngine;
using UnityEngine.Audio;

namespace Nac
{
    public class AudioSettingsObserver : MonoBehaviour
    {
        [SerializeField] private AudioMixer mixer;

        private void Awake()
        {
            SaveManager.OnLoaded
                .Subscribe(_ => ApplySoundSettings())
                .AddTo(this);

            if (SaveManager.Save?.AudioSetting != null)
            {
                ApplySoundSettings();
            }
        }

        private void ApplySoundSettings()
        {
            if (mixer == null)
            {
                return;
            }

            var config = SaveManager.Save?.AudioSetting;
            if (config == null) return;

            mixer.SetFloat("Master", Mathf.Log10(config.Master) * 20f);
            mixer.SetFloat("Music", Mathf.Log10(config.Music) * 20f);
            mixer.SetFloat("Ambient", Mathf.Log10(config.Ambient) * 20f);
            mixer.SetFloat("Anomalies", Mathf.Log10(config.Anomalies) * 20f);
        }
    }
}