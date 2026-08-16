using Unity.Netcode;
using UnityEngine;

namespace Nac.Network
{
    public class PlayerLimitValidator : NetworkValidator
    {
        [SerializeField, Min(1)] private int max;

        public override bool Validate(NetworkManager.ConnectionApprovalRequest request, out string reason)
        {
            if (NetworkManager.Singleton.ConnectedClients.Count >= max)
            {
                reason = NetworkReasons.PlayerLimit;
                return false;
            }

            reason = null;
            return true;
        }
    }
}