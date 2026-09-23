using UnityEngine;
using UnityEngine.UI;

public class MultiplayerButton : MonoBehaviour
{
    [SerializeField] private Button multiplayerButton;
    [SerializeField] private GameObject desktopOnlyText;

    private void Awake()
    {
        multiplayerButton.onClick.AddListener(OnMultiplayerClicked);
    }

    private void OnDestroy()
    {
        multiplayerButton.onClick.RemoveListener(OnMultiplayerClicked);
    }

    private void OnMultiplayerClicked()
    {
        desktopOnlyText.SetActive(!desktopOnlyText.activeSelf);
    }
}