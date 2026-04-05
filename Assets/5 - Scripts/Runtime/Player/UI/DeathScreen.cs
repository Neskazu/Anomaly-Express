using Cysharp.Threading.Tasks;
using DG.Tweening;
using UI.Base;
using UnityEngine;

namespace Player.UI
{
    public class DeathScreen : MonoBehaviour, IWindow
    {
        public static DeathScreen Instance { get; private set; }

        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform deathScreen;
        [SerializeField] private float animationDuration = 0.5f;

        private void Awake()
        {
            Instance = this;
        }

        public async UniTask Show()
        {
            await deathScreen
                .DOAnchorPos(Vector2.zero, animationDuration)
                .From(new Vector2(Screen.width, 0))
                .SetEase(Ease.InOutCubic);
        }

        public async UniTask Hide()
        {
            await deathScreen
                .DOAnchorPos(new Vector2(-Screen.width, 0), animationDuration)
                .SetEase(Ease.InOutCubic)
                .From(Vector2.zero);
        }
    }
}