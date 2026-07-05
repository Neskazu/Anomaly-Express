using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.VFX;

namespace Anomalies
{
    public class FantomVisualComponent : MonoBehaviour
    {
        [SerializeField] private FantomComponent controller;

        [Space]
        [SerializeField] private GameObject onRevealed;
        [SerializeField] private GameObject onConcealed;

        [Space]
        [SerializeField] private VisualEffect effect;

        private DG.Tweening.Tween anim;

        private void Start()
        {
            controller.Revealed
                .Subscribe(RevealedCallback)
                .AddTo(this);

            anim = DOTween.Sequence()
                .AppendInterval(1f)
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

            effect.SetBool("Activate", revealed);

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