using System.Collections.Generic;
using Anomalies;
using Train;
using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    public class TrainManager : NetworkBehaviour
    {
        public static TrainManager Instance { get; private set; }

        [SerializeField] private GameObject defaultWagon;
        [SerializeField] private GameObject[] anomalyWagons;
        [SerializeField] private GameObject vestibulePrefab;
        [SerializeField] private List<GameObject> trainPool = new List<GameObject>();
        [SerializeField] private bool currentWagonHasAnomaly = false;

        // Offsets for wagon and vestibule positioning
        private Vector3 _wagonOffset;
        private Vector3 _wagonReversedOffset;
        private Vector3 _vestibuleOffset;

        private int _currentWagonIndex = 0;
        private AnomalyBase _currentAnomaly = null;

        // Current wagon anomaly flag

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
            _currentWagonIndex = (_currentWagonIndex + 1) % anomalyWagons.Length;

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
            var instantiate = InstantiateWagon(anomalyWagons[_currentWagonIndex], vestibuleType, position);

            if (!instantiate.TryGetComponent(out _currentAnomaly))
            {
                Debug.LogWarning("Anomalous wagon missing anomaly component.");
                return;
            }

            _currentAnomaly.Activate();
        }

        private void SpawnFirstWagon(VestibuleType vestibuleType, Vector3 position)
        {
            InstantiateWagon(defaultWagon, vestibuleType, position);
        }

        private GameObject InstantiateWagon(GameObject wagonPrefab, VestibuleType vestibuleType, Vector3 position)
        {
            GameObject wagon = Instantiate(wagonPrefab);
            GameObject vestibule = Instantiate(vestibulePrefab);

            WagonController wagonController = wagon.GetComponent<WagonController>();
            VestibuleController vestibuleController = vestibule.GetComponent<VestibuleController>();
            SetOffsets(wagonController, vestibuleController, vestibuleType);

            wagon.transform.position = position - _wagonOffset;
            wagon.name = "wagon" + _currentWagonIndex;
            wagon.GetComponent<NetworkObject>().Spawn();

            vestibuleController.VestibuleDirection = vestibuleType;
            vestibule.transform.position = position + _wagonReversedOffset - _wagonOffset - _vestibuleOffset;
            vestibule.name = "vestibule" + _currentWagonIndex;
            vestibule.GetComponent<NetworkObject>().Spawn();

            trainPool.Add(wagon);
            trainPool.Add(vestibule);
            DespawnWagons(vestibuleType);

            currentWagonHasAnomaly = wagonController.hasAnomaly;

            return wagon;
        }

        private void SetOffsets(WagonController wagonController, VestibuleController vestibuleController, VestibuleType vestibuleType)
        {
            _wagonOffset = wagonController.GetOffest(vestibuleType);
            _wagonReversedOffset = wagonController.GetReversedOffset(vestibuleType);
            _vestibuleOffset = vestibuleController.GetOffset(vestibuleType);
        }

        private void DespawnWagons(VestibuleType vestibuleType)
        {
            // TODO: Узнать у несказу где удалять аномалии
            _currentAnomaly?.Deactivate();
            _currentAnomaly = null;

            int startIndex = vestibuleType == VestibuleType.Forward ? 0 : 1;

            for (int i = startIndex; i < startIndex + 2 && i < trainPool.Count; i++)
            {
                trainPool[i].GetComponent<NetworkObject>().Despawn();
            }

            trainPool.RemoveRange(startIndex, Mathf.Min(2, trainPool.Count - startIndex));

            if (vestibuleType != VestibuleType.Forward)
            {
                trainPool.Reverse();
            }
        }
    }
}