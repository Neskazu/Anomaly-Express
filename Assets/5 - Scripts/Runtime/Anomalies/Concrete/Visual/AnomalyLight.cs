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
            _lampsDisposable.Clear();

            foreach (var lamp in lamps)
            {
                if (lamp == null) continue;

                float timer = UnityEngine.Random.Range(minTimeBetweenFlash, maxTimeBetweenFlash);

                Observable.EveryUpdate()
                    .Subscribe(_ =>
                    {
                        if (lamp == null) return;

                        timer -= Time.deltaTime;
                        if (timer <= 0f)
                        {
                            lamp.SetActive(!lamp.activeSelf);
                            timer = UnityEngine.Random.Range(minTimeBetweenFlash, maxTimeBetweenFlash);
                        }
                    })
                    .AddTo(_lampsDisposable);
            }
        }

        protected override void OnDeactivate()
        {
            _lampsDisposable.Clear();

            foreach (var lamp in lamps)
            {
                if (lamp != null)
                    lamp.SetActive(true);
            }
        }

        public override void OnDestroy()
        {
            _lampsDisposable.Dispose();
            base.OnDestroy();
        }
    }
}