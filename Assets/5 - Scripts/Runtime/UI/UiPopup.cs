using Cysharp.Threading.Tasks;
using Localization;
using Nac.Extensions;
using Nac.Singleton;
using R3;
using R3.Triggers;
using Tween.Base;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UiPopup : Service<UiPopup>
    {
        [SerializeField] private GameObject shield;
        [SerializeField] private UISoundPlayer uiSoundPlayer;

        [Header("Background")]
        [SerializeField] private GameObject background;
        [SerializeField] private MonoTweenSequence bgShowSequence;
        [SerializeField] private MonoTweenSequence bgHideSequence;
        [SerializeField] private bool useBgShowReversed;

        [Header("Info")]
        [SerializeField] private LocalizedText titleText;
        [SerializeField] private LocalizedText infoText;
        [SerializeField] private LocalizedText buttonText;
        [SerializeField] private Button button;
        [SerializeField] private MonoTweenSequence infoShowSequence;
        [SerializeField] private MonoTweenSequence infoHideSequence;
        [SerializeField] private bool useInfoShowReversed;

        private readonly CompositeDisposable disposable = new();
        private bool bgActive;

        public override void Awake()
        {
            base.Awake();

            button.OnPointerEnterAsObservable()
                .Subscribe(uiSoundPlayer.PlayHover)
                .AddTo(this);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            disposable.Dispose();
        }

        public async UniTask Show(string titleKey, string textKey, string buttonKey)
        {
            titleText.SetKey(titleKey);
            infoText.SetKey(textKey);
            buttonText.SetKey(buttonKey);

            button.OnClickAsObservable()
                .Subscribe(Hide)
                .AddTo(disposable);

            await ShowBg();
            await infoShowSequence.Play();
        }

        public async UniTask Hide()
        {
            disposable.Clear();

            if (useInfoShowReversed)
            {
                await infoShowSequence.Play(true);
            }
            else
            {
                await infoHideSequence.Play();
            }

            await HideBg();
        }

        private async UniTask ShowBg()
        {
            if (bgActive)
            {
                return;
            }

            shield.SetActive(true);
            bgActive = true;

            await bgShowSequence.Play();
        }

        private async UniTask HideBg()
        {
            if (useBgShowReversed)
            {
                await bgShowSequence.Play(true);
            }
            else
            {
                await bgHideSequence.Play();
            }

            shield.SetActive(false);
        }
    }
}