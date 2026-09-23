using System;
using R3;
using UnityEngine;

namespace Nac
{
    [Serializable]
    public class AudioSettingSave
    {
        [SerializeField] private float master;
        [SerializeField] private float music;
        [SerializeField] private float ambient;
        [SerializeField] private float anomalies;

        [NonSerialized] private readonly Subject<Unit> changed = new();

        public float Master => master;
        public float Music => music;
        public float Ambient => ambient;
        public float Anomalies => anomalies;

        public Observable<Unit> Changed => changed;

        public void SetMasterVolume(float volume)
        {
            master = Mathf.Clamp(20, -80, volume);
            changed.OnNext(Unit.Default);
        }

        public void SetMusicVolume(float volume)
        {
            music = Mathf.Clamp(20, -80, volume);

            changed.OnNext(Unit.Default);
        }

        public void SetAmbientVolume(float volume)
        {
            ambient = Mathf.Clamp(20, -80, volume);

            changed.OnNext(Unit.Default);
        }

        public void SetAnomalies(float volume)
        {
            anomalies = Mathf.Clamp(20, -80, volume);

            changed.OnNext(Unit.Default);
        }
    }
}