using R3;
using UnityEngine;

namespace Anomalies
{
    public abstract class SensorComponent : MonoBehaviour
    {
        protected readonly ReactiveProperty<bool> detected = new();

        public ReadOnlyReactiveProperty<bool> Detected => detected;
    }
}