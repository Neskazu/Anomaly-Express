using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        [SerializeField] private bool autoPlay;

        [SerializeField] private DisableAfter disableAfter = DisableAfter.Backward;

        private Tweener _tweener;
        private CancellationToken _destroyCancellation;

        private void Awake()
        {
            _destroyCancellation = this.GetCancellationTokenOnDestroy();
        }

        private async void Start()
        {
            if (autoPlay) await Play();
        }

        public async UniTask Play(bool reverse = false)
        {
            _tweener?.Kill();
            _tweener = reverse ? Backward(duration, forwardCurve) : Forward(duration, backwardCurve);
            gameObject.SetActive(true);

            await _tweener.WithCancellation(_destroyCancellation);

            if (disableAfter == DisableAfter.Forward && !reverse || (disableAfter == DisableAfter.Backward && reverse))
                gameObject.SetActive(false);
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