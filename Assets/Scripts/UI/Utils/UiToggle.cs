using UnityEngine;
using UnityEngine.UI;

namespace UI.Utils
{
    public class UiToggle : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private RectTransform enabledVisual;
        [SerializeField] private RectTransform disabledVisual;

        private void Awake()
        {
            toggle.onValueChanged.AddListener(Animate);

            Animate(toggle.isOn);
        }

        private void Animate(bool value)
        {
            disabledVisual.gameObject.SetActive(!value);
            enabledVisual.gameObject.SetActive(value);
        }

        private void OnValidate()
        {
            Animate(toggle.isOn);
        }
    }
}