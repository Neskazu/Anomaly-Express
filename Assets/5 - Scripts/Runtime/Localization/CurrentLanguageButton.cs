using UnityEngine;
using UnityEngine.UI;
using Localization;
using TMPro;

public class CurrentLanguageButton : MonoBehaviour
{
    [SerializeField] private Image flag;
    [SerializeField] private TMP_Text languageName;

    private void OnEnable()
    {
        LocalizationManager.Instance.OnLanguageChanged += Refresh;
        Refresh();
    }
    private void OnDisable()
    {
        LocalizationManager.Instance.OnLanguageChanged -= Refresh;
    }
    public void Refresh()
    {
        Language language = LocalizationManager.Instance.GetCurrentLanguage();

        if (language != null)
        {
            flag.sprite = language.Flag;
            languageName.text = language.Info.NativeName;
        }
            
    }
}