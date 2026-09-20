using Localization;
using SaveSystem;
using UnityEngine;

namespace Scene
{
    public class Boot : MonoBehaviour
    {
        [SerializeField] private SceneTransitionSequence sequence;
        [SerializeField] private uint targetFrameRate = 60;
        [SerializeField] private GameObject LoadingText;

        private async void Start()
        {
            if (LoadingText != null) LoadingText.SetActive(false);

            SaveManager.Load();

            await LocalizationManager.Instance.InitializeAsync();
            if (LoadingText != null) LoadingText.SetActive(true);

            Application.targetFrameRate = (int)targetFrameRate;

            await SceneTransitionManager.Instance.Play(sequence);
        }
    }
}