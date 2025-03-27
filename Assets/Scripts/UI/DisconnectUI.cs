using Scene;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DisconnectUI : MonoBehaviour
    {
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private SceneTransitionSequence toMenu;

        private void Awake()
        {
            mainMenuButton.onClick.AddListener(() => { SceneTransitionController.Instance.Play(toMenu); });
        }

        private void Start()
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

            Hide();
        }

        private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
        {
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
}