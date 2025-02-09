using Managers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button readyButton;
        [SerializeField] private Button startButton;

        private void Awake()
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            readyButton.onClick.AddListener(OnReadyClicked);
            startButton.onClick.AddListener(OnStartClicked);
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
            MultiplayerSessionManager.Instance.StartGame();
        }
    }
}