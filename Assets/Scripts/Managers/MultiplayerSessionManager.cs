using System;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class MultiplayerSessionManager : NetworkBehaviour
{
    public const int MaxPlayerAmount = 2;
    public static MultiplayerSessionManager Instance { get; private set; }

    public event EventHandler OnTryingToJoinGame;
    [SerializeField]
    private NetworkList<PlayerData> playerDataNetworkList;
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        playerDataNetworkList = new NetworkList<PlayerData>();
    }
    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];
            if (playerData.clientId == clientId)
            {
                playerDataNetworkList.RemoveAt(i);
                Debug.Log("Player Disconnected: " + clientId);
            }
        }
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData { clientId = clientId });
        Debug.Log("player Connected: " + clientId);
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        Debug.Log("approve count:" + NetworkManager.Singleton.ConnectedClientsIds.Count + "  max: " + MaxPlayerAmount);
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MaxPlayerAmount)
        {
            response.Approved = false;
            response.Reason = "Game is Full";
            Debug.Log(response.Reason);
            response.Pending = false;
            return;
        }
        response.Approved = true;
        response.Pending = false;
    }
    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnClientConnectedCallback(ulong obj)
    {
        Debug.Log("Client connect");
    }

    private void NetworkManager_Client_OnClientDisconnectCallback(ulong obj)
    {
        Debug.Log("Client disconnect: " + NetworkManager.Singleton.DisconnectReason);
    }
}
