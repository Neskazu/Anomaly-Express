using DG.Tweening;
using System;
using Unity.Netcode;
using UnityEngine;

namespace Train
{
    public class DoorController : NetworkBehaviour, IInteractable
    {
        [SerializeField] private float openAngle = 100f;
        [SerializeField] private float tweenDuration = 0.5f;
        [SerializeField] private Collider doorCollider;
        [SerializeField] private Transform doorMesh;
        [SerializeField] private DoorType doorType;
        [SerializeField] private VestibuleController vestibuleController;
        private Quaternion closedRotation;
        [SerializeField]
        private bool isLocked=true;

        // Сетевой флаг заблокированости двери
        private NetworkVariable<bool> netIsLocked = new NetworkVariable<bool>(
            true,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        // Сетевой флаг состояния двери
        private NetworkVariable<bool> netIsOpen = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        public event Action<DoorController, bool> OnDoorStateChanged;
        private void Awake()
        {
            closedRotation = doorMesh.localRotation;
        }

        public override void OnNetworkSpawn()
        {
            netIsOpen.OnValueChanged += (_, newIsOpen) =>
            {
                if (newIsOpen)
                    return;
                else
                    ApplyCloseVisual();
            };

            if (netIsOpen.Value)
            {
                RequestCurrentAngleServerRpc();
            }
            else
            {
                ApplyCloseVisual();
            }
        }

        public void Interact(GameObject interactor)
        {
            Debug.Log("interact");
            if (doorType == DoorType.Ordinary)
            {
                float signedAngle = DetermineSignedAngle(interactor.transform.position);

                if (IsServer)
                {
                    ChangeStateServerLogic(signedAngle);
                }
                else
                {
                    ChangeStateServerRpc(signedAngle);
                }
            }
            else
            {
                if (!netIsLocked.Value)
                {
                    float signedAngle = DetermineSignedAngle(interactor.transform.position);
                    if (IsServer)
                    {
                        ChangeStateServerLogic(signedAngle);
                    }
                    else
                    {
                        ChangeStateServerRpc(signedAngle);
                    }
                }
            }
           
        }
        public void ForceInteract(GameObject interactor)
        {
            if (IsServer)
            {
                ToggleLockServerRpc();
                ChangeStateServerLogic(openAngle);
            }
            else
            {
                ToggleLockServerRpc();
                ChangeStateServerRpc(openAngle);
            }
        }
        [ServerRpc(RequireOwnership = false)]
        private void ChangeStateServerRpc(float signedAngle)
        {
            ChangeStateServerLogic(signedAngle);
        }

        private void ChangeStateServerLogic(float signedAngle)
        {
            bool newState = !netIsOpen.Value;
            netIsOpen.Value = newState;

            if (newState)
            {
                OpenDoorClientRpc(signedAngle);
            }
            else
            {
                CloseDoorClientRpc();
            }
        }

        [ClientRpc]
        private void OpenDoorClientRpc(float signedAngle)
        {
            ApplyOpenVisual(signedAngle);
            OnDoorStateChanged?.Invoke(this, true);
        }

        [ClientRpc]
        private void CloseDoorClientRpc()
        {
            ApplyCloseVisual();
            OnDoorStateChanged?.Invoke(this, false);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestCurrentAngleServerRpc(ServerRpcParams rpcParams = default)
        {
            float lastAngle = DetermineSignedAngle(rpcParams.Receive.SenderClientId == 0
                ? Vector3.zero
                : Vector3.zero);
            OpenDoorClientRpc(lastAngle);
        }

        private void ApplyOpenVisual(float signedAngle)
        {
            Vector3 targetEuler = closedRotation.eulerAngles + new Vector3(0f, signedAngle, 0f);
            doorMesh.DOLocalRotate(targetEuler, tweenDuration).SetEase(Ease.OutCubic);
            doorCollider.transform.DOLocalRotate(targetEuler, tweenDuration).SetEase(Ease.OutCubic);
        }

        private void ApplyCloseVisual()
        {
            doorMesh.DOLocalRotate(closedRotation.eulerAngles, tweenDuration).SetEase(Ease.OutCubic);
            doorCollider.transform.DOLocalRotate(closedRotation.eulerAngles, tweenDuration).SetEase(Ease.OutCubic);
        }

        private float DetermineSignedAngle(Vector3 interactorWorldPos)
        {
            Vector3 localPos = doorCollider.transform.InverseTransformPoint(interactorWorldPos);
            Debug.Log(localPos);
            return (localPos.y >= 0f ? openAngle : -openAngle);
        }
        public void ChangeLock()
        {
            isLocked = !isLocked;
        }
        public bool IsOpenNetwork()
        {
            return netIsOpen.Value;
        }
        [ServerRpc(RequireOwnership = false)]
        public void ToggleLockServerRpc()
        {
            netIsLocked.Value = !netIsLocked.Value;
        }
    }
    public enum DoorType
    {
        Ordinary,
        Level,
    }
}
