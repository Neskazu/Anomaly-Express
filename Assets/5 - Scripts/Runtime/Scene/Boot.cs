using Localization;
using SaveSystem;
using UnityEngine;

namespace Scene
{
    public class Boot : MonoBehaviour
    {
        [SerializeField] private SceneTransitionSequence sequence;
        [SerializeField] private uint targetFrameRate = 60;

        private async void Start()
        {
            SaveManager.Load();

            LocalizationManager.Instance.Initialize();

            Application.targetFrameRate = (int)targetFrameRate;

            await SceneTransitionManager.Instance.Play(sequence);
        }
    }
}