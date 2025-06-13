using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Tween.Base
{
    public class MonoTweenSequence : MonoBehaviour
    {
        [SerializeField] private MonoTween[] tweens;

        private Sequence _sequence;

        public Sequence Play(bool reverse = false, Action onComplete = null)
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence();
            _sequence.OnComplete(delegate { onComplete?.Invoke(); });

            foreach (MonoTween tween in reverse ? tweens.Reverse() : tweens)
                _sequence.Append(tween.Play(reverse));

            return _sequence.Play();
        }
    }
}