using System;
using Cysharp.Threading.Tasks;
using UI.Base;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace Scene
{
    public class SceneTransitionController
    {
        public static SceneTransitionController Instance { get; private set; }

        public event Action<SceneTransitionSequence> Loaded;

        private readonly IView _loadingScreen;

        public SceneTransitionController(IView loadingScreen)
        {
            _loadingScreen = loadingScreen;

            Instance = this;
        }

        public async UniTask Play(SceneTransitionSequence sequence)
        {
            await _loadingScreen.Show();

            foreach (var sceneTransitionStep in sequence.steps)
            {
                if (sceneTransitionStep.networkMode == SceneTransitionSequence.NetworkMode.Solo)
                    await SceneManager.LoadSceneAsync(sceneTransitionStep.scene.Path, sceneTransitionStep.loadMode).ToUniTask();
                else
                    NetworkManager.Singleton.SceneManager.LoadScene(sceneTransitionStep.scene.Path, sceneTransitionStep.loadMode);
            }

            await _loadingScreen.Hide();
            Loaded?.Invoke(sequence);
        }
    }
}