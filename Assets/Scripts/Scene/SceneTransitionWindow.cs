using Cysharp.Threading.Tasks;
using Tween.Base;
using UI.Base;
using UnityEngine;

namespace Scene
{
    public class SceneTransitionWindow : MonoBehaviour, IView
    {
        [SerializeField] private MonoTweenSequence tweenSequence;

        private static SceneTransitionWindow _instance;
        private static SceneTransitionController _controller;

        private void Awake()
        {
            if (_instance)
                Destroy(_instance);

            DontDestroyOnLoad(gameObject);

            _controller = new SceneTransitionController(this);
            _instance = this;
        }

        public UniTask Show()
        {
            return tweenSequence.Play();
        }

        public UniTask Hide()
        {
            return tweenSequence.Play(true);
        }
    }
}