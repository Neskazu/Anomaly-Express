using UnityEngine;

namespace Anomalies
{
    public class HandsComponent : MonoBehaviour
    {
        [SerializeField] private Animator handAnimator;
        [SerializeField] private Transform phone;
        [SerializeField] private Vector3 handsRestOffset;
        [SerializeField] private Vector3 handsPhotoOffset;
        [SerializeField] private Vector3 phoneRestOffset;
        [SerializeField] private Vector3 phonePhotoOffset;

        public Animator HandAnimator => handAnimator;
        public Transform Phone => phone;

        public Vector3 HandsRestOffset => handsRestOffset;
        public Vector3 HandsPhotoOffset => handsPhotoOffset;
        public Vector3 PhoneRestOffset => phoneRestOffset;
        public Vector3 PhonePhotoOffset => phonePhotoOffset;
    }
}