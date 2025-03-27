using DG.Tweening;
using Tween.Base;
using UnityEngine;

namespace Source.Tween
{
    public class MonoTweenFade : MonoTween
    {
        [SerializeField] private CanvasGroup target;

        [SerializeField] [Range(0f, 1f)] private float from = 1f;
        [SerializeField] [Range(0f, 1f)] private float to = 0f;

        protected override Tweener Forward(float duration, Ease easy)
        {
            return target
                .DOFade(to, duration)
                .From(from)
                .SetEase(easy);
        }

        protected override Tweener Backward(float duration, Ease easy)
        {
            return target
                .DOFade(from, duration)
                .From(to)
                .SetEase(easy);
        }
    }
}