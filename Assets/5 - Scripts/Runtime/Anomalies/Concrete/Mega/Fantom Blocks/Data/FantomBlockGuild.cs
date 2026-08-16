using System;
using System.Collections.Generic;
using UnityEngine;

namespace Anomalies
{
    [Serializable]
    public class FantomBlockGuild
    {
        [SerializeField] private FantomComponent[] components;

        public IReadOnlyList<FantomComponent> Components => components;

        public ulong OwnerId { get; private set; }

        public void SetOwner(ulong id)
        {
            OwnerId = id;
        }

        public void SetColor(Color clr)
        {
            foreach (var fantomComponent in components)
            {
                fantomComponent.UpdateColorRpc(OwnerId);
            }
        }
    }
}