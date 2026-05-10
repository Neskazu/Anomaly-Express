using KinematicCharacterController;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using Player.Components;

namespace Anomalies
{
    public class SplitControlAnomaly : AnomalyBase
    {
        
        [SerializeField]private Transform StartPoint;

        public static bool IsSplitActive { get; private set; } = false;
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            OnAnomalyStateChanged += OnAnomalyToggled;
        }
        public override void OnNetworkDespawn()
        {
            base.OnNetworkSpawn();
            OnAnomalyStateChanged -= OnAnomalyToggled;
        }
        private void Start()
        {
            if (IsServer) Activate();
        }
        protected override void OnActivate()
        {
            IsSplitActive = true;
        }

        protected override void OnDeactivate()
        {
            IsSplitActive = false;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            IsSplitActive = false;
        }
        private void OnAnomalyToggled()
        {
            StartCoroutine(WaitAndChangeJumpState());
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
        public void HandlePlayerFall(KinematicCharacterMotor motor)
        {
            if (StartPoint != null)
            {
                motor.SetPositionAndRotation(StartPoint.position, StartPoint.rotation);
                motor.BaseVelocity = Vector3.zero;
            }
        }
    }
}