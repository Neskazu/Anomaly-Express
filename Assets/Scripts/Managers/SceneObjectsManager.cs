using UnityEngine;

namespace Managers
{
    public class SceneObjectsManager : MonoBehaviour
    {
        public static SceneObjectsManager Instance;

        public GameObject Rail => rail;
        public AudioSource WheelsAudio => wheelsAudioSource;

        [Header("Objects")]
        [SerializeField] private GameObject rail;

        [Header("Audio")]
        [SerializeField] private AudioSource wheelsAudioSource;

        private void Awake()
        {
            Instance = this;
        }
    }
}