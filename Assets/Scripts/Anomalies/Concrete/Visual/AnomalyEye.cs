using System;
using DG.Tweening;
using R3;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Anomalies.Concrete.Visual
{
    public class AnomalyEye : AnomalyBase
    {
        [Header("References")]
        [SerializeField] private Transform eyeHolderTransform;
        [SerializeField] private Transform eyeTransform;

        [Header("Floating Settings")]
        [SerializeField] private Vector3 floatingOffset = Vector3.up * 3;
        [SerializeField] private float floatingDuration = 5.0f;

        [Header("Focus Settings")]
        [SerializeField] private float interval;
        [SerializeField] private float angle = 20.0f;
        [SerializeField] private float duration = .5f;

        private DG.Tweening.Tween _floatingTween;
        private DG.Tweening.Tween _rotatingTween;
        private IDisposable _subscription;

        protected override void OnActivate()
        {
            if (interval < duration)
                interval = duration;

            _subscription = Observable
                .Interval(TimeSpan.FromSeconds(interval))
                .Subscribe(Rotate)
                .AddTo(this);

            _floatingTween = eyeTransform.DOMove(eyeHolderTransform.position + floatingOffset, floatingDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        protected override void OnDeactivate()
        {
            _floatingTween?.Kill();
            _rotatingTween?.Kill();
            _subscription?.Dispose();
        }

        private void Rotate(Unit _)
        {
            Vector3 randomRotation = new(
                Random.Range(-angle, angle),
                Random.Range(-angle, angle),
                Random.Range(-angle, angle)
            );

            _rotatingTween = eyeTransform.DOLocalRotate(randomRotation, duration)
                .SetEase(Ease.InOutSine);
        }
    }
}