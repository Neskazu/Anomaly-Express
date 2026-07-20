using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controls
{
    [CreateAssetMenu(fileName = "InputActionMapReference", menuName = "Input/Map Reference", order = 0)]
    public class InputActionMapReference : ScriptableObject
    {
        [SerializeField] private InputActionReference[] actions;

        public IReadOnlyCollection<InputActionReference> Actions => actions;
    }
}