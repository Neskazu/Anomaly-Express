using UnityEngine;

namespace Nac.Singleton
{
    public class Singleton<T> : MonoBehaviour
        where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        private void Awake()
        {
            if (Instance)
            {
                Debug.LogError($"[{name}] Instance already exists!");
                Destroy(this);
            }

            Instance = this as T;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}