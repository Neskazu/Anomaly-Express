using UnityEngine;

namespace Anomalies
{
    public class SightSensorComponent : SensorComponent
    {
        [SerializeField] private Renderer sightRenderer;
        [SerializeField] private Color color;

        private Vector3 rgb;

        private void Start()
        {
            rgb = new Vector3(color.r, color.g, color.b);
        }

        private void Update()
        {
            foreach (var lens in LensComponent.Active)
            {
                if (!Similar(lens.RGB, rgb) || lens.Planes == null)
                {
                    continue;
                }

                if (!GeometryUtility.TestPlanesAABB(lens.Planes, sightRenderer.bounds))
                {
                    continue;
                }

                detected.Value = true;
                return;
            }

            detected.Value = false;
        }

        private static bool Similar(Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b) < 0.1f;
        }
    }
}