using System.Collections.Generic;
using UnityEngine;

namespace Anomalies
{
    public class AnomalyProportions : AnomalyBase
    {
        [SerializeField] private List<Transform> rigs = new();

        [SerializeField] private float minScale = 0.9f;
        [SerializeField] private float maxScale = 1.1f;

        protected override void OnActivate()
        {
            foreach (var rig in rigs)
            {
                if (rig != null)
                    Apply(rig);
            }
        }

        protected override void OnDeactivate()
        {
            foreach (var rig in rigs)
            {
                if (rig != null)
                    Revert(rig);
            }
        }

        private void Apply(Transform parent)
        {
            foreach (Transform child in parent)
            {
                child.localScale = new Vector3(
                    Random.Range(minScale, maxScale),
                    Random.Range(minScale, maxScale),
                    Random.Range(minScale, maxScale));

                Apply(child);
            }
        }

        private void Revert(Transform parent)
        {
            foreach (Transform child in parent)
            {
                child.localScale = Vector3.one;
                Revert(child);
            }
        }
    }
}