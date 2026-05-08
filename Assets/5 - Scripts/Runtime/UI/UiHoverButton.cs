using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UiHoverButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private Image fill;

    [SerializeField] private Color normalColor;
    [SerializeField] private Color hoverColor;
    private void Awake()
    {
        fill.color = normalColor;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        fill.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        fill.color = normalColor;
    }
}