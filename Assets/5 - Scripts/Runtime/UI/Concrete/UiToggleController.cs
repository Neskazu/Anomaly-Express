using UI;
using UnityEngine;
using UnityEngine.UI;

public class UiToggleController : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private UiTransition window;

    private void Awake()
    {
        button.onClick.AddListener(window.Toggle);
    }
}