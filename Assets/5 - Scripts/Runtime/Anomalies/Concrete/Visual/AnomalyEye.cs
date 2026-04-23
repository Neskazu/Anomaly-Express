using System;
using DG.Tweening;
using R3;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Anomalies.Concrete.Visual
{
    public class AnomalyEye : AnomalyBase
    {
        [Header("Hierarchy References")]
        [SerializeField] private Transform driftPivot;
        [SerializeField] private Transform shakePivot;
        [SerializeField] private Transform eyeVisual;

        [Header("Floating Settings")]
        [SerializeField] private float driftRadius = 33f;
        [SerializeField] private float minDriftDuration = 0.15f;
        [SerializeField] private float maxDriftDuration = 2f;
        [SerializeField] private float jitterStrength = 0.5f;
        [SerializeField] private float minDriftPause = 0.5f;
        [SerializeField] private float maxDriftPause = 1.5f;

        [Header("Rotation Settings")]
        [SerializeField] private float maxLookAngle = 40.0f;
        [SerializeField] private float minRotationDuration = 0.2f;
        [SerializeField] private float maxRotationDuration = 0.6f;

        [Header("Logic")]
        [Range(0, 1)][SerializeField] private float spinChance = 0.15f;
        [SerializeField] private float minInterval = 0.5f;
        [SerializeField] private float maxInterval = 2.5f;

        private DG.Tweening.Sequence _moveSeq;
        private DG.Tweening.Sequence _rotSeq;
        private DG.Tweening.Tween _jitterTween;
        private IDisposable _logicSub;
        private Vector3 _initialPos;

        protected override void OnActivate()
        {
            // Теперь берем позицию driftPivot, так как двигаем его
            _initialPos = driftPivot.localPosition;

            // Дрожь работает на своем слое (shakePivot) и не мешает полету
            _jitterTween = shakePivot.DOShakePosition(1f, jitterStrength, 15)
                .SetLoops(-1, LoopType.Yoyo) // FullStep лучше для бесконечного шейка
                .SetEase(Ease.Linear);

            StartHugeDrift();
            ScheduleNextAction();
        }

        private void StartHugeDrift()
        {
            _moveSeq?.Kill();
            _moveSeq = DOTween.Sequence();
            Vector3 targetPos = _initialPos + Random.insideUnitSphere * driftRadius;
            float duration = Random.Range(minDriftDuration, maxDriftDuration);
            float pause = Random.Range(minDriftPause, maxDriftPause);
            var easeType = duration < 0.4f ? Ease.OutExpo : Ease.InOutQuad;
            _moveSeq.Append(driftPivot.DOLocalMove(targetPos, duration)
                .SetEase(easeType));
            _moveSeq.AppendInterval(pause);

            // Зацикливаем
            _moveSeq.OnComplete(StartHugeDrift);
        }

        private void ScheduleNextAction()
        {
            _logicSub = Observable
                .Timer(TimeSpan.FromSeconds(Random.Range(minInterval, maxInterval)))
                .Subscribe(_ =>
                {
                    PerformAction();
                    ScheduleNextAction();
                })
                .AddTo(this);
        }

        private void PerformAction()
        {
            _rotSeq?.Kill();
            _rotSeq = DOTween.Sequence();

            bool isSpinning = Random.value < spinChance;
            float duration = Random.Range(minRotationDuration, maxRotationDuration);

            if (isSpinning)
            {
                float spin = (Random.value > 0.5f ? 720f : 1080f) * (Random.value > 0.5f ? 1 : -1);

                _rotSeq.Append(eyeVisual.DOLocalRotate(new Vector3(0, 0, spin), duration * 3f, RotateMode.LocalAxisAdd)
                    .SetEase(Ease.OutBack, 1.5f));
            }
            else
            {
                Vector3 targetRot = new Vector3(
                    Random.Range(-maxLookAngle, maxLookAngle),
                    Random.Range(-maxLookAngle, maxLookAngle),
                    0
                );

                // Вращаем саму модель глаза
                _rotSeq.Append(eyeVisual.DOLocalRotate(targetRot, duration)
                    .SetEase(Ease.OutExpo));

            }
        }

        protected override void OnDeactivate()
        {
            _moveSeq?.Kill();
            _rotSeq?.Kill();
            _jitterTween?.Kill();
            _logicSub?.Dispose();
        }
    }
}