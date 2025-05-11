using UnityEngine;

namespace Scene
{
    public class Boot : MonoBehaviour
    {
        [SerializeField] private SceneTransitionSequence sequence;

        private void Awake()
        {
            Application.targetFrameRate = -1;
        }

        private async void Start()
        {
            await SceneTransitionController.Instance.Play(sequence);
        }
    }
}