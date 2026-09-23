using Cysharp.Threading.Tasks;
using Nac.Network;
using Scene;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UiSingleplayerMenu : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button button;

        [Header("Settings")]
        [SerializeField] private SceneTransitionSequence toGame;

        private bool hosting;

        private void Start()
        {
            button.onClick.AddListener(LaunchGame);
        }

        private async void LaunchGame()
        {
            if (hosting)
            {
                return;
            }

            var nm = NetworkManager.Singleton;
#if UNITY_WEBGL && !UNITY_EDITOR
            // In WebGL, UnityTransport cannot run as a host/server without Unity Relay.
            // Switch to LocalLoopbackTransport for offline singleplayer.
            var loopback = nm.GetComponent<LocalLoopbackTransport>();
            if (loopback == null)
            {
                loopback = nm.gameObject.AddComponent<LocalLoopbackTransport>();
            }
            nm.NetworkConfig.NetworkTransport = loopback;
#else
            var transport = nm.GetComponent<UnityTransport>();
            transport.SetConnectionData("127.0.0.1", 0);
#endif

            hosting = true;
            var hosted = await NetworkController.Instance.HostAsync();

            if (hosted)
            {
                hosting = false;
                SceneTransitionManager.Instance
                    .Play(toGame)
                    .Forget();
            }
        }
    }
}