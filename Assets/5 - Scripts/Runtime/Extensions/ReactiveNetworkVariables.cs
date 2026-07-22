using R3;
using Unity.Netcode;

namespace Nac.Extensions
{
    public static class ReactiveNetworkVariableExtensions
    {
        public static Observable<T> AsObservable<T>(this NetworkVariable<T> netVar)
        {
            return Observable.Create<T>(observer =>
            {
                observer.OnNext(netVar.Value);

                NetworkVariable<T>.OnValueChangedDelegate handler = (prev, current) => { observer.OnNext(current); };

                netVar.OnValueChanged += handler;

                return Disposable.Create(() => netVar.OnValueChanged -= handler);
            });
        }
    }
}