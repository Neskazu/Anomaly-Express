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
            // 1. Захватываем стартовую позицию в самом начале, ДО любых Update
            if (doorTransform != null)
            {
                _initialLocalPos = doorTransform.localPosition;
                _isInitialized = true;
            }
        }

        protected override void OnActivate()
        {
            // Оборачиваем AudioMixer в try-catch для WebGL, чтобы скрипт не крашился, 
            // если браузер еще не дал разрешение на звук
            try
            {
                if (mixer != null)
                    mixer.SetFloat(anomaliesParameterName, IAudioAnomaly.Unmute);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("AudioMixer blocked by WebGL: " + e.Message);
            }

            if (source != null)
            {
                source.loop = true;
                source.Play();
            }
        }

        protected override void OnUpdate()
        {
            // Если позиция не инициализирована — ничего не делаем, чтобы не улететь в (0,0,0)
            if (doorTransform == null || !_isInitialized) return;

            // 2. Защита от лагов WebGL. Если дельта времени огромная (лаг), ограничиваем ее.
            float dt = Mathf.Min(Time.deltaTime, 0.1f);

            _shakeTimer += dt * shakeSpeed;
            float shakeValue = Mathf.Sin(_shakeTimer) * Mathf.Sin(_shakeTimer * 0.7f);

            if (shakeValue > 0.5f)
            {
                Vector3 offset = Random.insideUnitSphere * shakeIntensity;
                doorTransform.localPosition = _initialLocalPos + offset;
            }
            else
            {
                doorTransform.localPosition = Vector3.Lerp(doorTransform.localPosition, _initialLocalPos, dt * 10f);
            }
        }

        protected override void OnDeactivate()
        {
            if (source != null)
                source.Stop();

            try
            {
                if (mixer != null)
                    mixer.SetFloat(anomaliesParameterName, IAudioAnomaly.Mute);
            }
            catch { /* Игнорируем ошибки миксера */ }

            if (doorTransform != null && _isInitialized)
                doorTransform.localPosition = _initialLocalPos;
        }
    }
}