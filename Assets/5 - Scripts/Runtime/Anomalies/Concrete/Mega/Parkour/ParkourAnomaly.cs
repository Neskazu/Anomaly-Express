using KinematicCharacterController;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using Player.Components;

namespace Anomalies
{
    public class ParkourAnomaly : AnomalyBase
    {
        [Header("Group Settings")]
        [SerializeField] private GameObject[] _suitcases;
        [SerializeField] private Transform _exitPoint;
        [SerializeField] private Transform _startPoint;

        [Header("Floating Settings")]
        [SerializeField] private float amplitude = 0.2f;
        [SerializeField] private float frequency = 1f;

        private Vector3[] _startPositions;
        private float[] _phaseOffsets;
        private MeshRenderer[] _renderers;

        private readonly NetworkVariable<ulong> _guideId = new(ulong.MaxValue);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            int count = _suitcases.Length;
            _startPositions = new Vector3[count];
            _phaseOffsets = new float[count];
            _renderers = new MeshRenderer[count];

            for (int i = 0; i < count; i++)
            {
                if (_suitcases[i] != null)
                {
                    _startPositions[i] = _suitcases[i].transform.position;
                    _phaseOffsets[i] = Random.Range(0f, 100f);
                    _renderers[i] = _suitcases[i].GetComponent<MeshRenderer>();
                }
            }

            _guideId.OnValueChanged += OnGuideChanged;
            OnAnomalyStateChanged += OnAnomalyToggled;

            if (_guideId.Value != ulong.MaxValue)
                OnGuideChanged(ulong.MaxValue, _guideId.Value);
        }

        private void Start()
        {
            if (IsServer) Activate();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _guideId.OnValueChanged -= OnGuideChanged;
            OnAnomalyStateChanged -= OnAnomalyToggled;
        }

        protected override void OnActivate()
        {
            if (!IsServer) return;

            var clients = NetworkManager.Singleton.ConnectedClientsList;
            if (clients.Count > 0)
            {
                _guideId.Value = clients[Random.Range(0, clients.Count)].ClientId;
            }
        }

        protected override void OnDeactivate()
        {
            if (IsServer) _guideId.Value = ulong.MaxValue;
        }
        private void OnAnomalyToggled()
        {
            RefreshVisibility();
            StartCoroutine(WaitAndChangeJumpState());
        }

        private void OnGuideChanged(ulong previousId, ulong newId)
        {
            RefreshVisibility();

            if (newId != ulong.MaxValue && NetworkManager.Singleton.LocalClientId == newId)
            {
                StartCoroutine(WaitAndTeleport());
            }
        }

        private IEnumerator WaitAndTeleport()
        {
            while (NetworkManager.Singleton.LocalClient?.PlayerObject == null)
                yield return null;

            var player = NetworkManager.Singleton.LocalClient.PlayerObject;
            var motor = player.GetComponentInChildren<KinematicCharacterMotor>();

            if (motor != null)
            {
                motor.SetPositionAndRotation(_exitPoint.position, _exitPoint.rotation);
                motor.BaseVelocity = Vector3.zero;
            }
            else
            {
                player.transform.position = _exitPoint.position;
            }
        }

        private IEnumerator WaitAndChangeJumpState()
        {
            while (NetworkManager.Singleton.LocalClient?.PlayerObject == null)
                yield return null;

            var player = NetworkManager.Singleton.LocalClient.PlayerObject;
            var jumpComp = player.GetComponent<PlayerJump>();

            if (jumpComp != null)
            {
                jumpComp.IsJumpEnabled = IsActive;
            }
        }

        protected override void OnUpdate()
        {
            for (int i = 0; i < _suitcases.Length; i++)
            {
                if (_suitcases[i] == null) continue;
                float offsetY = Mathf.Sin(Time.time * frequency + _phaseOffsets[i]) * amplitude;
                _suitcases[i].transform.position = _startPositions[i] + new Vector3(0, offsetY, 0);
            }
        }

        private void RefreshVisibility()
        {
            bool isGuide = NetworkManager.Singleton.LocalClientId == _guideId.Value;

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                    _renderers[i].enabled = !IsActive || isGuide;
            }
        }
        public void HandlePlayerFall(KinematicCharacterMotor motor)
        {
            bool isGuide = NetworkManager.Singleton.LocalClientId == _guideId.Value;
            Transform targetPoint = isGuide ? _exitPoint : _startPoint;

            if (targetPoint != null)
            {
                motor.SetPositionAndRotation(targetPoint.position, targetPoint.rotation);
                motor.BaseVelocity = Vector3.zero;
            }
        }
    }
}