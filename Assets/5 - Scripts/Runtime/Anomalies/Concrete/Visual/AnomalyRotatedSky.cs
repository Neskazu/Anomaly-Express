using Managers;
using UnityEngine;

namespace Anomalies.Concrete.Visual
{
    public class AnomalyRotatedSky : AnomalyBase
    {
        [SerializeField] private Material normal;
        [SerializeField] private Material rotated;
        [SerializeField] private Transform pivot;

        private GameObject _world;

        protected override void OnActivate()
        {
            RenderSettings.skybox = rotated;

            _world = SceneObjectsManager.Instance.World;
            _world.transform.SetParent(pivot);
            pivot.rotation = Quaternion.Euler(0, 0, 90);
        }

        protected override void OnDeactivate()
        {
            RenderSettings.skybox = normal;

            pivot.rotation = Quaternion.Euler(0, 0, 0);
            _world.transform.SetParent(null);
        }
    }
}