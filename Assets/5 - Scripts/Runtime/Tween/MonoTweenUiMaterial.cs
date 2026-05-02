using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Tween.Base;

namespace Tween
{
    public class MonoTweenUiMaterial : MonoTween
    {
        [Header("Target UI Component")]
        [SerializeField] private Graphic targetGraphic;
        [SerializeField] private string propertyName = "_FloatValue";

        [Header("Tween Values")]
        [SerializeField] private float from = 0f;
        [SerializeField] private float to = 1f;

        protected override Tweener Forward(float duration, Ease ease)
        {
            return targetGraphic.material
                .DOFloat(to, propertyName, duration)
                .From(from)
                .SetEase(ease);
        }

        protected override Tweener Backward(float duration, Ease ease)
        {
            return targetGraphic.material
                .DOFloat(from, propertyName, duration)
                .From(to)
                .SetEase(ease);
        }
    }
}