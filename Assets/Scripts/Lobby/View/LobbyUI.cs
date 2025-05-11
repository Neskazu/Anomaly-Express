using System.Linq;
using Managers;
using Network;
using Network.Players;
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
        [SerializeField] private SceneTransitionSequence toMenu;

        private static PlayerDataProvider Players =>
            MultiplayerManager.Players;

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

            MultiplayerManager.Players.OnUpdated += Redraw;
            MultiplayerManager.Players.OnConnected += Redraw;
            MultiplayerManager.Players.OnDisconnected += Redraw;
        }

        private void Start()
        {
            foreach (PlayerData playerData in MultiplayerManager.Players)
                Redraw(playerData);
        }

        private void OnDestroy()
        {
            MultiplayerManager.Players.OnUpdated -= Redraw;
            MultiplayerManager.Players.OnConnected -= Redraw;
            MultiplayerManager.Players.OnDisconnected -= Redraw;
        }

        private void Redraw(PlayerData obj)
        {
            for (var i = 0; i < Players.Count(); i++)
            {
                playersView[i].gameObject.SetActive(true);
                playersView[i].UpdateInfo(Players[i]);
            }

            for (var i = Players.Count(); i < playersView.Length; i++)
            {
                playersView[i].gameObject.SetActive(false);
            }
        }

        private async void OnMainMenuClicked()
        {
            MultiplayerManager.Instance.Disconnect();
            await SceneTransitionController.Instance.Play(toMenu);
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