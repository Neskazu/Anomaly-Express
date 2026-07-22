using UnityEngine;

namespace Anomalies
{
    public class SightSensorComponent : SensorComponent
    {
        [SerializeField] private Renderer sightRenderer;
        [SerializeField] private LayerMask mask;
        [SerializeField] private Color color;

        private Vector3 rgb;

        private void Start()
        {
            rgb = new Vector3(color.r, color.g, color.b);
        }

        private void FixedUpdate()
        {
            foreach (var lens in LensComponent.Active)
            {
                if (!Similar(lens.RGB, rgb))
                {
                    continue;
                }

                var dir = (sightRenderer.bounds.center - lens.Position);

                if (Vector3.Angle(lens.Forward, dir.normalized) > 45f)
                {
                    continue;
                }

                var dist = dir.magnitude;

                if (Physics.Raycast(lens.Position, dir.normalized, out _, dist, mask))
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

        public void SetColor(Color clr)
        {
            color = clr;
            rgb = new Vector3(color.r, color.g, color.b);
        }
    }
}