using System;
using System.Collections.Generic;
using Anomalies;
using MegaAnomalies;
using R3;
using SaveSystem;
using Scene;
using Train;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class TrainManager : NetworkBehaviour
    {
        public static TrainManager Instance { get; private set; }

        public Observable<GameObject> OnNewWagon => _onNewWagon;

        [Header("Standard Anomalies")]

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
        [Header("Mega Anomalies")]
        [SerializeField] private SceneTransitionSequence[] megaAnomalySequences;
        [SerializeField, Range(0f, 1f)] private float unseenMegaAnomalyChance = 1.0f;

        private List<int> _unseenMegaAnomalies = new List<int>();
        private List<int> _seenMegaAnomalies = new List<int>();

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
            int index = GetNextIndex(
                megaAnomalySequences.Length,
                _unseenMegaAnomalies,
                _seenMegaAnomalies,
                unseenMegaAnomalyChance,
                () =>
                {
                    SaveManager.Save.Session.SeenMegaAnomalies = _seenMegaAnomalies;
                    SaveManager.SaveGame();
                });

            if (index != -1)
            {
                await SceneTransitionController.Instance.Play(megaAnomalySequences[index]);
            }
            else
            {
                Debug.LogError("No Mega Anomaly sequences assigned!");
            }
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
        // Random and pools
        private void InitializeAnomalyPools()
        {
            SaveManager.Load();

            // »нициализаци€ пула обычных аномалий
            _seenAnomalies = SaveManager.Save.Session.SeenAnomalies ?? new List<int>();
            InitializePool(anomalyWagons.Length, _seenAnomalies, _unseenAnomalies, () => {
                SaveManager.Save.Session.SeenAnomalies = _seenAnomalies;
                SaveManager.SaveGame();
            });

            // »нициализаци€ пула мега-аномалий
            _seenMegaAnomalies = SaveManager.Save.Session.SeenMegaAnomalies ?? new List<int>();
            InitializePool(megaAnomalySequences.Length, _seenMegaAnomalies, _unseenMegaAnomalies, () => {
                SaveManager.Save.Session.SeenMegaAnomalies = _seenMegaAnomalies;
                SaveManager.SaveGame();
            });
        }

        private void InitializePool(int totalItems, List<int> seen, List<int> unseen, Action saveCallback)
        {
            unseen.Clear();
            for (int i = 0; i < totalItems; i++)
            {
                if (!seen.Contains(i))
                {
                    unseen.Add(i);
                }
            }

            // ≈сли все элементы просмотрены, сбрасываем прогресс
            if (unseen.Count == 0 && totalItems > 0)
            {
                unseen.Clear();
                for (int i = 0; i < totalItems; i++) unseen.Add(i);
                seen.Clear();
                saveCallback?.Invoke();
            }
        }

        private GameObject GetNextAnomalyPrefab()
        {
            int index = GetNextIndex(
                anomalyWagons.Length,
                _unseenAnomalies,
                _seenAnomalies,
                unseenAnomalyChance,
                () =>
                {
                    SaveManager.Save.Session.SeenAnomalies = _seenAnomalies;
                    SaveManager.SaveGame();
                });

            return index != -1 ? anomalyWagons[index] : null;
        }

        /// <summary>
        /// ”ниверсальный метод дл€ получени€ следующего случайного индекса (с учетом увиденных/неувиденных)
        /// </summary>
        private int GetNextIndex(int totalItems, List<int> unseen, List<int> seen, float unseenChance, Action saveCallback)
        {
            if (totalItems == 0) return -1;

            // Ќа вс€кий случай провер€ем, не пуст ли список (хот€ InitializePool должен это обрабатывать)
            if (unseen.Count == 0)
            {
                for (int i = 0; i < totalItems; i++) unseen.Add(i);
                seen.Clear();
                saveCallback?.Invoke();
            }

            bool pickUnseen = true;

            if (seen.Count > 0)
            {
                pickUnseen = Random.value <= unseenChance;
            }

            int selectedIndex = 0;

            if (pickUnseen && unseen.Count > 0)
            {
                int randomListIndex = Random.Range(0, unseen.Count);
                selectedIndex = unseen[randomListIndex];

                unseen.RemoveAt(randomListIndex);
                seen.Add(selectedIndex);

                saveCallback?.Invoke();
            }
            else if (seen.Count > 0)
            {
                int randomListIndex = Random.Range(0, seen.Count);
                selectedIndex = seen[randomListIndex];
            }

            return selectedIndex;
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