using Managers;
using Unity.Netcode;
using UnityEngine;

namespace Train
{
    public class VestibuleController : NetworkBehaviour
    {
        //test
        public bool isLoad = false;
        [SerializeField]
        private VestibuleType VestibuleDirection = VestibuleType.Forward;
        [SerializeField]
        private bool isBackward = false;
        [SerializeField]
        private Transform spawnpointForward;
        [SerializeField]
        private Transform spawnpointBackward;
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        // Update is called once per frame
        void Update()
        {
            if (!IsServer)
            {
                return;
            }
            if (isLoad)
            {
                isLoad = false;
                if (VestibuleDirection == VestibuleType.Forward)
                {
                    TrainManager.Instance.SpawnWagon(VestibuleDirection, spawnpointForward.position, isBackward);
                    VestibuleDirection = VestibuleType.Backward;
                }
                else
                {
                    TrainManager.Instance.SpawnWagon(VestibuleDirection, spawnpointBackward.position, isBackward);
                    VestibuleDirection = VestibuleType.Forward;
                }
                isBackward = true;

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