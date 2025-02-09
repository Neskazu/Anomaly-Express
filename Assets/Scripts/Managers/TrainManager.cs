using System.Collections.Generic;
using Train;
using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    public class TrainManager : NetworkBehaviour
    {
        public static TrainManager Instance { get; private set; }
        public int currentWagonIndex = 0;
        [SerializeField] private GameObject DeffaultWagon;
        [SerializeField] private GameObject[] AnomalyWagons;
        [SerializeField] private GameObject vestibulePrefab;

        [SerializeField] private List<GameObject> trainPool;

        //offsets
        private Vector3 wagonOffset;
        private Vector3 wagonReversedOffset;

        private Vector3 vestibuleOffset;

        //current data
        [SerializeField] private bool currentWagonHasAnomaly = false;

        private void Awake()
        {
            Instance = this;
        }

        public void SpawnWagon(VestibuleType VestibuleDirection, Vector3 position, bool isBackward)
        {
            if (!IsServer)
            {
                return;
            }

            //todo make it random and non repeating
            currentWagonIndex++;

            if (currentWagonIndex >= AnomalyWagons.Length)
            {
                currentWagonIndex = 0;
            }

            if (currentWagonHasAnomaly && !isBackward)
            {
                SpawnFirsWagon(VestibuleDirection, position);
                return;
            }

            if (!currentWagonHasAnomaly && isBackward)
            {
                SpawnFirsWagon(VestibuleDirection, position);
                return;
            }

            SpawnNextWagon(VestibuleDirection, position);
        }

        void SpawnNextWagon(VestibuleType VestibuleDirection, Vector3 position)
        {
            InstantiateWagon(AnomalyWagons[currentWagonIndex], VestibuleDirection, position);
        }

        void SpawnFirsWagon(VestibuleType vestibuleType, Vector3 position)
        {
            InstantiateWagon(DeffaultWagon, vestibuleType, position);
        }

        void DespawnWagons(VestibuleType vestibuleType)
        {
            if (vestibuleType == VestibuleType.Forward)
            {
                trainPool[0].GetComponent<NetworkObject>().Despawn(true);
                trainPool[1].GetComponent<NetworkObject>().Despawn(true);
                trainPool.RemoveRange(0, 2);
            }
            else
            {
                trainPool[1].GetComponent<NetworkObject>().Despawn(true);
                trainPool[2].GetComponent<NetworkObject>().Despawn(true);
                trainPool.RemoveRange(1, 2);
                trainPool.Reverse();
            }
        }

        void InstantiateWagon(GameObject wagonPrefab, VestibuleType vestibuleType, Vector3 position)
        {
            GameObject wagon = Instantiate(wagonPrefab);
            GameObject vestibule = Instantiate(vestibulePrefab);
            WagonController wagonController = wagon.GetComponent<WagonController>();
            SetOffsets(wagonController, vestibule.GetComponent<VestibuleController>(), vestibuleType);
            //set spawn postion based on offsets
            wagon.transform.position = position - wagonOffset;
            wagon.name = "wagon" + currentWagonIndex;
            wagon.GetComponent<NetworkObject>().Spawn();
            vestibule.transform.position = position + wagonReversedOffset - wagonOffset - vestibuleOffset;
            vestibule.GetComponent<NetworkObject>().Spawn();
            //add to pool
            trainPool.Add(wagon);
            trainPool.Add(vestibule);
            DespawnWagons(vestibuleType);
            //set anomaly
            currentWagonHasAnomaly = wagonController.hasAnomaly;
        }

        void SetOffsets(WagonController wagonController, VestibuleController vestibuleController,
            VestibuleType vestibuleType)
        {
            wagonOffset = wagonController.GetOffest(vestibuleType);
            wagonReversedOffset = wagonController.GetReversedOffset(vestibuleType);
            vestibuleOffset = vestibuleController.GetOffset(vestibuleType);
        }
    }
}