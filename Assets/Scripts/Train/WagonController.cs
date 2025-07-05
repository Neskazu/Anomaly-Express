using UnityEngine;

namespace Train
{
    public class WagonController : MonoBehaviour
    {
        public bool hasAnomaly;
        public Transform forwardCenter;
        public Transform backwardCenter;

        public Vector3 GetOffest(VestibuleType vestibuleType)
        {
            if (vestibuleType == VestibuleType.Forward)
            {
                return forwardCenter.position;
            }

            return backwardCenter.position;
        }

        public Vector3 GetReversedOffset(VestibuleType vestibule)
        {
            if (vestibule == VestibuleType.Forward)
            {
                return backwardCenter.position;
            }

            return forwardCenter.position;
        }
    }
}