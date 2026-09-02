using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Localization;
using Nac.Extensions;
using R3;

public class LanguageItem : MonoBehaviour
{
    [SerializeField] private Image flag;
    [SerializeField] private TMP_Text languageName;
    [SerializeField] private Button button;
    [SerializeField] private GameObject selected;

    private readonly CompositeDisposable disposables = new();
    private Language language;

    public void Initialize(Language lang)
    {
        language = lang;

        flag.sprite = lang.Flag;
        languageName.text = lang.Info.NativeName;
        languageName.font = LocalizationManager.Instance.GetFontForLanguage(lang.Info.Code);
        var isCurrent =
            LocalizationManager.Instance.GetCurrentLanguage().Info.Code == language.Info.Code;

        selected.SetActive(isCurrent);

        button.onClick.RemoveListener(OnClick);
        button.onClick.AddListener(OnClick);
    }

    private void OnEnable()
    {
        LocalizationManager.Language
            .Subscribe(RefreshSelected)
            .AddTo(disposables);
    }

    private void OnDisable()
    {
        disposables.Clear();
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