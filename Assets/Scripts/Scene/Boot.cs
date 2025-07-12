using UnityEngine;

namespace Scene
{
    public class Boot : MonoBehaviour
    {
        [SerializeField] private SceneTransitionSequence sequence;

        private async void Start()
        {
            Application.targetFrameRate = 60;

            await SceneTransitionController.Instance.Play(sequence);
        }
    }
}