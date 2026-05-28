using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class UIButtonAudio : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private UISoundPlayer audioPlayer;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (audioPlayer == null)
        {
            audioPlayer = Object.FindFirstObjectByType<UISoundPlayer>();
        }
    }

    private void OnEnable()
    {
        button.onClick.AddListener(HandleClick);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(HandleClick);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            audioPlayer?.PlayHover();
        }
    }

    private void HandleClick()
    {
        audioPlayer?.PlayClick();
    }
}
