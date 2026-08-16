using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Player
{
    [CreateAssetMenu(fileName = "Character Data", menuName = "Data/Player/Character", order = 0)]
    public class CharacterData : ScriptableObject
    {
        [SerializeField] private ulong id;
        [Space]
        [SerializeField] private GameObject character;
        [SerializeField] private GameObject chibi;
        [SerializeField] private GameObject hands;
        [Space]
        [SerializeField] private Color color;
        [SerializeField] private Gradient gradient;

        public ulong Id => id;
        public GameObject Character => character;
        public GameObject Chibi => chibi;
        public GameObject Hands => hands;
        public Color Color => color;
        public Gradient Gradient => gradient;

        private void OnValidate()
        {
            if (id == 0)
            {
                var buffer = Guid.NewGuid().ToByteArray();
                id = BitConverter.ToUInt64(buffer, 0);

#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }
    }
}