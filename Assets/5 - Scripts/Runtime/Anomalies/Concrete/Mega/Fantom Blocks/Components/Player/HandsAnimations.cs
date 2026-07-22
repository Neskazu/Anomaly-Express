using R3;
using Unity.Netcode;
using UnityEngine;

namespace Anomalies
{
    public class HandsAnimations : NetworkBehaviour
    {
        private static readonly int Active = Animator.StringToHash("Active");
        private static readonly int PhotoMode = Animator.StringToHash("Photo Mode");

        private HandsController controller;
        private Animator animator;

        public void Setup(HandsController handsController, GameObject hands)
        {
            controller = handsController;
            animator = hands.GetComponentInChildren<Animator>();

            controller.Active
                .Subscribe(OnPhoneActive)
                .AddTo(this);

            controller.PhotoMode
                .Subscribe(OnPhotoMode)
                .AddTo(this);
        }

        private void OnPhoneActive(bool active)
        {
            if (IsOwner)
            {
                animator.SetBool(Active, active);
            }
        }

        private void OnPhotoMode(bool active)
        {
            if (IsOwner)
            {
                animator.SetBool(PhotoMode, active);
            }
        }
    }
}