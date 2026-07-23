using UnityEngine;
using UnityEngine.UI;

public class SocialLinksManager : MonoBehaviour
{
    [System.Serializable]
    public struct SocialButton
    {
        public Button button;
        [Tooltip("Для обычных сайтов: https://discord.gg/...\nДля Steam: steam://openurl/https://store.steampowered.com/app/...")]
        public string url;
    }

    [Header("Все кнопки соцсетей")]
    [SerializeField] private SocialButton[] socialButtons;

    private void Start()
    {
        foreach (var social in socialButtons)
        {
            if (social.button == null) continue;

            string targetUrl = social.url;
            social.button.onClick.AddListener(() => OpenURL(targetUrl));
        }
    }

    public void OpenURL(string url)
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            Application.OpenURL(url);
        }
    }
}