using Controls;
using Managers;
using Nac;
using Network;
using R3;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Components
{
    public class PlayerPunch : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputActionReference action;

        [Header("Settings")]
        [SerializeField] private float cooldown;
        [SerializeField] private float range = 150;
        [SerializeField] private float force = 10;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private PlayerController player;
        [SerializeField] private NetworkObject networkObject;

        private RaycastHit _hit;
        private float _timer;

        private void Start()
        {
            PlayersManager.Instance.OnPlayerUpdated
                .Subscribe(ApplyPunch)
                .AddTo(this);

            InputManager.Singleton
                .Subscribe(action, ReactiveInputPhase.Started, Punch)
                .AddTo(this);
        }

        private void ApplyPunch(PlayerData playerData)
        {
            if (playerData.Owner != networkObject.OwnerClientId)
            {
                return;
            }

            player.PunchVelocity = playerData.Punch * force;
        }

        private void FixedUpdate()
        {
            if (_timer > 0)
            {
                _timer -= Time.fixedDeltaTime;
            }
        }

        private void Punch()
        {
            if (_timer > 0f || !Camera.main)
            {
                return;
            }

            _timer = cooldown;

            if (!Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out _hit, range, layerMask))
            {
                return;
            }

            var target = _hit.collider.GetComponent<NetworkObject>();
            GameManager.Instance.PunchPlayerServerRpc(target.OwnerClientId, target.transform.position - transform.position);
        }
    }
}