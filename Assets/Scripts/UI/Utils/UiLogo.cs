using Cysharp.Threading.Tasks;
using Tween.Base;
using UnityEngine;

namespace UI.Utils
{
    public class UiLogo : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private MonoTweenSequence showSequence;
        [SerializeField] private MonoTweenSequence hideSequence;

        [Header("Settings")]
        [SerializeField] private float startDelay = 0.5f;
        [SerializeField] private float screenTime = 2f;

        private void Awake()
        {
            DontDestroyOnLoad(canvas.gameObject);
        }

        public async void Start()
        {
            await UniTask.WaitForSeconds(startDelay);
            await showSequence.Play();
            await UniTask.WaitForSeconds(screenTime);
            await hideSequence.Play();

            Destroy(canvas.gameObject);
        }
    }
}