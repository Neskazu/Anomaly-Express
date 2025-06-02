using Managers;
using Unity.Netcode;
using UnityEngine;

namespace Train
{
    public class VestibuleController : NetworkBehaviour
    {
        [SerializeField]
        private bool isLoad = false;

        [SerializeField]
        private VestibuleType vestibuleDirection = VestibuleType.Forward;
        public VestibuleType VestibuleDirection {  get { return vestibuleDirection; } set { vestibuleDirection = value; } }

        [SerializeField]
        private bool isBackward = false;

        [SerializeField]
        private Transform spawnpointForward;

        [SerializeField]
        private Transform spawnpointBackward;
        [SerializeField]
        private DoorController doorBackward;
        [SerializeField]
        private DoorController doorForward;

        void Update()
        {
            if (!IsServer)
            {
                return;
            }

            if (isLoad)
            {
                isLoad = false;
                SpawnWagonBasedOnDirection();
                isBackward = true;
            }
        }
        private void SpawnWagonBasedOnDirection()
        {
            if (vestibuleDirection == VestibuleType.Forward)
            {
                TrainManager.Instance.SpawnWagon(vestibuleDirection, spawnpointForward.position, isBackward);
                vestibuleDirection = VestibuleType.Backward;
            }
            else
            {
                TrainManager.Instance.SpawnWagon(vestibuleDirection, spawnpointBackward.position, isBackward);
                vestibuleDirection = VestibuleType.Forward;
            }
        }

        public Vector3 GetOffset(VestibuleType vestibuleType)
        {
            if (vestibuleType == VestibuleType.Backward)
            {
                return spawnpointForward.position - spawnpointBackward.position;
            }
            return Vector3.zero;
        }
    }

    public enum VestibuleType
    {
        Forward,
        Backward
    }
}
