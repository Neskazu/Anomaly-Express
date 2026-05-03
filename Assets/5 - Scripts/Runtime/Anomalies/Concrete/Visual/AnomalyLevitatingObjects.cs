using UnityEngine;

namespace Anomalies
{
    public class AnomalyLevitatingObjects : AnomalyBase
    {
        [Header("Objects to Float")]
        [SerializeField] private GameObject[] _objects;

        [Header("Floating Settings")]
        [SerializeField] private float _floatHeight = 1.5f;
        [SerializeField] private float _bobAmplitude = 0.1f;
        [SerializeField] private float _bobFrequency = 0.8f;
        [SerializeField] private float _rotationSpeed = 15f;
        [SerializeField] private float _transitionSpeed = 2f;

        private Vector3[] _startPositions;
        private Quaternion[] _startRotations;
        private float[] _phaseOffsets;

        private float _levitationProgress = 0f;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            int count = _objects.Length;
            _startPositions = new Vector3[count];
            _startRotations = new Quaternion[count];
            _phaseOffsets = new float[count];

            for (int i = 0; i < count; i++)
            {
                if (_objects[i] == null) continue;

                _startPositions[i] = _objects[i].transform.position;
                _startRotations[i] = _objects[i].transform.rotation;
                _phaseOffsets[i] = Random.Range(0f, 100f);
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
            float targetProgress = IsActive ? 1f : 0f;
            _levitationProgress = Mathf.MoveTowards(_levitationProgress, targetProgress, Time.deltaTime * _transitionSpeed);

            for (int i = 0; i < _objects.Length; i++)
            {
                if (_objects[i] == null) continue;
                float bobbing = Mathf.Sin(Time.time * _bobFrequency + _phaseOffsets[i]) * _bobAmplitude;
                Vector3 targetPos = _startPositions[i] + Vector3.up * (_floatHeight * _levitationProgress + bobbing * _levitationProgress);
                _objects[i].transform.position = Vector3.Lerp(_startPositions[i], targetPos, _levitationProgress);
                if (_levitationProgress > 0.01f)
                {
                    _objects[i].transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime * _levitationProgress);
                }
                else
                {
                    _objects[i].transform.rotation = Quaternion.Slerp(_objects[i].transform.rotation, _startRotations[i], Time.deltaTime * _transitionSpeed);
                }
            }
        }
    }
}