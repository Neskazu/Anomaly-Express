using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Anomalies
{
    public class ParkourAnomaly : AnomalyBase
    {
        [Header("Group Settings")]
        [SerializeField] private GameObject[] _suitcases;
        [SerializeField] private Transform _exitPoint;

        [Header("Floating Settings")]
        public float amplitude = 0.2f;
        public float frequency = 1f;

        private Vector3[] _startPositions;
        private float[] _phaseOffsets;
        private MeshRenderer[] _renderers; 

        private NetworkVariable<ulong> _guideId = new NetworkVariable<ulong>(999);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            int count = _suitcases.Length;
            _startPositions = new Vector3[count];
            _phaseOffsets = new float[count];
            _renderers = new MeshRenderer[count];

            for (int i = 0; i < count; i++)
            {
                _startPositions[i] = _suitcases[i].transform.position;
                _phaseOffsets[i] = Random.Range(0f, 100f);
                _renderers[i] = _suitcases[i].GetComponent<MeshRenderer>();
            }

            OnAnomalyStateChanged += RefreshVisibility;
        }

        protected override void OnActivate()
        {
            if (!IsServer) return;

            var clients = NetworkManager.Singleton.ConnectedClientsList;
            if (clients.Count > 0)
            {
                _guideId.Value = clients[Random.Range(0, clients.Count)].ClientId;
                TeleportGuideClientRpc(_guideId.Value);
            }
        }

        [ClientRpc]
        private void TeleportGuideClientRpc(ulong targetId)
        {
            if (NetworkManager.Singleton.LocalClientId == targetId)
            {
                var player = NetworkManager.Singleton.LocalClient.PlayerObject;
                if (player.TryGetComponent<CharacterController>(out var cc)) cc.enabled = false;
                player.transform.position = _exitPoint.position;
                if (cc != null) cc.enabled = true;
            }
        }

        protected override void OnUpdate()
        {
            for (int i = 0; i < _suitcases.Length; i++)
            {
                float offsetY = Mathf.Sin(Time.time * frequency + _phaseOffsets[i]) * amplitude;
                _suitcases[i].transform.position = _startPositions[i] + new Vector3(0, offsetY, 0);
            }
        }

        private void RefreshVisibility()
        {
            bool isGuide = NetworkManager.Singleton.LocalClientId == _guideId.Value;

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null) continue;

                if (IsActive)
                    _renderers[i].enabled = isGuide;
                else
                    _renderers[i].enabled = true;
            }
        }

        protected override void OnDeactivate()
        {
            if (IsServer) _guideId.Value = 999;
            RefreshVisibility();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            OnAnomalyStateChanged -= RefreshVisibility;
        }
    }
}