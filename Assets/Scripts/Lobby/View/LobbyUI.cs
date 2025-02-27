using Managers;
using Network;
using Scene;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.View
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button readyButton;
        [SerializeField] private Button startButton;

        [SerializeField] private PlayerUI[] playersView;

        [SerializeField] private LobbyController controller;

        private static PlayerDataProvider Players =>
            MultiplayerManager.Instance.Players;

        private bool isReady;

        private void Awake()
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            readyButton.onClick.AddListener(OnReadyClicked);

            if (NetworkManager.Singleton.IsHost)
            {
                startButton.gameObject.SetActive(true);
                startButton.onClick.AddListener(OnStartClicked);
            }

            Players.OnAdd += Redraw;
            Players.OnChange += Redraw;
            Players.OnRemove += Redraw;
        }

        private void Start()
        {
            Redraw(default);
        }

        private void OnDestroy()
        {
            Players.OnAdd -= Redraw;
            Players.OnChange -= Redraw;
            Players.OnRemove -= Redraw;
        }

        private void Redraw(PlayerData obj)
        {
            for (var i = 0; i < Players.Count; i++)
            {
                playersView[i].gameObject.SetActive(true);
                playersView[i].UpdateInfo(Players[i]);
            }

            for (var i = Players.Count; i < playersView.Length; i++)
            {
                playersView[i].gameObject.SetActive(false);
            }
        }

        private void OnMainMenuClicked()
        {
            MultiplayerManager.Instance.Disconnect();
            SceneLoader.Load(SceneLoader.Scene.Menu);
        }

        private void OnReadyClicked()
        {
            isReady = !isReady;
            controller.SetReadyServerRpc(isReady);
        }

        private void OnStartClicked()
        {
            controller.StartGame();
        }
    }
}