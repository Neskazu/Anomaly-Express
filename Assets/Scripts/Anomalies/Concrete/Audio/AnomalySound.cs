using UnityEngine;

namespace Anomalies.Concrete.Audio
{
    public class AnomalySound : AnomalyBase, IAudioAnomaly
    {
        [Header("References")]
        [SerializeField] private AudioSource source;

        protected override void OnActivate()
        {
            source.Play();
        }

        protected override void OnDeactivate()
        {
            source.Stop();
        }
    }
}