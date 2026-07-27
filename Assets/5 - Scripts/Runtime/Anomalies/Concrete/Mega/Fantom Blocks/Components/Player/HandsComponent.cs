using UnityEngine;

namespace Anomalies
{
    public class HandsComponent : MonoBehaviour
    {
        [SerializeField] private Animator handAnimator;
        [SerializeField] private Animator phoneAnimator;

        public Animator HandAnimator => handAnimator;
        public Animator PhoneAnimator => phoneAnimator;
    }
}