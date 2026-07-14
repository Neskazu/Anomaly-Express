using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Localization;

public class LanguageItem : MonoBehaviour
{
    [SerializeField] private Image flag;
    [SerializeField] private TMP_Text languageName;
    [SerializeField] private Button button;
    [SerializeField] private GameObject selected;

    private Language language;

    public void Initialize(Language lang)
    {
        language = lang;

        flag.sprite = lang.Flag;
        languageName.text = lang.Info.NativeName;
        languageName.font = LocalizationManager.Instance.GetFontForLanguage(lang.Info.Code);
        bool isCurrent =
    LocalizationManager.Instance.GetCurrentLanguage().Info.Code == language.Info.Code;

        selected.SetActive(isCurrent);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }
    private void OnEnable()
    {
        LocalizationManager.Instance.OnLanguageChanged += RefreshSelected;
    }

    private void OnDisable()
    {
        LocalizationManager.Instance.OnLanguageChanged -= RefreshSelected;
    }

    private void RefreshSelected()
    {
        selected.SetActive(
            LocalizationManager.Instance.GetCurrentLanguage().Info.Code == language.Info.Code);
    }
    private void OnClick()
    {
        LocalizationManager.Instance.SetLanguage(language.Info.Code);
    }
}