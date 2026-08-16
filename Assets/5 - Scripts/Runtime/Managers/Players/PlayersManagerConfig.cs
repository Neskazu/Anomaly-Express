using System.Collections.Generic;
using Player;
using UnityEngine;

namespace Nac
{
    [CreateAssetMenu(fileName = "Players Manager Config", menuName = "Nac/Players Manager Config")]
    public class PlayersManagerConfig : ScriptableObject
    {
        [SerializeField] private CharacterData[] characters;

        public IReadOnlyList<CharacterData> Characters => characters;
    }
}