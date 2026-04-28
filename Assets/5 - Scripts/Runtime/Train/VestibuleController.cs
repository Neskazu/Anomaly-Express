using Managers;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using DG.Tweening;

namespace Train
{
    public class VestibuleController : NetworkBehaviour
    {
        [SerializeField] private VestibuleType vestibuleDirection = VestibuleType.Forward;
        [SerializeField] private bool isBackward = false;
        [SerializeField] private Transform spawnPointForward;
        [SerializeField] private Transform spawnPointBackward;
        [SerializeField] private DoorController doorBackward;
        [SerializeField] private DoorController doorForward;
        [SerializeField] private List<ulong> clientsInVestibule;
        [Header("Visuals")]
        [SerializeField] private List<WagonNumberAnimate> wagonNumberAnimators;

        // TODO: надо будет брать у двери
        [SerializeField] private float doorTweenDuration = .5f;

        // local flag for door
        private bool _forwardIsOpen = false;
        private bool _backwardIsOpen = false;

        public VestibuleType VestibuleDirection
        {
            get => vestibuleDirection;
            set => vestibuleDirection = value;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            doorForward.OnDoorStateChanged += OnDoorStateChanged;
            doorBackward.OnDoorStateChanged += OnDoorStateChanged;

            _forwardIsOpen = doorForward.IsOpenNetwork();
            _backwardIsOpen = doorBackward.IsOpenNetwork();
        }

        private void OnDoorStateChanged(DoorController doorController, bool isOpen)
        {
            if (doorController == doorForward)
            {
                _forwardIsOpen = isOpen;
            }
            else if (doorController == doorBackward)
            {
                _backwardIsOpen = isOpen;
            }

            if (!_forwardIsOpen && !_backwardIsOpen && IsAllPlayerInVestibule())
            {
                LoadNextLevel();
            }
        }

        private void LoadNextLevel()
        {
            if (!IsServer)
            {
                return;
            }
            AnimateWagonNumbersClientRpc(TrainManager.Instance.CurrentWagonIndex);
            doorBackward.ToggleLockServerRpc();

            DOVirtual.DelayedCall(doorTweenDuration, () =>
            {
                SpawnWagonBasedOnDirection();
                isBackward = true;
                doorForward.ToggleLockServerRpc();
                SwapDoors();
            });
        }

        void SwapDoors()
        {
            (doorForward, doorBackward) = (doorBackward, doorForward);
        }
        [ClientRpc]
        private void AnimateWagonNumbersClientRpc(int nextIndex)
        {
            foreach (var animator in wagonNumberAnimators)
            {
                if (animator != null)
                {
                    Debug.Log("animation"+nextIndex);
                    animator.PlayScroll(nextIndex);
                }
            }
        }
        private void Start()
        {
            if (!IsServer)
            {
                return;
            }

            if (vestibuleDirection == VestibuleType.Forward)
            {
                doorBackward.ForceInteract(gameObject);
            }
            else
            {
                doorForward.ForceInteract(gameObject);
            }
        }

        private void SpawnWagonBasedOnDirection()
        {
            if (vestibuleDirection == VestibuleType.Forward)
            {
                TrainManager.Instance.SpawnWagon(vestibuleDirection, spawnPointForward.position, isBackward);
                vestibuleDirection = VestibuleType.Backward;
            }
            else
            {
                TrainManager.Instance.SpawnWagon(vestibuleDirection, spawnPointBackward.position, isBackward);
                vestibuleDirection = VestibuleType.Forward;
            }
        }

        public Vector3 GetOffset(VestibuleType vestibuleType)
        {
            if (vestibuleType == VestibuleType.Backward)
            {
                return spawnPointForward.position - spawnPointBackward.position;
            }

            return Vector3.zero;
        }

        public bool IsAllPlayerInVestibule()
        {
            int total = NetworkManager.Singleton.ConnectedClients.Count;
            return clientsInVestibule.Count == total;
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("trigger enter");

            if (!IsServer)
            {
                return;
            }

            var networkObject = other.GetComponent<NetworkObject>();

            if (!networkObject)
            {
                return;
            }

            ulong clientId = networkObject.OwnerClientId;
            clientsInVestibule.Add(clientId);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsServer)
            {
                return;
            }

            var networkObject = other.GetComponent<NetworkObject>();

            if (!networkObject)
            {
                return;
            }

            ulong clientId = networkObject.OwnerClientId;
            clientsInVestibule.Remove(clientId);
        }
    }

    public enum VestibuleType
    {
        Forward,
        Backward
    }
}