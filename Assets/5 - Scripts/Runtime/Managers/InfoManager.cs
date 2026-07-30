using Nac.Singleton;
using Player;
using UnityEngine;

namespace Managers
{
    public class InfoManager : Service<InfoManager>
    {
        [SerializeField] private CharacterData[] characters;

        public CharacterData GetCharacter(int characterId)
        {
            return characters[characterId];
        }

        public CharacterData GetCharacter(ulong clientId)
        {
            var data = MultiplayerManager.Players.Get(clientId);

            return characters[data.CharacterId];
        }
    }
}