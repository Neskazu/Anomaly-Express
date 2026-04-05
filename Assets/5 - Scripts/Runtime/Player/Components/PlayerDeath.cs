using Cysharp.Threading.Tasks;
using Managers;
using Mono;
using Network;
using Sessions;
using UI.Base;
using Unity.Netcode;
using UnityEngine;

namespace Player.Components
{
    public class PlayerDeath : MonoBehaviour
    {
        [SerializeField] private GameObject humanModel;
        [SerializeField] private GameObject ghostModel;

        [SerializeField] private LayerMask humanLayers;
        [SerializeField] private LayerMask ghostLayers;

        [SerializeField] private NetworkObject networkObject;

        private bool _isDead;

        private IWindow DeathScreen =>
            UI.DeathScreen.Instance;

        private void Awake()
        {
            MultiplayerManager.Players.OnUpdated += ChangePlayerState;

            var data = MultiplayerManager.Players.Get(networkObject.OwnerClientId);
            _isDead = data.IsDead;
        }

        public void OnDestroy()
        {
            MultiplayerManager.Players.OnUpdated -= ChangePlayerState;
        }

        private async void ChangePlayerState(PlayerData playerData)
        {
            if (playerData.ClientId != networkObject.OwnerClientId)
                return;

            if (playerData.IsDead == _isDead)
                return;

            _isDead = playerData.IsDead;

            if (networkObject.IsOwner)
            {
                if (playerData.IsDead)
                    await OwnerDeath();
                else
                    await OwnerRevive();
            }
            else
            {
                if (playerData.IsDead)
                    Death();
                else
                    Revive();
            }
        }

        private async UniTask OwnerDeath()
        {
            await DeathScreen.Show();

            if (Camera.main)
                Camera.main.cullingMask = ghostLayers;
            Death();

            await DeathScreen.Hide();
        }

        private async UniTask OwnerRevive()
        {
            await DeathScreen.Show();

            if (Camera.main)
                Camera.main.cullingMask = humanLayers;
            Revive();

            await DeathScreen.Hide();
        }

        private void Death()
        {
            humanModel.SetActive(false);
            ghostModel.SetActive(true);
        }

        private void Revive()
        {
            humanModel.SetActive(true);
            ghostModel.SetActive(false);
        }
    }
}