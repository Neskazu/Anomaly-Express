using DG.Tweening;
using Tween.Base;
using UnityEngine;

namespace Tween
{
    public class MonoTweenSize : MonoTween
    {
        [SerializeField] private RectTransform target;

        [SerializeField] private Vector2 from = new(0f, 0f);
        [SerializeField] private Vector2 to = new(1, 1);

        protected override Tweener Forward(float duration, Ease easy)
        {
            return target
                .DOSizeDelta(to, duration)
                .From(from)
                .SetEase(easy);
        }

        protected override Tweener Backward(float duration, Ease easy)
        {
            return target
                .DOSizeDelta(from, duration)
                .From(to)
                .SetEase(easy);
        }
    }
}