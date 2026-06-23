using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Anomalies.Concrete.Visual
{
    public class AnomalyMosaicCensor : AnomalyBase
    {
        [Header("Mosaic Objects")]
        [SerializeField] private List<GameObject> mosaicObjects = new();

        [Header("Timing")]
        [SerializeField] private float startDelay = 10f;
        [SerializeField] private float interval = 5f;

        private readonly List<GameObject> _availableObjects = new();
        private Sequence _sequence;

        protected override void OnActivate()
        {
            _sequence?.Kill();

            _availableObjects.Clear();
            _availableObjects.AddRange(mosaicObjects);

            DisableAll();

            _sequence = DOTween.Sequence();
            _sequence.AppendInterval(startDelay);
            _sequence.AppendCallback(ActivateRandomMosaic);
            _sequence.AppendInterval(interval);
            _sequence.SetLoops(-1);
        }

        protected override void OnDeactivate()
        {
            _sequence?.Kill();
            DisableAll();
        }

        private void ActivateRandomMosaic()
        {
            if (_availableObjects.Count == 0)
            {
                _sequence?.Kill();
                return;
            }

            int index = Random.Range(0, _availableObjects.Count);
            GameObject obj = _availableObjects[index];

            obj.SetActive(true);

            _availableObjects.RemoveAt(index);
        }

        private void DisableAll()
        {
            foreach (var obj in mosaicObjects)
                obj.SetActive(false);
        }
    }
}