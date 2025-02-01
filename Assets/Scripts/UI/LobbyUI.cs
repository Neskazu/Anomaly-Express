using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField]
    private Button mainMenuButton;
    [SerializeField]
    private Button readyButton;
    [SerializeField]
    private Button startButton;
    private void Awake()
    {
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        readyButton.onClick.AddListener(OnReadyClicked);
        startButton.onClick.AddListener(OnStartClicked);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnMainMenuClicked()
    {
        NetworkManager.Singleton.Shutdown();
        SceneLoader.Load(SceneLoader.Scene.Menu);
    }
    private void OnReadyClicked()
    {
        // Set the player ready
    }
    private void OnStartClicked()
    {
        SceneLoader.LoadNetwork(SceneLoader.Scene.Game);
        // Start the game
    }
}
