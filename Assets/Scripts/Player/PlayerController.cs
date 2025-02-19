using Managers;
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private NetworkObject networkObject;
        [SerializeField] private Behaviour[] localComponents;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private bool isDead;
#endif

        private void Start()
        {
            if (!networkObject.IsOwner)
                return;

            foreach (Behaviour localComponent in localComponents)
                localComponent.enabled = true;
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Input.GetKeyDown(KeyCode.G) && networkObject.IsOwner)
            {
                if (!isDead)
                    GameManager.Instance.KillPlayerServerRpc(networkObject.OwnerClientId);
                else
                    GameManager.Instance.RevivePlayerServerRpc(networkObject.OwnerClientId);

                isDead = !isDead;
            }
#endif
        }
    }
}