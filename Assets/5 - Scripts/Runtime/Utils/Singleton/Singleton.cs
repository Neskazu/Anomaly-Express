using UnityEngine;

namespace Nac.Singleton
{
    public class Singleton<T> : MonoBehaviour
        where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        public static string Tag => typeof(T).Name;

        public virtual void Awake()
        {
            if (Instance)
            {
                Debug.LogError($"[{Tag}] Instance already exists!");
                Destroy(this);
            }

            Debug.Log($"[{Tag}] Instance created.");
            Instance = this as T;
        }

        public virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Debug.Log($"[{Tag}] Instance destroyed.");
                Instance = null;
            }
        }
    }
}