using Cysharp.Threading.Tasks;
using DG.Tweening;
using Tween.Base;
using UI.Base;
using UnityEngine;

namespace Scene
{
    public class SceneTransitionWindow : MonoBehaviour, IWindow
    {
        [SerializeField] private MonoTweenSequence tweenSequence;

        private Sequence _sequence;
        private UniTask _task;
        private bool _active;

        public static SceneTransitionWindow Instance { get; private set; }

        private void Awake()
        {
            if (Instance)
            {
                Destroy(Instance);
            }

            DontDestroyOnLoad(gameObject);

            Instance = this;

            _ = new SceneTransitionController(this);
        }

        public UniTask Show()
        {
            if (_active)
            {
                return UniTask.CompletedTask;
            }

            if (_sequence.IsActive())
            {
                return _task;
            }

            _sequence = tweenSequence.Play();
            _task = _sequence.ToUniTask();

            _sequence.AppendCallback(delegate { _active = true; });

            return _task;
        }

        public UniTask Hide()
        {
            if (!_active)
            {
                return UniTask.CompletedTask;
            }

            if (_sequence.IsActive())
            {
                return _task;
            }

            _sequence = tweenSequence.Play(true);
            _task = _sequence.ToUniTask();

            _sequence.AppendCallback(delegate { _active = false; });

            return _task;
        }
    }
}