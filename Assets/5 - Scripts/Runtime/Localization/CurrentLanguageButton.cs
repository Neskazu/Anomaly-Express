using UnityEngine;
using UnityEngine.UI;
using Localization;
using Nac.Extensions;
using R3;
using TMPro;

public class CurrentLanguageButton : MonoBehaviour
{
    [SerializeField] private Image flag;
    [SerializeField] private TMP_Text languageName;

    private readonly CompositeDisposable disposables = new();

    private void OnDestroy()
    {
        disposables.Dispose();
    }

    private void OnEnable()
    {
        LocalizationManager.Language
            .Subscribe(Refresh)
            .AddTo(disposables);
    }

    private void OnDisable()
    {
        disposables.Clear();
    }

    public void Refresh()
    {
        var language = LocalizationManager.Instance.GetCurrentLanguage();

        if (language != null)
        {
            flag.sprite = language.Flag;
            languageName.text = language.Info.NativeName;
            languageName.font = LocalizationManager.Instance.GetFontForLanguage(language.Info.Code);
        }
    }
}