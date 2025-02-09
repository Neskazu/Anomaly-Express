using System;
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    [DisallowMultipleComponent]
    public sealed class NetworkPlayer : NetworkBehaviour
    {
        // Dependencies
        [SerializeField] private GameObject humanModel;
        [SerializeField] private GameObject ghostModel;

        // Components will be enabled if the player is local
        [SerializeField] private Behaviour[] localComponents;

        private NetworkVariable<bool> isAlive = new(true, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Owner);

        [SerializeField] private LayerMask humanLayerMask;
        [SerializeField] private LayerMask ghostLayerMask;
        

        public override void OnNetworkSpawn()
        {
            isAlive.OnValueChanged += OnIsAliveChanged;

            if (!IsOwner)
                return;

            foreach (var localComponent in localComponents)
                localComponent.enabled = true;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner)
                return;

            foreach (var localComponent in localComponents)
                localComponent.enabled = false;
        }

        /* TODO:
         * Тут происходит трах single-responsibility
         * Вот бы у меня был список игроков и я мог вынести это в отдельную систему...
         */
        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.G) && IsOwner)
                isAlive.Value = !isAlive.Value;
#endif
        }

        private void OnIsAliveChanged(bool _, bool newValue)
        {
            if (newValue)
                ToHuman();
            else
                ToGhost();
        }

        private void ToGhost()
        {
            humanModel.SetActive(false);
            ghostModel.SetActive(true);

            if (IsOwner)
                UnityEngine.Camera.main.cullingMask = ghostLayerMask;
        }

        private void ToHuman()
        {
            humanModel.SetActive(true);
            ghostModel.SetActive(false);

            if (IsOwner) 
                UnityEngine.Camera.main.cullingMask = humanLayerMask;
        }
    }
}