using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    [SerializeField] private RectTransform cursorRect;

    private void Start()
    {
        // Скрываем системный курсор
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        cursorRect.position = Input.mousePosition;
    }
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.visible = false;
        }
    }
}
