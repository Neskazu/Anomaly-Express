using System;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] playerUI;
    [SerializeField]
    private PlayerInfoUI[] playerInfoUI;
    private int playerCount;
    private void Start()
    {
        MultiplayerSessionManager.Instance.OnPlayerDataNetworkListChanged += MultiplayerSessionManager_OnPlayerDataNetworkListChanged;
        Debug.Log("listChanged");
        SetPlayer();
    }

    private void MultiplayerSessionManager_OnPlayerDataNetworkListChanged(object sender, EventArgs e)
    {
        SetPlayer();
    }
    private void SetPlayer()
    {
        Show();
    }
    private void Show()
    {
        UpdatePlayerCount();
        for (int i = 0; i < playerUI.Length; i++)
        {
            bool isActive = i < playerCount;
            if (isActive)
            {
                PlayerData playerData = MultiplayerSessionManager.Instance.GetPlayerDataFromPlayerIndex(i);
                playerInfoUI[i].UpdateInfo(playerData);
            }
            playerUI[i].SetActive(isActive);
        }
    }

    private void UpdatePlayerCount()
    {
        playerCount = MultiplayerSessionManager.Instance.GetConnectedPlayersCount();
    }
}