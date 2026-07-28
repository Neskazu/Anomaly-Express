using System.Collections.Generic;
using Anomalies;
using MegaAnomalies;
using R3;
using SaveSystem;
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
        private List<int> _unseenAnomalies = new List<int>();
        private List<int> _seenAnomalies = new List<int>();

        [SerializeField] private GameObject vestibulePrefab;

        [SerializeField] private List<GameObject> trainPool = new List<GameObject>();

        [SerializeField] private bool currentWagonHasAnomaly = false;

        [SerializeField, Range(0f, 1f)] private float baseAnomalyChance = 0.6f;
        [SerializeField] private float anomalyChanceIncrease = 0.05f;
        [SerializeField] private int maxMissStreak = 5;
        [SerializeField, Range(0f, 1f)] private float unseenAnomalyChance = 0.8f;
        [SerializeField] private int WagonsBeforeMega = 1;

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
        private void Start()
        {
            if (IsServer)
            {
                InitializeAnomalyPools();
            }
        }
        public async void LoadToMegaAnomaly()
        {
            await AnomalyTransitionAnimation.Instance.Play();
            if (IsServer)
            {
                ClearAllWagons();
            }
            await SceneTransitionController.Instance.Play(sequence);
        }
        public void SpawnWagon(VestibuleType vestibuleType, Vector3 position, bool isBackward)
        {
            if (!IsServer)
            {
                return;
            }

            bool shouldSpawnDefault = (currentWagonHasAnomaly && !isBackward) || (!currentWagonHasAnomaly && isBackward);

            if (shouldSpawnDefault)
            {
                SpawnFirstWagon(vestibuleType, position);
                _passedAnomalyWagons = 0;
                _currentWagonIndex = 0;
                return;
            }
            if (_currentWagonIndex == WagonsBeforeMega)
            {
                LoadToMegaAnomaly();
                return;
            }
            _currentWagonIndex += 1;

            // guaranteed
            if (_passedAnomalyWagons >= maxMissStreak)
            {
                SpawnAnomalyWagon(vestibuleType, position);
                _passedAnomalyWagons = 0;
                return;
            }

            // chance
            float currentChance = baseAnomalyChance + (_passedAnomalyWagons * anomalyChanceIncrease);
            currentChance = Mathf.Clamp01(currentChance);

            if (Random.value <= currentChance)
            {
                SpawnAnomalyWagon(vestibuleType, position);
                _passedAnomalyWagons = 0;
            }
            else
            {
                SpawnFirstWagon(vestibuleType, position);
                _passedAnomalyWagons++;
            }
        }

        private void SpawnAnomalyWagon(VestibuleType vestibuleType, Vector3 position)
        {
            GameObject prefabToSpawn = GetNextAnomalyPrefab();

            if (prefabToSpawn == null)
            {
                Debug.LogError("No anomaly prefab selected!");
                return;
            }

            var instantiate = InstantiateWagon(prefabToSpawn, vestibuleType, position);

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
            wagon.name = "wagon " + wagonPrefab.name + " " + _currentWagonIndex;
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
        private void InitializeAnomalyPools()
        {
            SaveManager.Load();

            _seenAnomalies = SaveManager.Save.Session.SeenAnomalies ?? new List<int>();
            _unseenAnomalies.Clear();

            for (int i = 0; i < anomalyWagons.Length; i++)
            {
                if (!_seenAnomalies.Contains(i))
                {
                    _unseenAnomalies.Add(i);
                }
            }

            if (_unseenAnomalies.Count == 0 && anomalyWagons.Length > 0)
            {
                ResetAnomaliesProgress();
            }
        }
        private void ResetAnomaliesProgress()
        {
            _unseenAnomalies.Clear();
            for (int i = 0; i < anomalyWagons.Length; i++)
            {
                _unseenAnomalies.Add(i);
            }

            _seenAnomalies.Clear();
            SaveManager.Save.Session.SeenAnomalies = _seenAnomalies;
            SaveManager.SaveGame();
        }
        private GameObject GetNextAnomalyPrefab()
        {
            if (anomalyWagons.Length == 0) return null;

            if (_unseenAnomalies.Count == 0)
            {
                ResetAnomaliesProgress();
            }

            bool pickUnseen = true;

            if (_seenAnomalies.Count > 0)
            {
                pickUnseen = Random.value <= unseenAnomalyChance;
            }

            int selectedPrefabIndex = 0;

            if (pickUnseen)
            {
                int randomListIndex = Random.Range(0, _unseenAnomalies.Count);
                selectedPrefabIndex = _unseenAnomalies[randomListIndex];

                _unseenAnomalies.RemoveAt(randomListIndex);
                _seenAnomalies.Add(selectedPrefabIndex);

                SaveManager.Save.Session.SeenAnomalies = _seenAnomalies;
                SaveManager.SaveGame();
            }
            else
            {
                int randomListIndex = Random.Range(0, _seenAnomalies.Count);
                selectedPrefabIndex = _seenAnomalies[randomListIndex];
            }

            return anomalyWagons[selectedPrefabIndex];
        }
        private void ClearAllWagons()
        {
            _currentAnomaly?.Deactivate();
            _currentAnomaly = null;

            foreach (var trainObj in trainPool)
            {
                if (trainObj != null && trainObj.TryGetComponent(out NetworkObject netObj))
                {
                    if (netObj.IsSpawned)
                    {
                        netObj.Despawn();
                    }
                }
            }

            trainPool.Clear();
        }
    }
}