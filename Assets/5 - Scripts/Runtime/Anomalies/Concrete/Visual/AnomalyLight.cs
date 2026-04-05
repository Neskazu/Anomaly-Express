using System;
using R3;
using UnityEngine;

namespace Anomalies.Concrete.Visual
{
    public class AnomalyLight : AnomalyBase
    {
        [Header("References")]
        [SerializeField] private GameObject[] lamps;

        [Header("Settings")]
        [SerializeField] private float minTimeBetweenFlash = 0.2f;
        [SerializeField] private float maxTimeBetweenFlash = 0.5f;

        private readonly CompositeDisposable _lampsDisposable = new();

        protected override void OnActivate()
        {
            foreach (var lamp in lamps)
            {
                Observable.Interval(
                        TimeSpan.FromSeconds(UnityEngine.Random.Range(minTimeBetweenFlash, maxTimeBetweenFlash)))
                    .Subscribe(_ => lamp.SetActive(!lamp.activeInHierarchy))
                    .AddTo(_lampsDisposable);
            }
        }

        protected override void OnDeactivate()
        {
            _lampsDisposable.Dispose();
        }
    }
}