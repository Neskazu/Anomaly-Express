using Cysharp.Threading.Tasks;
using DG.Tweening;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Player.UI
{
    public class DeathScreen : MonoBehaviour, IWindow
    {
        public static DeathScreen Instance { get; private set; }

        [SerializeField] private Canvas canvas;
        [SerializeField] private Graphic targetGraphic;
        [SerializeField] private float duration = 0.5f;

        private const string DissolveProperty = "_Dissolve";
        private const string InvertProperty = "_Invert";

        private void Awake()
        {
            Instance = this;
        }

        public async UniTask Show()
        {
            await DOTween.Sequence()
                .Append(targetGraphic.material
                    .DOFloat(0.0f, InvertProperty, 0))
                .Append(targetGraphic.material
                    .DOFloat(0.0f, DissolveProperty, duration)
                    .From(1.0f)
                    .SetEase(Ease.InOutSine));
        }

        public async UniTask Hide()
        {
            await DOTween.Sequence()
                .Append(targetGraphic.material
                    .DOFloat(1.0f, InvertProperty, 0))
                .Append(targetGraphic.material
                    .DOFloat(1.0f, DissolveProperty, duration)
                    .From(0.0f)
                    .SetEase(Ease.InOutSine));
        }
    }
}