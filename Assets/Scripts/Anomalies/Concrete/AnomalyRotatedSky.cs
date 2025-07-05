using Managers;
using UnityEngine;

namespace Anomalies.Concrete
{
    public class AnomalyRotatedSky : AnomalyBase
    {
        [SerializeField] private Material normal;
        [SerializeField] private Material rotated;
        [SerializeField] private Transform pivot;

        private GameObject _rails;

        protected override void OnActivate()
        {
            RenderSettings.skybox = rotated;

            _rails = SceneObjectsManager.Instance.Rail;
            _rails.transform.SetParent(pivot);
            pivot.rotation = Quaternion.Euler(0, 0, 90);
        }

        protected override void OnDeactivate()
        {
            RenderSettings.skybox = normal;

            pivot.rotation = Quaternion.Euler(0, 0, 0);
            _rails.transform.SetParent(null);
        }
    }
}