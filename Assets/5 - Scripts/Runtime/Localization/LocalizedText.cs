using TMPro;
using UnityEngine;

namespace Localization
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField]
        private string key;
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
            LocalizationManager.Instance.OnLanguageChanged += Refresh;

            Refresh();
        }

        private void OnDisable()
        {
            if (LocalizationManager.Instance != null)
                LocalizationManager.Instance.OnLanguageChanged -= Refresh;
        }

        public void Refresh()
        {
            text.font = LocalizationManager.Instance.CurrentFont;
            text.text = LocalizationManager.Instance.Get(key);
        }

#if UNITY_EDITOR
        public void SetKey(string value)
        {
            key = value;
        }
#endif
    }
}