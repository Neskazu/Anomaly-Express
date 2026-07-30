using UnityEngine;

namespace Player
{
    [CreateAssetMenu(fileName = "Character Data", menuName = "Data/Player/Character", order = 0)]
    public class CharacterData : ScriptableObject
    {
        [SerializeField] private GameObject character;
        [SerializeField] private GameObject chibi;
        [SerializeField] private GameObject hands;
        [Space]
        [SerializeField] private Color color;
        [SerializeField] private Gradient gradient;

        public GameObject Character => character;
        public GameObject Chibi => chibi;
        public GameObject Hands => hands;
        public Color Color => color;
        public Gradient Gradient => gradient;
    }
}