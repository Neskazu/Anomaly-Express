using UnityEngine;

namespace Managers
{
    public class SceneObjectsManager : MonoBehaviour
    {
        public static SceneObjectsManager Instance;

        public GameObject World => world;
        public AudioSource WheelsAudio => wheelsAudioSource;

        [Header("Objects")]
        [SerializeField] private GameObject world;

        [Header("Audio")]
        [SerializeField] private AudioSource wheelsAudioSource;

        private void Awake()
        {
            Instance = this;
        }
    }
}