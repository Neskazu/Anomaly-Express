using Managers;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using DG.Tweening;

namespace Train
{
    public class VestibuleController : NetworkBehaviour
    {

        [SerializeField]
        private VestibuleType vestibuleDirection = VestibuleType.Forward;
        public VestibuleType VestibuleDirection {  get { return vestibuleDirection; } set { vestibuleDirection = value; } }

        [SerializeField]
        private bool isBackward = false;

        [SerializeField]
        private Transform spawnpointForward;

        [SerializeField]
        private Transform spawnpointBackward;
        [SerializeField]
        private DoorController doorBackward;
        [SerializeField]
        private DoorController doorForward;
        [SerializeField]
        private List<ulong> clientsInVestibule;
        //надо будет брать у двери 
        [SerializeField] private float doorTweenDuration = .5f;
        //local flag for door
        private bool forwardIsOpen = false;
        private bool backwardIsOpen = false;
        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            // Подписываемся на события изменений дверей
            doorForward.OnDoorStateChanged += OnDoorStateChanged;
            doorBackward.OnDoorStateChanged += OnDoorStateChanged;

            // Инициализируем локальные флаги из текущего состояния сетевого флага двери
            forwardIsOpen = doorForward.IsOpenNetwork();
            backwardIsOpen = doorBackward.IsOpenNetwork();

        }

        private void OnDoorStateChanged(DoorController doorController, bool isOpen)
        {
            if (doorController == doorForward)
            {
                forwardIsOpen = isOpen;
            }
            else if (doorController == doorBackward)
            {
                backwardIsOpen = isOpen;
            }
            if(!forwardIsOpen&&!backwardIsOpen&&isAllPlayerInVestibule())
            {
                LoadNextLevel();
            }

        }
        private IEnumerator DelayedLoadNext()
        {
            if (IsServer)
            {

                doorBackward.ToggleLockServerRpc();
                doorForward.ToggleLockServerRpc();
            }
            // ждём полного завершения анимации закрытия (tweenDuration)
            yield return new WaitForSeconds(doorTweenDuration);

            // Теперь уже безопасно спавнить/отгружать уровень
            SpawnWagonBasedOnDirection();
            isBackward = true;
        }
        private void LoadNextLevel()
        {
            if (!IsServer)
            {
                return;
            }
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
            DoorController door = doorForward;
            doorForward = doorBackward;
            doorBackward = door;
        }
        private void Start()
        {
            if (!IsServer)
            {
                return ;
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
                TrainManager.Instance.SpawnWagon(vestibuleDirection, spawnpointForward.position, isBackward);
                vestibuleDirection = VestibuleType.Backward;
            }
            else
            {
                TrainManager.Instance.SpawnWagon(vestibuleDirection, spawnpointBackward.position, isBackward);
                vestibuleDirection = VestibuleType.Forward;
            }
        }

        public Vector3 GetOffset(VestibuleType vestibuleType)
        {
            if (vestibuleType == VestibuleType.Backward)
            {
                return spawnpointForward.position - spawnpointBackward.position;
            }
            return Vector3.zero;
        }
        public bool isAllPlayerInVestibule()
        {
            int total = NetworkManager.Singleton.ConnectedClients.Count;
            return clientsInVestibule.Count == total;
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("trigger enter");
            if(!IsServer)
            {
                return;
            }
            var networkObject = other.GetComponent<NetworkObject>();
            if (networkObject == null)
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
            if (networkObject == null)
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
