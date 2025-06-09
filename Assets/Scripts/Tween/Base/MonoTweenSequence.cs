using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Tween.Base
{
    public class MonoTweenSequence : MonoBehaviour
    {
        [SerializeField] private MonoTween[] tweens;

        private CancellationToken _token;

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();
        }

        public async UniTask Play(bool reverse = false)
        {
            UniTask[] tweenTasks = new UniTask[tweens.Length];

            for (int i = 0; i < tweens.Length; i++)
                tweenTasks[i] = tweens[i].Play(reverse);

            await UniTask.WhenAll(tweenTasks);
        }
    }
}