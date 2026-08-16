using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

namespace Nac.Extensions
{
    public readonly struct ReadOnlyNetworkList<T> : IReadOnlyList<T> where T : unmanaged, System.IEquatable<T>
    {
        private readonly NetworkList<T> list;

        public ReadOnlyNetworkList(NetworkList<T> list) => this.list = list;

        public T this[int index] => list[index];
        public int Count => list.Count;

        public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();
    }
}