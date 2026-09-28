using System;
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
        private object[] args;

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

        private void OnDestroy()
        {
            disposable.Dispose();
        }

        private void OnEnable()
        {
            LocalizationManager.Language
                .Subscribe(Refresh)
                .AddTo(disposable);
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

            var str = LocalizationManager.Instance.Get(key);
            try
            {
                text.text = args is { Length: > 0 } ? string.Format(str, args) : str;
            }
            catch (Exception e)
            {
                Debug.LogError("Issue with formatted localization: " + e.Message);
                text.text = str;
            }
        }

        public void SetKey(string newKey, params object[] newArgs)
        {
            key = newKey;
            args = newArgs;

            Refresh();
        }

#if UNITY_EDITOR
        public void SetKey_Editor(string value)
        {
            key = value;
        }
#endif
    }
}