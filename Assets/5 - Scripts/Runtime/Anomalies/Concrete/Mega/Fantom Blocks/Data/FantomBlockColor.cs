using System;
using UnityEngine;

namespace Anomalies
{
    [Serializable]
    public class FantomBlockColor
    {
        [SerializeField] private Color color;
        [SerializeField] private Gradient gradient;

        public Color RGBA => color;
        public Gradient Gradient => gradient;
    }
}