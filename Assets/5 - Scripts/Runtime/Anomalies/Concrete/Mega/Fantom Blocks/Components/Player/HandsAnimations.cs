using R3;
using Unity.Netcode;
using UnityEngine;

namespace Anomalies
{
    public class HandsAnimations : MonoBehaviour
    {
        private static readonly int Active = Animator.StringToHash("Phone Active");
        private static readonly int PhotoMode = Animator.StringToHash("Phone Photo Mode");

        private HandsComponent handsComponent;
        private Animator playerAnimator;

        public void Setup(HandsController controller, GameObject hands)
        {
            handsComponent = hands.GetComponentInChildren<HandsComponent>();

            controller.Active
                .Subscribe(OnPhoneActive)
                .AddTo(this);

            controller.PhotoMode
                .Subscribe(OnPhotoMode)
                .AddTo(this);
        }

        private void OnPhoneActive(bool active)
        {
            handsComponent.HandAnimator.SetBool(Active, active);

            PlayBodyAnimation(Active, active);
        }

        private void OnPhotoMode(bool active)
        {
            handsComponent.HandAnimator.SetBool(PhotoMode, active);

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