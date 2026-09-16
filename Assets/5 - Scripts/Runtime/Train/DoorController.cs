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
        public event Action OnDoorShaken;

        private NetworkVariable<bool> _netIsLocked = new NetworkVariable<bool>(true);
        private NetworkVariable<bool> _netIsOpen = new NetworkVariable<bool>();

        private Quaternion _closedRotationMesh;
        private Quaternion _closedRotationCollider;

        private bool _isAnimating = false;

        private float _lastServerInteractionTime = 0f;

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
            if (_isAnimating) return;

            float angleSign = DetermineSignedAngle(interactor.transform.position);

            if (doorType == DoorType.Ordinary)
            {
                float signedAngle = angleSign * openAngle;
                SendStateChangeRequest(signedAngle);
            }
            else
            {
                if (!_netIsLocked.Value)
                {
                    float signedAngle = angleSign * openAngle;
                    SendStateChangeRequest(signedAngle);
                }
                else
                {
                    float signedShakeAngle = angleSign * shakeAngle;
                    ShakeDoor(signedShakeAngle);
                }
            }
        }

        private void SendStateChangeRequest(float signedAngle)
        {
            if (IsServer)
            {
                ChangeStateServerLogic(signedAngle);
            }
            else
            {
                ChangeStateServerRpc(signedAngle);
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
            if (Time.time - _lastServerInteractionTime < tweenDuration * 0.9f) return;
            _lastServerInteractionTime = Time.time;

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
            float lastAngle = openAngle * DetermineSignedAngle(Vector3.zero);
            OpenDoorClientRpc(lastAngle);
        }

        private void ApplyOpenVisual(float signedAngle)
        {
            _isAnimating = true;
            KillCurrentTweens();

            Vector3 targetEulerMesh = _closedRotationMesh.eulerAngles + new Vector3(0f, signedAngle, 0f);
            Vector3 targetEulerCollider = _closedRotationCollider.eulerAngles + new Vector3(0f, signedAngle, 0f);

            doorMesh.DOLocalRotate(targetEulerMesh, tweenDuration).SetEase(Ease.OutCubic)
                .OnComplete(() => _isAnimating = false);

            doorCollider.transform.DOLocalRotate(targetEulerCollider, tweenDuration).SetEase(Ease.OutCubic);
        }

        private void ApplyCloseVisual()
        {
            _isAnimating = true;
            KillCurrentTweens();

            doorMesh.DOLocalRotate(_closedRotationMesh.eulerAngles, tweenDuration).SetEase(Ease.OutCubic)
                .OnComplete(() => _isAnimating = false);

            doorCollider.transform.DOLocalRotate(_closedRotationCollider.eulerAngles, tweenDuration).SetEase(Ease.OutCubic);
        }

        private void KillCurrentTweens()
        {
            doorMesh.DOKill();
            doorCollider.transform.DOKill();
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
            if (_isAnimating) return;

            _isAnimating = true;
            KillCurrentTweens();
            OnDoorShaken?.Invoke();

            Sequence seq = DOTween.Sequence();
            seq.Append(doorMesh.DOLocalRotate(
                    _closedRotationMesh.eulerAngles + new Vector3(0f, shakeAngle, 0f), shakeDuration))
                .Append(doorMesh.DOLocalRotate(
                    _closedRotationMesh.eulerAngles - new Vector3(0f, shakeAngle, 0f), shakeDuration))
                .Append(doorMesh.DOLocalRotate(_closedRotationMesh.eulerAngles, shakeDuration));

            seq.SetEase(Ease.InOutSine);
            seq.OnComplete(() => _isAnimating = false);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetLockServerRpc(bool lockState)
        {
            _netIsLocked.Value = lockState;
        }

        public bool IsLockedNetwork()
        {
            return _netIsLocked.Value;
        }
    }


public enum DoorType
    {
        Ordinary,
        Level,
    }
}