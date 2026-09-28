using System;
using Cysharp.Threading.Tasks;
using R3;

namespace Nac.Extensions
{
    public static class ObservablesExtensions
    {
        public static IDisposable Subscribe<T>(this Observable<T> source, Action onNext)
        {
            return source.Subscribe(_ => onNext());
        }

        public static IDisposable Subscribe<T>(this Observable<T> source, Func<UniTask> onNext)
        {
            return source.Subscribe(_ => onNext().Forget());
        }
    }
}