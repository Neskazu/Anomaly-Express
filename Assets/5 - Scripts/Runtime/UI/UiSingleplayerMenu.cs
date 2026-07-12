using System.Linq;
using Cysharp.Threading.Tasks;
using Managers;
using Network.Players;
using Scene;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UiSingleplayerMenu : MonoBehaviour
    {
        private static PlayerDataProvider Players => MultiplayerManager.Players;

        [Header("References")]
        [SerializeField] private Button button;

        [Header("Settings")]
        [SerializeField] private SceneTransitionSequence toGame;

        private void Start()
        {
            button.onClick.AddListener(LaunchGame);
        }

        private void LaunchGame()
        {
            // TODO: Заменить "player" на имя
            var settings = new MultiplayerManager.Settings
            {
                Name = "Player",
                Address = "127.0.0.1",
                Port = 0
            };

            MultiplayerManager.Instance.Host(settings);

            var player = Players.ToList().First();

            player.CharacterId = Random.Range(0, 4);

            Players.Update(player);

            SceneTransitionController.Instance
                .Play(toGame)
                .Forget();
        }
    }
}