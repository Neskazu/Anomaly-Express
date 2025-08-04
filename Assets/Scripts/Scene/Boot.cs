using UnityEngine;

namespace Scene
{
    public class Boot : MonoBehaviour
    {
        [SerializeField] private SceneTransitionSequence sequence;
        [SerializeField] private uint targetFrameRate = 60;

        private async void Start()
        {
            Application.targetFrameRate = (int) targetFrameRate;

            await SceneTransitionController.Instance.Play(sequence);
        }
    }
}