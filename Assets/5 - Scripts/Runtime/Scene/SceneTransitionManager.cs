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

        private readonly Subject<SceneTransitionSequence> preLoading = new();
        private readonly Subject<SceneTransitionSequence> postLoading = new();

        public Observable<SceneTransitionSequence> PreLoading => preLoading;
        public Observable<SceneTransitionSequence> PostLoading => postLoading;

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

            preLoading.OnNext(sequence);

            foreach (var sceneTransitionStep in sequence.steps)
            {
                if (sceneTransitionStep.networkMode == SceneTransitionSequence.NetworkMode.Solo)
                {
                    await SceneManager.LoadSceneAsync(sceneTransitionStep.scene.Path, sceneTransitionStep.loadMode).ToUniTask();
                }
                else if (NetworkManager.Singleton.IsServer)
                {
                    var completionSource = new UniTaskCompletionSource();

                    void OnSceneEvent(SceneEvent sceneEvent)
                    {
                        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete &&
                            sceneEvent.ClientId == NetworkManager.Singleton.LocalClientId)
                        {
                            NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
                            completionSource.TrySetResult();
                        }
                    }

                    NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
                    NetworkManager.Singleton.SceneManager.LoadScene(sceneTransitionStep.scene.Path, sceneTransitionStep.loadMode);

                    await completionSource.Task;
                }
            }

            if (showLoadingScreen)
            {
                await SceneTransitionWindow.Instance.Hide();
            }

            postLoading.OnNext(sequence);
        }

        private void ReturnToMainMenu(bool _)
        {
            Play(toMainMenu)
                .Forget();
        }
    }
}