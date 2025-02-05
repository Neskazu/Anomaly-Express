using System;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerSessionManager : NetworkBehaviour
{
    public const int MaxPlayerAmount = 4;
    public static MultiplayerSessionManager Instance { get; private set; }
    public EventHandler OnPlayerDataNetworkListChanged;

    public event EventHandler OnTryingToJoinGame;
    [SerializeField]
    private NetworkList<PlayerData> playerDataNetworkList;
    private string playerName;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
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
        SetPlayerNameServerRpc(GetPlayerName());
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
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
        SetPlayerNameServerRpc(GetPlayerName());
        Debug.Log("Client connect");
    }

    private void NetworkManager_Client_OnClientDisconnectCallback(ulong obj)
    {
        Debug.Log("Client disconnect: " + NetworkManager.Singleton.DisconnectReason);
    }
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < playerDataNetworkList.Count;
    }
    public int GetConnectedPlayersCount()
    {
        return playerDataNetworkList.Count;
    }
    public PlayerData GetPlayerDataFromPlayerIndex(int clientId)
    {
        return playerDataNetworkList[clientId];
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerName = playerName;

        playerDataNetworkList[playerDataIndex] = playerData;
    }
    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                return i;
            }
        }
        return -1;
    }
    public string GetPlayerName()
    {
        return playerName;
    }
    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;
    }
    public void TogglePlayerReady(ulong clientId)
    {
        int playerIndex = GetPlayerDataIndexFromClientId(clientId);
        PlayerData playerData = GetPlayerDataFromPlayerIndex(playerIndex);
        playerData.isReady = !playerData.isReady;
        playerDataNetworkList[playerIndex] = playerData;
        CheckReadyAndStart();
    }
    [ServerRpc(RequireOwnership = false)]
    public void TogglePlayerReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        TogglePlayerReady(senderClientId);
    }
    private void CheckReadyAndStart()
    {
        if (!IsServer)
        {
            return;
        }
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (!playerData.isReady)
            {
                return;
            }
        }
        StartGame();
    }
    public void StartGame()
    {
        SceneLoader.LoadNetwork(SceneLoader.Scene.Game);
    }
    public void KickPlayer(ulong clientId)
    {
        Debug.Log("Kicking player: " + clientId);
        NetworkManager.Singleton.DisconnectClient(clientId);
        NetworkManager_Client_OnClientDisconnectCallback(clientId);
    }
}
