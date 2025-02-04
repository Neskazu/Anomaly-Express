
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
        playerName.text = playerData.playerName.ToString();
        readyImage.sprite = playerData.isReady ? readySprite : notReadySprite;
    }
}
