using Managers;
using Mono;
using Network;
using Sessions;
using Unity.Netcode;
using UnityEngine;

namespace Player.Components
{
    public class PlayerPunch : MonoBehaviour
    {
        [SerializeField] private float cooldown;
        [SerializeField] private float range = 150;
        [SerializeField] private float force = 10;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private PlayerController player;
        [SerializeField] private NetworkObject networkObject;

        private RaycastHit _hit;
        private float _timer;

        private InputManager Input
            => InputManager.Singleton;

        private void Start()
        {
            MultiplayerManager.Players.OnUpdated += ApplyPunch;
            Input.OnPunch += Punch;
        }

        private void ApplyPunch(PlayerData playerData)
        {
            if (playerData.ClientId != networkObject.OwnerClientId)
                return;

            player.PunchVelocity = playerData.Punch * force;
        }

        private void FixedUpdate()
        {
            if (_timer > 0)
                _timer -= Time.fixedDeltaTime;
        }

        private void Punch()
        {
            if (_timer > 0f || !Camera.main)
                return;

            _timer = cooldown;

            if (!Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out _hit, range, layerMask))
                return;

            var target = _hit.collider.GetComponent<NetworkObject>();
            GameManager.Instance.PunchPlayerServerRpc(target.OwnerClientId, target.transform.position - transform.position);
        }

        private void OnDestroy()
        {
            MultiplayerManager.Players.OnUpdated -= ApplyPunch;
            Input.OnPunch -= Punch;
        }
    }
}