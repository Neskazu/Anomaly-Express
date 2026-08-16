using Cysharp.Threading.Tasks;
using Nac.Network;
using Nac.Singleton;
using R3;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scene
{
    public class SceneTransitionManager : Service<SceneTransitionManager>
    {
        [SerializeField] private SceneTransitionSequence toMainMenu;

        private readonly Subject<SceneTransitionSequence> loaded = new();

        public Observable<SceneTransitionSequence> Loaded => loaded;

        private void Start()
        {
            NetworkController.Instance.Online
                .Skip(1)
                .Where(online => !online)
                .Subscribe(ReturnToMainMenu)
                .AddTo(this);
        }

        public async UniTask Play(SceneTransitionSequence sequence, bool showLoadingScreen = true)
        {
            if (showLoadingScreen)
            {
                await SceneTransitionWindow.Instance.Show();
            }

            foreach (var sceneTransitionStep in sequence.steps)
            {
                if (sceneTransitionStep.networkMode == SceneTransitionSequence.NetworkMode.Solo)
                {
                    await SceneManager.LoadSceneAsync(sceneTransitionStep.scene.Path, sceneTransitionStep.loadMode).ToUniTask();
                }
                else
                {
                    NetworkManager.Singleton.SceneManager.LoadScene(sceneTransitionStep.scene.Path, sceneTransitionStep.loadMode);
                }
            }

            if (showLoadingScreen)
            {
                await SceneTransitionWindow.Instance.Hide();
            }

            loaded.OnNext(sequence);
        }

        private void ReturnToMainMenu(bool _)
        {
            Play(toMainMenu)
                .Forget();
        }
    }
}