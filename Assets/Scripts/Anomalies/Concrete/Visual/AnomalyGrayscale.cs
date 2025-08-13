using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

namespace Anomalies.Concrete.Visual
{
    public class AnomalyGrayscale : AnomalyBase
    {
        [Header("References")]
        [SerializeField] private Volume volume;

        [Header("Settings")]
        [SerializeField] private float time = 10f;

        private DG.Tweening.Tween _tween;

        protected override void OnActivate()
        {
            volume.weight = 0.0f;

            _tween = DOTween
                .To(() => volume.weight, x => volume.weight = x, 1.0f, time)
                .SetEase(Ease.InOutSine);
        }

        protected override void OnDeactivate()
        {
            _tween?.Kill();

            volume.weight = 0.0f;
        }
    }
}