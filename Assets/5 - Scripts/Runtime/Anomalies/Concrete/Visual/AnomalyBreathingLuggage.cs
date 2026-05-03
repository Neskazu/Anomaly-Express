using UnityEngine;

namespace Anomalies
{
    public class AnomalyBreathingLuggage : AnomalyBase
    {
        [Header("Luggage Objects")]
        [SerializeField] private GameObject[] _bags;

        [Header("Breathing Settings")]
        [SerializeField] private float _breathAmplitude = 0.15f;
        [SerializeField] private float _breathFrequency = 2.0f;
        [SerializeField] private float _transitionSpeed = 1.0f;

        [Header("Organic Feel")]
        [Range(0f, 1f)]
        [SerializeField] private float _randomness = 0.3f;

        private Vector3[] _startScales;
        private float[] _phaseOffsets;
        private float[] _individualFrequencies;

        private float _effectIntensity = 0f;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            int count = _bags.Length;
            _startScales = new Vector3[count];
            _phaseOffsets = new float[count];
            _individualFrequencies = new float[count];

            for (int i = 0; i < count; i++)
            {
                if (_bags[i] == null) continue;

                _startScales[i] = _bags[i].transform.localScale;
                _phaseOffsets[i] = Random.Range(0f, Mathf.PI * 2);
                _individualFrequencies[i] = _breathFrequency * Random.Range(1f - _randomness, 1f + _randomness);
            }
        }

        protected override void OnActivate()
        {
        }

        protected override void OnDeactivate()
        {
        }

        protected override void OnUpdate()
        {
            float targetIntensity = IsActive ? 1f : 0f;
            _effectIntensity = Mathf.MoveTowards(_effectIntensity, targetIntensity, Time.deltaTime * _transitionSpeed);
            for (int i = 0; i < _bags.Length; i++)
            {
                if (_bags[i] == null) continue;
                float breathCycle = Mathf.Sin(Time.time * _individualFrequencies[i] + _phaseOffsets[i]);
                float scaleModifier = 1f + (breathCycle * _breathAmplitude * _effectIntensity);
                _bags[i].transform.localScale = _startScales[i] * scaleModifier;
            }
        }
    }
}