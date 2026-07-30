using System;
using System.Collections.Generic;
using Managers;
using Network.Players;
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
            var data = MultiplayerManager.Players.Get(OwnerId);

            foreach (var fantomComponent in components)
            {
                fantomComponent.UpdateColorRpc(data.CharacterId);
            }
        }
    }
}