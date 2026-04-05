using System;
using DG.Tweening;
using UnityEngine;

namespace Tween.Base
{
    public abstract class MonoTween : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private float duration = 0.3f;

        [SerializeField] private Ease forwardCurve = Ease.InOutSine;
        [SerializeField] private Ease backwardCurve = Ease.InOutSine;

        [Header("Behaviour")]
        [SerializeField] private bool preInit;
        [SerializeField] private bool autoPlay;

        [SerializeField] private DisableAfter disableAfter = DisableAfter.Backward;

        private Tweener _tweener;
        private Component _target;

        private void Awake()
        {
            if (preInit)
                Backward(0, Ease.Flash);
        }

        private void Start()
        {
            if (autoPlay) Play();
        }

        public Tweener Play(bool reverse = false, Action onComplete = null)
        {
            _tweener?.Kill();
            _tweener = reverse ? Backward(duration, forwardCurve) : Forward(duration, backwardCurve);
            _target = _tweener.target as Component;

            _target?.gameObject.SetActive(true);

            _tweener.OnComplete(delegate
            {
                if (disableAfter == DisableAfter.Forward && !reverse || disableAfter == DisableAfter.Backward && reverse)
                    _target?.gameObject.SetActive(false);

                onComplete?.Invoke();
            });

            return _tweener;
        }

        protected abstract Tweener Forward(float duration, Ease easy);
        protected abstract Tweener Backward(float duration, Ease easy);

        private enum DisableAfter
        {
            Never,
            Forward,
            Backward
        }
    }
}