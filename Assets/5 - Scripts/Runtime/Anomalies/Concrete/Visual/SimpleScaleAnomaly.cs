using System.Collections.Generic;
using UnityEngine;

namespace Anomalies.Concrete.Visual
{
    public class SimpleScaleAnomaly : AnomalyBase
    {
        [Header("Settings")]
        [SerializeField] private List<Transform> targets;
        [SerializeField] private float duration = 30f;
        [SerializeField] private float maxScale = 2f;

        private float _timer;

        protected override void OnActivate()
        {
            _timer = 0f;
        }

        protected override void OnDeactivate()
        {
        }

        protected override void OnUpdate()
        {
            if (_timer >= duration) return;

            _timer += Time.deltaTime;
            float progress = Mathf.Min(_timer / duration, 1f);
            float currentScaleValue = Mathf.Lerp(1f, maxScale, progress);
            Vector3 newScale = new Vector3(currentScaleValue, currentScaleValue, currentScaleValue);

            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] != null)
                {
                    targets[i].localScale = newScale;
                }
            }
        }
    }
}