
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text playerName;
    [SerializeField]
    private Image readyImage;
    [SerializeField] private Sprite readySprite;    
    [SerializeField] private Sprite notReadySprite;

    public void UpdateInfo(PlayerData playerData)
    {
        playerName.text = playerData.PlayerName.ToString();
        readyImage.sprite = playerData.IsReady ? readySprite : notReadySprite;
    }
}
