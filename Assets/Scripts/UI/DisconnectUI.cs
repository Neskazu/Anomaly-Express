using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DisconnectUI : MonoBehaviour
{

    [SerializeField]
    private Button mainMenuButton;
    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() => {
            SceneLoader.Load(SceneLoader.Scene.Menu);
        });
    }
    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        Hide();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log("client id + is server id: " + clientId + " " + NetworkManager.ServerClientId);
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Show();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
    }
}
