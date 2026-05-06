using UnityEngine;
using DG.Tweening;

namespace Anomalies
{
    public class CameraFovAnomaly : AnomalyBase
    {
        [Header("FOV Limits")]
        [SerializeField] private float targetFovMin = 5f;
        [SerializeField] private float targetFovMax = 160f;

        [Header("Animation Settings")]
        [SerializeField] private float duration = 45f;
        [SerializeField] private float durationBack = 1.5f;

        private float _defaultFov;
        private Camera _localCamera;
        protected override void OnActivate()
        {

            _localCamera = Camera.main;
            if (_localCamera != null) _defaultFov = _localCamera.fieldOfView;
            float finalTarget = (Random.value > 0.5f) ? targetFovMax : targetFovMin;
            _localCamera.DOFieldOfView(finalTarget, duration);
        }

        protected override void OnDeactivate()
        {
            _localCamera.DOFieldOfView(_defaultFov, durationBack);
        }
        protected override void OnUpdate() { }
    }
}