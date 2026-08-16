using Cysharp.Threading.Tasks;
using DG.Tweening;
using Nac.Singleton;
using Tween.Base;
using UI.Base;
using UnityEngine;

namespace Scene
{
    public class SceneTransitionWindow : Service<SceneTransitionWindow>, IWindow
    {
        [SerializeField] private MonoTweenSequence tweenSequence;

        private Sequence _sequence;
        private UniTask _task;
        private bool _active;

        public UniTask Show()
        {
            if (_active)
                return UniTask.CompletedTask;

            if (_sequence != null && _sequence.IsPlaying())
                _sequence.Kill();

            _sequence = tweenSequence.Play();
            _sequence.AppendCallback(() => _active = true);

            return _sequence.AsyncWaitForCompletion().AsUniTask();
        }

        public UniTask Hide()
        {
            if (!_active)
                return UniTask.CompletedTask;

            if (_sequence != null && _sequence.IsPlaying())
                _sequence.Kill();

            _sequence = tweenSequence.Play(true);
            _sequence.AppendCallback(() => _active = false);

            return _sequence.AsyncWaitForCompletion().AsUniTask();
        }
    }
}