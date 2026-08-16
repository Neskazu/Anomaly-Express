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

            NetworkManager.Singleton
                .GetComponent<UnityTransport>()
                .SetConnectionData("127.0.0.1", 0);

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