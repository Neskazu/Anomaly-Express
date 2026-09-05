using Nac.Extensions;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UiExitButton : MonoBehaviour
    {
        [SerializeField] protected Button button;
        [SerializeField] private UISoundPlayer uiSoundPlayer;

        private void Awake()
        {
            button.OnClickAsObservable()
                .Subscribe(Exit)
                .AddTo(this);

            button.OnPointerEnterAsObservable()
                .Subscribe(uiSoundPlayer.PlayHover)
                .AddTo(this);
        }

        private void Exit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}