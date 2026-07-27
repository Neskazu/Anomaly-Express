using R3;
using UnityEngine;

namespace Anomalies
{
    public class HandsAnimations : MonoBehaviour
    {
        private static readonly int Active = Animator.StringToHash("Active");
        private static readonly int PhotoMode = Animator.StringToHash("Photo Mode");

        private HandsComponent handsComponent;

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
            handsComponent.PhoneAnimator.SetBool(Active, active);
        }

        private void OnPhotoMode(bool active)
        {
            handsComponent.HandAnimator.SetBool(PhotoMode, active);
            handsComponent.PhoneAnimator.SetBool(PhotoMode, active);
        }
    }
}