using UnityEngine;

namespace Anomalies
{
    public class ParticleSwitchComponent : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particles;

        private void OnEnable()
        {
            particles.Play();
        }

        private void OnDisable()
        {
            particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}