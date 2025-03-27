using DG.Tweening;
using Tween.Base;
using UnityEngine;

namespace Source.Tween
{
    public class MonoTweenAnchorPos : MonoTween
    {
        [SerializeField] private RectTransform target;

        [SerializeField] private Vector2 from;
        [SerializeField] private Vector2 to;

        protected override Tweener Forward(float duration, Ease easy)
        {
            return target
                .DOAnchorPos(to, duration)
                .SetEase(easy);
        }

        protected override Tweener Backward(float duration, Ease easy)
        {
            return target
                .DOAnchorPos(from, duration)
                .SetEase(easy);
        }
    }
}