using DG.Tweening;
using R3;
using UnityEngine;

namespace Anomalies
{
    public class FantomVisualComponent : MonoBehaviour
    {
        [SerializeField] private FantomComponent controller;

        [Space]
        [SerializeField] private GameObject onRevealed;
        [SerializeField] private GameObject onConcealed;

        private DG.Tweening.Tween anim;

        private void Start()
        {
            controller.Revealed
                .Subscribe(RevealedCallback)
                .AddTo(this);

            anim = DOTween.Sequence()
                .SetAutoKill(false)
                .Pause();
        }

        private void OnDestroy()
        {
            anim?.Kill();
            anim = null;
        }

        private void RevealedCallback(bool revealed)
        {
            anim.OnComplete(() => OnCompleteCallback(revealed));

            onRevealed.SetActive(revealed);
            onConcealed.SetActive(!revealed);

            if (revealed)
            {
                anim.PlayForward();
                return;
            }

            anim.PlayBackwards();
        }

        private void OnCompleteCallback(bool revealed)
        {
            onRevealed.SetActive(revealed);
            onConcealed.SetActive(!revealed);
        }
    }
}