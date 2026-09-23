using System;
using R3;
using UnityEngine;

namespace Nac
{
    [Serializable]
    public class AudioSettingSave
    {
        // 1f = 100% громкости (0 децибел)
        [SerializeField] private float master = 1f;
        [SerializeField] private float music = 1f;
        [SerializeField] private float ambient = 1f;
        [SerializeField] private float anomalies = 1f;

        [NonSerialized] private readonly Subject<Unit> changed = new();

        public float Master => master;
        public float Music => music;
        public float Ambient => ambient;
        public float Anomalies => anomalies;

        public Observable<Unit> Changed => changed;

        public void SetMasterVolume(float volume)
        {
            master = Mathf.Clamp(volume, 0.0001f, 1f);
            changed.OnNext(Unit.Default);
        }

        public void SetMusicVolume(float volume)
        {
            music = Mathf.Clamp(volume, 0.0001f, 1f);
            changed.OnNext(Unit.Default);
        }

        public void SetAmbientVolume(float volume)
        {
            ambient = Mathf.Clamp(volume, 0.0001f, 1f);
            changed.OnNext(Unit.Default);
        }

        public void SetAnomalies(float volume)
        {
            anomalies = Mathf.Clamp(volume, 0.0001f, 1f);
            changed.OnNext(Unit.Default);
        }
    }
}