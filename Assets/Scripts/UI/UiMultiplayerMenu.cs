using Managers;
using Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UiMultiplayerMenu : MonoBehaviour
    {
        [SerializeField] private SceneTransitionSequence toLobby;

        [SerializeField] private TMP_InputField playerNameField;
        [SerializeField] private TMP_InputField addressField;
        [SerializeField] private TMP_InputField portField;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;

        private ushort Port => ushort.Parse(portField.text);
        private string Address => addressField.text;

        private void Start()
        {
            hostButton.onClick.AddListener(OnHostClicked);
            joinButton.onClick.AddListener(OnJoinClicked);
        }

        private async void OnHostClicked()
        {
            var settings = new MultiplayerManager.Settings
            {
                Name = playerNameField.text,
                Address = Address,
                Port = Port
            };

            MultiplayerManager.Instance.Host(settings);
            await SceneTransitionController.Instance.Play(toLobby);
        }

        private async void OnJoinClicked()
        {
            var settings = new MultiplayerManager.Settings
            {
                Name = playerNameField.text,
                Address = Address,
                Port = Port
            };

            MultiplayerManager.Instance.Connect(settings);
            await SceneTransitionController.Instance.Play(toLobby);
        }
    }
}