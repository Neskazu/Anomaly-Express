using Nac;
using Nac.Network;
using Network;
using R3;
using Scene;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.View
{
    public class LobbyVisual : MonoBehaviour
    {
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button readyButton;
        [SerializeField] private Button startButton;

        [SerializeField] private PlayerView[] playersView;

        [SerializeField] private LobbyController controller;
        [SerializeField] private SceneTransitionSequence toMenu;

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
        }

        private void Start()
        {
            foreach (var player in PlayersManager.Instance.Players)
            {
                Redraw(player);
            }

            PlayersManager.Instance.OnPlayerUpdated.Subscribe(Redraw).AddTo(this);
            PlayersManager.Instance.OnPlayerConnected.Subscribe(Redraw).AddTo(this);
            PlayersManager.Instance.OnPlayerDisconnected.Subscribe(Redraw).AddTo(this);
        }

        private void Redraw(PlayerData obj)
        {
            foreach (var view in playersView)
            {
                view.gameObject.SetActive(false);
            }

            foreach (var player in PlayersManager.Instance.Players)
            {
                foreach (var view in playersView)
                {
                    if (view.CharacterId != player.CharacterId)
                    {
                        continue;
                    }

                    view.gameObject.SetActive(true);
                    view.UpdateInfo(player);
                }
            }
        }

        private void OnMainMenuClicked()
        {
            NetworkController.Instance.Disconnect();
        }

        private void OnReadyClicked()
        {
            isReady = !isReady;

            controller.SetReady(isReady);
        }

        private void OnStartClicked()
        {
            controller.StartGame();
        }
    }
}