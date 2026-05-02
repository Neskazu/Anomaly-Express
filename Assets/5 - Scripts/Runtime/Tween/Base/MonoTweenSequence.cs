using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Tween.Base
{
    public class MonoTweenSequence : MonoBehaviour
    {
        [SerializeField] private MonoTween[] tweens;

        [Header("Behaviour")]
        [SerializeField] private bool preInit;
        [SerializeField] private bool autoPlay;
        [SerializeField] private DisableAfter disableAfter = DisableAfter.Backward;

        private Sequence _sequence;

        private void Awake()
        {
            if (!preInit)
            {
                return;
            }

            foreach (var tween in tweens)
            {
                tween.Play(true).Complete();
            }
        }

        private void Start()
        {
            if (autoPlay) Play();
        }

        public Sequence Play(bool reverse = false, Action onComplete = null)
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            gameObject.SetActive(true);
 
            foreach (var tween in reverse ? tweens.Reverse() : tweens)
            {
                _sequence.Append(tween.Play(reverse));
            }

            _sequence.OnComplete(delegate
            {
                if ((disableAfter == DisableAfter.Forward && !reverse) ||
                    (disableAfter == DisableAfter.Backward && reverse))
                {
                    gameObject.SetActive(false);
                }

                onComplete?.Invoke();
            });

            return _sequence.Play();
        }

        private enum DisableAfter
        {
            Never,
            Forward,
            Backward
        }
    }
}