using UnityEngine;

namespace Managers
{
    public class SceneObjectsManager : MonoBehaviour
    {
        [SerializeField] private GameObject rail;

        public static SceneObjectsManager Instance;
        public GameObject Rail => rail;

        private void Awake()
        {
            Instance = this;
        }
    }
}