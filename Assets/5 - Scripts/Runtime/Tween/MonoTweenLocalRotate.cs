using DG.Tweening;
using Tween.Base;
using UnityEngine;

namespace Tween
{
    public class MonoTweenLocalRotate : MonoTween
    {
        [SerializeField] private Transform target;

        [SerializeField] private Vector3 from;
        [SerializeField] private Vector3 to;

        protected override Tweener Forward(float duration, Ease easy)
        {
            return target
                .DOLocalRotate(to, duration)
                .SetEase(easy);
        }

        protected override Tweener Backward(float duration, Ease easy)
        {
            return target
                .DOLocalRotate(from, duration)
                .SetEase(easy);
        }
    }
}