using UnityEngine;

namespace Scene
{
    public class Boot : MonoBehaviour
    {
        [SerializeField] private SceneTransitionSequence sequence;

        private async void Start()
        {
            await SceneTransitionController.Instance.Play(sequence);
        }
    }
}