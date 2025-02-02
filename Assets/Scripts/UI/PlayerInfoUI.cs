using TMPro;
using UnityEngine;

public class PlayerInfoUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text playerName;
    public void UpdateInfo(PlayerData playerData)
    {
        playerName.text = playerData.playerName.ToString();
    }
}
