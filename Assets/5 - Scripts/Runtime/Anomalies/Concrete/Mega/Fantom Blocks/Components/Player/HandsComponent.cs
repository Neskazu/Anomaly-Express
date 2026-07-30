using UnityEngine;

namespace Anomalies
{
    public class HandsComponent : MonoBehaviour
    {
        [SerializeField] private Animator handAnimator;

        public Animator HandAnimator => handAnimator;
    }
}