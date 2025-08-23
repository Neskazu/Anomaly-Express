using DG.Tweening;
using System;
using Unity.Netcode;
using UnityEngine;

namespace Train
{
    public class DoorController : NetworkBehaviour, IInteractable
    {
        [SerializeField] private float openAngle = 100f;
        [SerializeField] private float shakeAngle = 2f;
        [SerializeField] private float tweenDuration = 0.5f;
        [SerializeField] private float shakeDuration = 0.15f;
        [SerializeField] private Collider doorCollider;
        [SerializeField] private Transform doorMesh;
        [SerializeField] private DoorType doorType;
        [SerializeField] private VestibuleController vestibuleController;
        [SerializeField] private bool isLocked = true;

        public event Action<DoorController, bool> OnDoorStateChanged;

        // Сетевой флаг заблокированости двери
        private NetworkVariable<bool> _netIsLocked = new NetworkVariable<bool>(true);
        // Сетевой флаг состояния двери
        private NetworkVariable<bool> _netIsOpen = new NetworkVariable<bool>();

        private Quaternion _closedRotationMesh;

        private Quaternion _closedRotationCollider;

        private void Awake()
        {
            _closedRotationMesh = doorMesh.localRotation;
            _closedRotationCollider = doorCollider.transform.localRotation;
        }

        public override void OnNetworkSpawn()
        {
            _netIsOpen.OnValueChanged += (_, newIsOpen) =>
            {
                if (newIsOpen)
                    return;
                else
                    ApplyCloseVisual();
            };

            if (_netIsOpen.Value)
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
            float angleSign = DetermineSignedAngle(interactor.transform.position);
            if (doorType == DoorType.Ordinary)
            {
                float signedAngle = angleSign * openAngle;

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
                if (!_netIsLocked.Value)
                {
                    float signedAngle = angleSign * openAngle;

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
                    float signedShakeAngle = angleSign * shakeAngle;
                    ShakeDoor(signedShakeAngle);
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
            bool newState = !_netIsOpen.Value;
            _netIsOpen.Value = newState;

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
            float lastAngle = openAngle*DetermineSignedAngle(Vector3.zero);
            OpenDoorClientRpc(lastAngle);
        }

        private void ApplyOpenVisual(float signedAngle)
        {
            Vector3 targetEulerMesh = _closedRotationMesh.eulerAngles + new Vector3(0f, signedAngle, 0f);
            Vector3 targetEulerCollider = _closedRotationCollider.eulerAngles + new Vector3(0f, signedAngle, 0f);
            doorMesh.DOLocalRotate(targetEulerMesh, tweenDuration).SetEase(Ease.OutCubic);
            doorCollider.transform.DOLocalRotate(targetEulerCollider, tweenDuration).SetEase(Ease.OutCubic);
        }

        private void ApplyCloseVisual()
        {
            doorMesh.DOLocalRotate(_closedRotationMesh.eulerAngles, tweenDuration).SetEase(Ease.OutCubic);
            doorCollider.transform.DOLocalRotate(_closedRotationCollider.eulerAngles, tweenDuration).SetEase(Ease.OutCubic);
        }

        private float DetermineSignedAngle(Vector3 interactorWorldPos)
        {
            Vector3 localPos = doorCollider.transform.InverseTransformPoint(interactorWorldPos);
            return (localPos.y >= 0f ? 1 : -1);
        }

        public void ChangeLock()
        {
            isLocked = !isLocked;
        }

        public bool IsOpenNetwork()
        {
            return _netIsOpen.Value;
        }

        [ServerRpc(RequireOwnership = false)]
        public void ToggleLockServerRpc()
        {
            _netIsLocked.Value = !_netIsLocked.Value;
        }
        private void ShakeDoor(float shakeAngle)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(doorMesh.DOLocalRotate(
                _closedRotationMesh.eulerAngles + new Vector3(0f, shakeAngle, 0f), shakeDuration))
               .Append(doorMesh.DOLocalRotate(
                _closedRotationMesh.eulerAngles - new Vector3(0f, shakeAngle, 0f), shakeDuration))
               .Append(doorMesh.DOLocalRotate(_closedRotationMesh.eulerAngles, shakeDuration));

            seq.SetEase(Ease.InOutSine);
        }
    }

    public enum DoorType
    {
        Ordinary,
        Level,
    }
}