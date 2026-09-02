using TMPro;
using UnityEngine;
using R3;
using Nac.Extensions;

namespace Localization
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string key;

        private readonly CompositeDisposable disposable = new();

        public string Key
        {
            get => key;
            set
            {
                key = value;
                Refresh();
            }
        }

        private TMP_Text text;

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            LocalizationManager.Language
                .Subscribe(Refresh)
                .AddTo(disposable);
        }

        private void OnDestroy()
        {
            disposable.Dispose();
        }

        private void OnDisable()
        {
            disposable.Clear();
        }

        public void Refresh()
        {
            if (LocalizationManager.Instance == null)
            {
                return;
            }

            text.font = LocalizationManager.Instance.CurrentFont;
            text.text = LocalizationManager.Instance.Get(key);
        }

#if UNITY_EDITOR
        public void SetKey_Editor(string value)
        {
            key = value;
        }
#endif
    }
}