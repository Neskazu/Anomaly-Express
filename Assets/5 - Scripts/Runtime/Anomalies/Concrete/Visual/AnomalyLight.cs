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

        private CompositeDisposable _lampsDisposable;

        protected override void OnActivate()
        {
            // На всякий случай очищаем предыдущие подписки.
            _lampsDisposable?.Dispose();

            // Создаём новый контейнер для текущей активации.
            _lampsDisposable = new CompositeDisposable();

            foreach (var lamp in lamps)
            {
                Observable.Interval(
                        TimeSpan.FromSeconds(
                            UnityEngine.Random.Range(
                                minTimeBetweenFlash,
                                maxTimeBetweenFlash)))
                    .Subscribe(_ => lamp.SetActive(!lamp.activeSelf))
                    .AddTo(_lampsDisposable);
            }
        }

        protected override void OnDeactivate()
        {
            _lampsDisposable?.Dispose();
            _lampsDisposable = null;
        }
    }
}