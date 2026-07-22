using System.Collections.Generic;
using UnityEngine;

namespace Anomalies
{
    public class LensComponent : MonoBehaviour
    {
        private static readonly HashSet<LensComponent> Lens = new();
        public static IReadOnlyCollection<LensComponent> Active => Lens;

        [Header("References")]
        [SerializeField] private Transform cam;

        [Header("Settings")]
        [SerializeField] private Color color;

        public Vector3 RGB { get; private set; }

        public Vector3 Position => cam.transform.position;
        public Vector3 Forward => cam.transform.forward;

        private void Start()
        {
            RGB = new Vector3(color.r, color.g, color.b);
        }

        private void OnEnable()
        {
            Lens.Add(this);
        }

        private void OnDisable()
        {
            Lens.Remove(this);
        }

        public void UpdateColor(Color clr)
        {
            color = clr;
            RGB = new Vector3(clr.r, clr.g, clr.b);
        }
    }
}