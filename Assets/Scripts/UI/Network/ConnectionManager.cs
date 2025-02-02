using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode.Transports.UTP;


public class ConnectionManager : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField addressField;
    [SerializeField]
    private TMP_InputField portField;
    [SerializeField]
    private Button hostButton;
    [SerializeField]
    private Button joinButton;
    [SerializeField]
    private TMP_InputField playerNameField;
    private ushort Port => ushort.Parse(portField.text);
    private string Address => addressField.text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        
    
        hostButton.onClick.AddListener(OnHostClicked);
        joinButton.onClick.AddListener(OnJoinClicked);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnHostClicked()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(Address, Port);
        MultiplayerSessionManager.Instance.SetPlayerName(playerNameField.text);
        MultiplayerSessionManager.Instance.StartHost();
        SceneLoader.LoadNetwork(SceneLoader.Scene.Lobby);
    }
    private void OnJoinClicked()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(Address, Port);
        MultiplayerSessionManager.Instance.SetPlayerName(playerNameField.text);
        MultiplayerSessionManager.Instance.StartClient();
    }
}
