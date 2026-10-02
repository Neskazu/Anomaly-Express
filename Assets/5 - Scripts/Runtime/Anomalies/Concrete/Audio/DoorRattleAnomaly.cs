using UnityEngine;
using UnityEngine.Audio;

namespace Anomalies.Concrete.Audio
{
    public class DoorRattleAnomaly : AnomalyBase, IAudioAnomaly
    {
        [Header("References")]
        [SerializeField] private Transform doorTransform;
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private string anomaliesParameterName;

        [Header("Shake Settings")]
        [SerializeField] private float shakeIntensity = 0.08f;
        [SerializeField] private float shakeSpeed = 25f;

        private Vector3 _initialLocalPos;
        private float _shakeTimer;
        private bool _isInitialized;

        private void Awake()
        {
            if (doorTransform != null)
            {
                _initialLocalPos = doorTransform.localPosition;
                _isInitialized = true;
            }
        }

        protected override void OnActivate()
        {
            mixer.SetFloat(anomaliesParameterName, IAudioAnomaly.Unmute);

            if (source != null)
            {
                source.loop = true;
                source.Play();
            }
        }

        protected override void OnUpdate()
        {
            if (doorTransform == null || !_isInitialized) return;

            _shakeTimer += Time.deltaTime * shakeSpeed;
            float shakeValue = Mathf.Sin(_shakeTimer) * Mathf.Sin(_shakeTimer * 0.7f);

            if (shakeValue > 0.5f)
            {
                Vector3 offset = Random.insideUnitSphere * shakeIntensity;
                doorTransform.localPosition = _initialLocalPos + offset;
            }
            else
            {
                doorTransform.localPosition = Vector3.Lerp(doorTransform.localPosition, _initialLocalPos, Time.deltaTime * 10f);
            }
        }

        protected override void OnDeactivate()
        {
            if (source != null)
                source.Stop();

            mixer.SetFloat(anomaliesParameterName, IAudioAnomaly.Mute);

            if (doorTransform != null && _isInitialized)
                doorTransform.localPosition = _initialLocalPos;
        }
    }
}