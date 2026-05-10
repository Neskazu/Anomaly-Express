using System.Collections.Generic;
using Anomalies;
using MegaAnomalies;
using R3;
using Scene;
using Train;
using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    public class TrainManager : NetworkBehaviour
    {
        public static TrainManager Instance { get; private set; }

        public Observable<GameObject> OnNewWagon => _onNewWagon;

        [SerializeField] private GameObject defaultWagon;
        [SerializeField] private GameObject[] anomalyWagons;
        [SerializeField] private GameObject vestibulePrefab;
        [SerializeField] private List<GameObject> trainPool = new List<GameObject>();
        [SerializeField] private bool currentWagonHasAnomaly = false;

        [SerializeField, Range(0f, 1f)] private float baseAnomalyChance = 0.6f;
        [SerializeField] private float anomalyChanceIncrease = 0.05f;
        [SerializeField] private int maxMissStreak = 5;

        // Offsets for wagon and vestibule positioning
        private Vector3 _wagonOffset;
        private Vector3 _wagonReversedOffset;
        private Vector3 _vestibuleOffset;
        public int CurrentWagonIndex => _currentWagonIndex;
        private int _currentWagonIndex = 0;
        private int _passedAnomalyWagons = 0;
        private AnomalyBase _currentAnomaly = null;

        private Subject<GameObject> _onNewWagon;
        [SerializeField] private SceneTransitionSequence sequence;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _onNewWagon = new Subject<GameObject>().AddTo(this);
        }

        public void SpawnWagon(VestibuleType vestibuleType, Vector3 position, bool isBackward)
        {
            if (!IsServer)
            {
                return;
            }

            _currentWagonIndex = (_currentWagonIndex + 1) % anomalyWagons.Length;
            Debug.Log(_currentWagonIndex);
            if(_currentWagonIndex==0)
            {
                LoadToMegaAnomaly();
                return;
            }

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
        public async void LoadToMegaAnomaly()
        {
            Debug.Log("Load");
            await AnomalyTransitionAnimation.Instance.Play();
            await SceneTransitionController.Instance.Play(sequence);
        }
        //public void SpawnWagon(VestibuleType vestibuleType, Vector3 position, bool isBackward)
        //{
        //    if (!IsServer)
        //    {
        //        return;
        //    }

        //    _currentWagonIndex = Random.Range(0, anomalyWagons.Length);

        //    bool shouldSpawnDefault = (currentWagonHasAnomaly && !isBackward) || (!currentWagonHasAnomaly && isBackward);

        //    if (shouldSpawnDefault)
        //    {
        //        SpawnFirstWagon(vestibuleType, position);
        //        _passedAnomalyWagons = 0;
        //        return;
        //    }

        //    // guaranteed
        //    if (_passedAnomalyWagons >= maxMissStreak)
        //    {
        //        SpawnAnomalyWagon(vestibuleType, position);
        //        _passedAnomalyWagons = 0;
        //        return;
        //    }

        //    // chance
        //    float currentChance = baseAnomalyChance + (_passedAnomalyWagons * anomalyChanceIncrease);
        //    currentChance = Mathf.Clamp01(currentChance);

        //    if (Random.value <= currentChance)
        //    {
        //        SpawnAnomalyWagon(vestibuleType, position);
        //        _passedAnomalyWagons = 0;
        //    }
        //    else
        //    {
        //        SpawnFirstWagon(vestibuleType, position);
        //        _passedAnomalyWagons++;
        //    }
        //}

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

            _onNewWagon.OnNext(wagon);
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