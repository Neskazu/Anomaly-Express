using UnityEngine;
using Localization;

public class LanguagePanel : MonoBehaviour
{
    [SerializeField] private LanguageItem itemPrefab;
    [SerializeField] private Transform content;

    private void OnEnable()
    {
        Build();
    }

    private void Build()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);

        foreach (Language language in LocalizationManager.Instance.GetLanguages())
        {
            LanguageItem item = Instantiate(itemPrefab, content);
            item.Initialize(language);
        }
    }
}