using System;
using System.Collections.Generic;
using UnityEngine;

namespace Anomalies
{
    [Serializable]
    public class FantomBlockGuild
    {
        [SerializeField] private ulong id;
        [SerializeField] private FantomBlockColor color;
        [SerializeField] private FantomComponent[] components;

        public ulong Id => id;
        public FantomBlockColor Color => color;
        public IReadOnlyCollection<FantomComponent> Components => components;
    }
}