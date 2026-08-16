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
        private List<string> _unseenAnomalies = new List<string>();
        private List<string> _seenAnomalies = new List<string>();

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

        private List<string> _unseenMegaAnomalies = new List<string>();
        private List<string> _seenMegaAnomalies = new List<string>();

        private List<string> _allAnomalyIds = new List<string>();
        private List<string> _allMegaAnomalyIds = new List<string>();

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

            string nextMegaId = GetNextId(_allMegaAnomalyIds, _unseenMegaAnomalies, _seenMegaAnomalies, unseenMegaAnomalyChance, () =>
            {
                SaveManager.Save.Session.SeenMegaAnomalies = _seenMegaAnomalies;
                SaveManager.SaveGame();
            });

            SceneTransitionSequence selectedSequence = null;
            foreach (var seq in megaAnomalySequences)
            {
                if (seq.name == nextMegaId)
                {
                    selectedSequence = seq;
                    break;
                }
            }

            if (selectedSequence != null)
            {
                await SceneTransitionManager.Instance.Play(selectedSequence);
            }
            else
            {
                Debug.LogError("No Mega Anomaly sequence found or assigned!");
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
        private void InitializeAnomalyPools()
        {
            SaveManager.Load();

            _allAnomalyIds.Clear();
            foreach (var prefab in anomalyWagons)
            {
                if (prefab.TryGetComponent(out AnomalyBase anomaly))
                    _allAnomalyIds.Add(anomaly.Id);
                else
                    Debug.LogWarning($"Prefab {prefab.name} does not have AnomalyBase!");
            }

            _allMegaAnomalyIds.Clear();
            foreach (var seq in megaAnomalySequences)
            {
                _allMegaAnomalyIds.Add(seq.name);
            }

            _seenAnomalies = SaveManager.Save.Session.SeenAnomalies ?? new List<string>();
            InitializePool(_allAnomalyIds, _seenAnomalies, _unseenAnomalies, () => {
                SaveManager.Save.Session.SeenAnomalies = _seenAnomalies;
                SaveManager.SaveGame();
            });

            _seenMegaAnomalies = SaveManager.Save.Session.SeenMegaAnomalies ?? new List<string>();
            InitializePool(_allMegaAnomalyIds, _seenMegaAnomalies, _unseenMegaAnomalies, () => {
                SaveManager.Save.Session.SeenMegaAnomalies = _seenMegaAnomalies;
                SaveManager.SaveGame();
            });
        }

        private void InitializePool(List<string> allIds, List<string> seen, List<string> unseen, Action saveCallback)
        {
            seen.RemoveAll(id => !allIds.Contains(id));

            unseen.Clear();
            foreach (var id in allIds)
            {
                if (!seen.Contains(id))
                {
                    unseen.Add(id);
                }
            }
            if (unseen.Count == 0 && allIds.Count > 0)
            {
                unseen.AddRange(allIds);
                seen.Clear();
                saveCallback?.Invoke();
            }
        }

        private GameObject GetNextAnomalyPrefab()
        {
            string nextId = GetNextId(_allAnomalyIds, _unseenAnomalies, _seenAnomalies, unseenAnomalyChance, () =>
            {
                SaveManager.Save.Session.SeenAnomalies = _seenAnomalies;
                SaveManager.SaveGame();
            });

            if (string.IsNullOrEmpty(nextId)) return null;

            // ������� ������ �� ����������� ID
            foreach (var prefab in anomalyWagons)
            {
                if (prefab.TryGetComponent(out AnomalyBase anomaly) && anomaly.Id == nextId)
                {
                    return prefab;
                }
            }
            return null;
        }
        private string GetNextId(List<string> allIds, List<string> unseen, List<string> seen, float unseenChance, Action saveCallback)
        {
            if (allIds.Count == 0) return null;

            if (unseen.Count == 0)
            {
                unseen.AddRange(allIds);
                seen.Clear();
                saveCallback?.Invoke();
            }

            bool pickUnseen = true;

            if (seen.Count > 0)
            {
                pickUnseen = Random.value <= unseenChance;
            }

            string selectedId = null;

            if (pickUnseen && unseen.Count > 0)
            {
                int randomListIndex = Random.Range(0, unseen.Count);
                selectedId = unseen[randomListIndex];

                unseen.RemoveAt(randomListIndex);
                seen.Add(selectedId);

                saveCallback?.Invoke();
            }
            else if (seen.Count > 0)
            {
                int randomListIndex = Random.Range(0, seen.Count);
                selectedId = seen[randomListIndex];
            }

            return selectedId;
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
        //helper good to be refactored
        public int GetExpectedNextIndex(bool playerWentBackward)
        {
            bool isMistake = (currentWagonHasAnomaly && !playerWentBackward) || (!currentWagonHasAnomaly && playerWentBackward);

            if (isMistake)
            {
                return 0;
            }
            else
            {
                return _currentWagonIndex + 1;
            }
        }
    }
}