using System;
using DG.Tweening;
using R3;
using Unity.Netcode;
using UnityEngine;

namespace Anomalies
{
    public class HandsAnimations : MonoBehaviour
    {
        private static readonly int Active = Animator.StringToHash("Phone Active");
        private static readonly int PhotoMode = Animator.StringToHash("Phone Photo Mode");

        [SerializeField] private Vector3 restRotation;
        [SerializeField] private Vector3 photomodeRotation;
        [SerializeField] private float animationDuration;
        [SerializeField] private float idleRestSpeed;
        [SerializeField] private float idlePhotoSpeed;
        [SerializeField] private HandsSync handsSync;

        private HandsComponent handsComponent;
        private Transform handsRoot;
        private Animator playerAnimator;

        public void Setup(HandsController controller, GameObject hands)
        {
            handsComponent = hands.GetComponentInChildren<HandsComponent>();
            handsRoot = hands.transform.parent;

            controller.Active
                .Subscribe(OnPhoneActive)
                .AddTo(this);

            controller.PhotoMode
                .Subscribe(OnPhotoMode)
                .AddTo(this);

            handsComponent.Phone.position = handsComponent.PhoneRestOffset;
            handsRoot.position = handsComponent.HandsRestOffset;
        }

        private void OnPhoneActive(bool active)
        {
            handsComponent.HandAnimator.SetBool(Active, active);

            if (active)
            {
                handsSync.SyncPhoneRpc(true, false);
            }
            else
            {
                Observable
                    .Timer(TimeSpan.FromSeconds(animationDuration))
                    .Subscribe(_ => handsSync.SyncPhoneRpc(false, false))
                    .AddTo(this);
            }

            PlayBodyAnimation(Active, active);
        }

        private void OnPhotoMode(bool active)
        {
            var handRotation = active ? photomodeRotation : restRotation;
            var handsOffset = active ? handsComponent.HandsPhotoOffset : handsComponent.HandsRestOffset;
            var phoneOffset = active ? handsComponent.PhonePhotoOffset : handsComponent.PhoneRestOffset;
            var idleSpeed = active ? idlePhotoSpeed : idleRestSpeed;

            if (!active)
            {
                handsComponent.HandAnimator.speed = idleRestSpeed;
            }

            handsRoot.DOLocalMove(handsOffset, animationDuration);
            handsRoot.DOLocalRotate(handRotation, animationDuration)
                .OnComplete(() => handsComponent.HandAnimator.speed = idleSpeed);

            handsComponent.Phone.DOLocalMove(phoneOffset, animationDuration);
            handsComponent.HandAnimator.SetBool(PhotoMode, active);

            handsSync.SyncPhoneRpc(true, active);
            PlayBodyAnimation(PhotoMode, active);
        }

        private void PlayBodyAnimation(int id, bool value)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient)
            {
                return;
            }

            if (playerAnimator)
            {
                playerAnimator.SetBool(id, value);
            }
            else
            {
                var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
                playerAnimator = playerObj?.GetComponent<Animator>();

                if (playerAnimator)
                {
                    playerAnimator.SetBool(id, value);
                }
            }
        }
    }
}