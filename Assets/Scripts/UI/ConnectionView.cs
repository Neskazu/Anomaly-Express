using Managers;
using Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ConnectionView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField addressField;
        [SerializeField] private TMP_InputField portField;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private TMP_InputField playerNameField;

        private ushort Port => ushort.Parse(portField.text);
        private string Address => addressField.text;

        private void Start()
        {
            hostButton.onClick.AddListener(OnHostClicked);
            joinButton.onClick.AddListener(OnJoinClicked);
        }

        private void OnHostClicked()
        {
            var settings = new MultiplayerManager.Settings
            {
                Name = playerNameField.text,
                Address = Address,
                Port = Port
            };

            MultiplayerManager.Instance.Host(settings);

            SceneLoader.LoadNetwork(SceneLoader.Scene.Lobby);
        }

        private void OnJoinClicked()
        {
            var settings = new MultiplayerManager.Settings
            {
                Name = playerNameField.text,
                Address = Address,
                Port = Port
            };

            MultiplayerManager.Instance.Connect(settings);
        }
    }
}