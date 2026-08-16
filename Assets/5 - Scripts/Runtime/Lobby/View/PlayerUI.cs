using Network;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.View
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerName;
        [SerializeField] private Image readyImage;
        [SerializeField] private Sprite readySprite;
        [SerializeField] private Sprite notReadySprite;
        [SerializeField] private CharacterData characterData;

        public ulong CharacterId => characterData.Id;

        public void UpdateInfo(PlayerData playerData)
        {
            playerName.text = playerData.PlayerName.ToString();
            readyImage.sprite = playerData.IsReady ? readySprite : notReadySprite;
        }
    }
}