using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class UiPressedButton : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [Header("Targets")]
    [SerializeField] private RectTransform[] targets;

    [Header("Settings")]
    [SerializeField] private float pressedScale = 0.96f;
    [SerializeField] private float duration = 0.08f;

    private Vector3[] _defaultScales;

    private void Awake()
    {
        // Если массив пустой, ничего не делаем, чтобы избежать ошибок
        if (targets == null || targets.Length == 0) return;

        // Запоминаем исходный масштаб только для тех элементов, которые ты вручную добавил в инспекторе
        _defaultScales = new Vector3[targets.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                _defaultScales[i] = targets[i].localScale;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (targets == null) return;

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null) continue;

            targets[i].DOKill();
            targets[i].DOScale(_defaultScales[i] * pressedScale, duration);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetScale();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetScale();
    }

    private void ResetScale()
    {
        if (targets == null) return;

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null) continue;

            targets[i].DOKill();
            targets[i].DOScale(_defaultScales[i], duration);
        }
    }
}