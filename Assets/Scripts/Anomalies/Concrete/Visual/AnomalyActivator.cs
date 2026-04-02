using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Anomalies.Concrete.Visual
{
    public class AnomalyActivator : AnomalyBase
    {
        [Header("Parents")]
        [SerializeField] private Transform casesParent;
        [SerializeField] private Transform bagsParent;

        [Header("Timing")]
        [SerializeField] private float startDelay = 40.0f;
        [SerializeField] private float spawnDelay = 10.0f;
        [SerializeField] private float minDelay = 0.45f;
        [SerializeField] private float acceleration = 0.95f;

        private List<GameObject> _objects = new();
        private Sequence _sequence;

        protected override void OnActivate()
        {
            _sequence?.Kill();

            CollectObjects();
            DisableAll();

            _sequence = DOTween.Sequence();

            float currentDelay = spawnDelay;

            _sequence.AppendInterval(startDelay);

            foreach (var obj in _objects)
            {
                GameObject captured = obj;

                _sequence.AppendCallback(() =>
                {
                    captured.SetActive(true);

                    //var t = capturedObj.transform;
                    //t.localScale = Vector3.zero;
                    //t.DOScale(1f, scaleDuration)
                    // .SetEase(Ease.OutBack);
                });
                _sequence.AppendInterval(currentDelay);

                currentDelay *= acceleration;
                if (currentDelay < minDelay)
                    currentDelay = minDelay;
            }
        }

        protected override void OnDeactivate()
        {
            _sequence?.Kill();
        }

        private void CollectObjects()
        {
            _objects.Clear();

            foreach (Transform child in casesParent)
                _objects.Add(child.gameObject);

            foreach (Transform child in bagsParent)
                _objects.Add(child.gameObject);
        }

        private void DisableAll()
        {
            foreach (var obj in _objects)
                obj.SetActive(false);
        }
    }
}