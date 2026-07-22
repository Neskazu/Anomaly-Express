using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controls
{
    [CreateAssetMenu(fileName = "Input Preset", menuName = "Input/Preset", order = 0)]
    public class InputPreset : ScriptableObject
    {
        [SerializeField] private InputActionMapReference[] maps;

        [Header("Overrides")]
        [SerializeField] private InputActionReference[] toEnable;
        [SerializeField] private InputActionReference[] toDisable;

        public IReadOnlyCollection<InputActionMapReference> Maps => maps;
        public IReadOnlyCollection<InputActionReference> ToEnable => toEnable;
        public IReadOnlyCollection<InputActionReference> ToDisable => toDisable;

        public IEnumerable<Guid> ToEnableIds => toEnable.Select(iar => iar.action.id);
        public IEnumerable<Guid> ToDisableIds => toDisable.Select(iar => iar.action.id);
    }
}