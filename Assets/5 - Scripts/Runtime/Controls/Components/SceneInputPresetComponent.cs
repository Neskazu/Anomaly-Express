using UnityEngine;

namespace Controls
{
    public class SceneInputPresetComponent : MonoBehaviour
    {
        [SerializeField] private InputPreset preset;

        private void Awake()
        {
            InputManager.Instance.ActivatePreset(preset);
        }
    }
}