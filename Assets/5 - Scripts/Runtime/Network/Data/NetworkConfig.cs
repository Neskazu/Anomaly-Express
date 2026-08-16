using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Nac.Network
{
    [CreateAssetMenu(fileName = "Network Config", menuName = "Nac/Network Controller Config")]
    public class NetworkConfig : ScriptableObject
    {
        [SerializeField] private NetworkValidator[] approvalValidators;
        [SerializeField] private NetworkObject[] networkServices;

        public IReadOnlyList<NetworkValidator> ApprovalValidators => approvalValidators;
        public IReadOnlyList<NetworkObject> NetworkServices => networkServices;
    }
}