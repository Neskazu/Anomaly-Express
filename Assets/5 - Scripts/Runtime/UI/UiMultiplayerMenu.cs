using System.Linq;
using Cysharp.Threading.Tasks;
using Nac.Network;
using SaveSystem;
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
        [SerializeField] private GameObject multiplayerMenuRoot;

        private bool busy;

        private ushort Port => ushort.Parse(portField.text);

        private string Address => addressField.text;

        private UiSave Config => SaveManager.Save.Ui;

        private void Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // Multiplayer is disabled in WebGL build
            if (multiplayerMenuRoot != null)
            {
                multiplayerMenuRoot.SetActive(false);
            }
            gameObject.SetActive(false);
            return;
#endif
            hostButton.onClick.AddListener(OnHostClicked);
            joinButton.onClick.AddListener(OnJoinClicked);

            playerNameField.text = Config.NetworkMenu.Username;
            addressField.text = Config.NetworkMenu.Ip;
            portField.text = Config.NetworkMenu.Port.ToString();
        }

        private async void OnHostClicked()
        {
            if (busy)
            {
                return;
            }

            busy = true;

            Config.NetworkMenu.SetIp(addressField.text);
            Config.NetworkMenu.SetPort(portField.text);
            SaveManager.SaveGame();

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
#if UNITY_WEBGL && !UNITY_EDITOR
            transport.UseWebSockets = true;
#endif
            transport.SetConnectionData(Address, Port);

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

            Config.NetworkMenu.SetIp(addressField.text);
            Config.NetworkMenu.SetPort(portField.text);
            SaveManager.SaveGame();

            await SceneTransitionWindow.Instance.Show();

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
#if UNITY_WEBGL && !UNITY_EDITOR
            transport.UseWebSockets = true;
#endif
            transport.SetConnectionData(Address, Port);

            await NetworkController.Instance.ConnectAsync();
            await SceneTransitionWindow.Instance.Hide();

            busy = false;
        }
    }
}