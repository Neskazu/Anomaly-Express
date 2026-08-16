using Cysharp.Threading.Tasks;
using Nac.Network;
using Scene;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UiMultiplayerMenu : MonoBehaviour
    {
        [SerializeField] private SceneTransitionSequence toLobby;
        [Space]
        [SerializeField] private TMP_InputField playerNameField;
        [SerializeField] private TMP_InputField addressField;
        [SerializeField] private TMP_InputField portField;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;

        private bool busy;

        private ushort Port => ushort.Parse(portField.text);

        private string Address => addressField.text;

        private void Start()
        {
            hostButton.onClick.AddListener(OnHostClicked);
            joinButton.onClick.AddListener(OnJoinClicked);
        }

        private async void OnHostClicked()
        {
            if (busy)
            {
                return;
            }

            busy = true;

            NetworkManager.Singleton
                .GetComponent<UnityTransport>()
                .SetConnectionData(Address, Port);

            var hosted = await NetworkController.Instance.HostAsync();
            if (hosted)
            {
                SceneTransitionManager.Instance
                    .Play(toLobby)
                    .Forget();
            }

            busy = false;
        }

        private async void OnJoinClicked()
        {
            if (busy)
            {
                return;
            }

            busy = true;

            NetworkManager.Singleton
                .GetComponent<UnityTransport>()
                .SetConnectionData(Address, Port);

            var connected = await NetworkController.Instance.ConnectAsync();
            if (connected)
            {
                SceneTransitionManager.Instance
                    .Play(toLobby)
                    .Forget();
            }

            busy = false;
        }
    }
}