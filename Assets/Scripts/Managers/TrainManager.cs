using System.Collections.Generic;
using Train;
using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    public class TrainManager : NetworkBehaviour
    {
        public static TrainManager Instance { get; private set; }
        private int currentWagonIndex = 0;
        [SerializeField] private GameObject defaultWagon;
        [SerializeField] private GameObject[] anomalyWagons;
        [SerializeField] private GameObject vestibulePrefab;
        [SerializeField] private List<GameObject> trainPool = new List<GameObject>();

        // Offsets for wagon and vestibule positioning
        private Vector3 wagonOffset;
        private Vector3 wagonReversedOffset;
        private Vector3 vestibuleOffset;

        // Current wagon anomaly flag
        [SerializeField] private bool currentWagonHasAnomaly = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SpawnWagon(VestibuleType vestibuleType, Vector3 position, bool isBackward)
        {
            if (!IsServer)
            {
                return;
            }

            // TODO: сделать выбор случайным и без повторов
            currentWagonIndex = (currentWagonIndex + 1) % anomalyWagons.Length;

            bool shouldSpawnDefault = (currentWagonHasAnomaly && !isBackward) || (!currentWagonHasAnomaly && isBackward);
            if (shouldSpawnDefault)
            {
                SpawnFirstWagon(vestibuleType, position);
            }
            else
            {
                SpawnAnomalyWagon(vestibuleType, position);
            }
        }

        private void SpawnAnomalyWagon(VestibuleType vestibuleType, Vector3 position)
        {
            InstantiateWagon(anomalyWagons[currentWagonIndex], vestibuleType, position);
        }

        private void SpawnFirstWagon(VestibuleType vestibuleType, Vector3 position)
        {
            InstantiateWagon(defaultWagon, vestibuleType, position);
        }

        private void InstantiateWagon(GameObject wagonPrefab, VestibuleType vestibuleType, Vector3 position)
        {
            GameObject wagon = Instantiate(wagonPrefab);
            GameObject vestibule = Instantiate(vestibulePrefab);

            WagonController wagonController = wagon.GetComponent<WagonController>();
            VestibuleController vestibuleController = vestibule.GetComponent<VestibuleController>();
            SetOffsets(wagonController, vestibuleController, vestibuleType);

            wagon.transform.position = position - wagonOffset;
            wagon.name = "wagon" + currentWagonIndex;
            wagon.GetComponent<NetworkObject>().Spawn();

            vestibuleController.VestibuleDirection = vestibuleType;
            vestibule.transform.position = position + wagonReversedOffset - wagonOffset - vestibuleOffset;
            vestibule.name = "vestibule" + currentWagonIndex;
            vestibule.GetComponent<NetworkObject>().Spawn();

            trainPool.Add(wagon);
            trainPool.Add(vestibule);
            DespawnWagons(vestibuleType);

            currentWagonHasAnomaly = wagonController.hasAnomaly;
        }

        private void SetOffsets(WagonController wagonController, VestibuleController vestibuleController, VestibuleType vestibuleType)
        {
            wagonOffset = wagonController.GetOffest(vestibuleType);
            wagonReversedOffset = wagonController.GetReversedOffset(vestibuleType);
            vestibuleOffset = vestibuleController.GetOffset(vestibuleType);
        }

        private void DespawnWagons(VestibuleType vestibuleType)
        {
            int startIndex = vestibuleType == VestibuleType.Forward ? 0 : 1;

            for (int i = startIndex; i < startIndex + 2 && i < trainPool.Count; i++)
            {
                trainPool[i].GetComponent<NetworkObject>().Despawn(true);
            }
            trainPool.RemoveRange(startIndex, Mathf.Min(2, trainPool.Count - startIndex));

            if (vestibuleType != VestibuleType.Forward)
            {
                trainPool.Reverse();
            }
        }
    }
}
