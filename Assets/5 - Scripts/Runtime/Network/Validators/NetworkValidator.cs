using Unity.Netcode;
using UnityEngine;

namespace Nac.Network
{
    public abstract class NetworkValidator : ScriptableObject
    {
        public abstract bool Validate(NetworkManager.ConnectionApprovalRequest request, out string reason);
    }
}