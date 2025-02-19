using System;
using Cysharp.Threading.Tasks;
using Managers;
using Network;
using UI.Base;
using Unity.Netcode;
using Unity.VisualScripting;
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

        private IView DeathScreen => 
            UI.DeathScreen.Instance;

        private static PlayerDataProvider Players =>
            MultiplayerManager.Instance.Players;

        private void Awake()
        {
            Players.OnChange += ChangePlayerState;
        }

        public void OnDestroy()
        {
            Players.OnChange -= ChangePlayerState;
        }

        private async void ChangePlayerState(PlayerData playerData)
        {
            if (playerData.ClientId != networkObject.OwnerClientId)
                return;

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