using System;
using Nac.Extensions;
using Nac.Singleton;
using R3;
using Unity.Netcode;

namespace Scene
{
    public class NetworkTransitionObserver : NetworkService<NetworkTransitionObserver>
    {
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                SceneTransitionManager.Instance.PreLoading
                    .Subscribe(PreLoadingRpc)
                    .AddTo(this);
            }
            else
            {
                NetworkManager.Singleton.SceneManager.OnSceneEvent += OnClientSceneEvent;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (NetworkManager.Singleton && NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnClientSceneEvent;
            }
        }

        private void OnClientSceneEvent(SceneEvent sceneEvent)
        {
            switch (sceneEvent.SceneEventType)
            {
                case SceneEventType.LoadComplete:
                case SceneEventType.SynchronizeComplete:
                    if (sceneEvent.ClientId != NetworkManager.Singleton.LocalClientId)
                        break;

                    SceneTransitionWindow.Instance.Hide();
                    break;
                default:
                    return;
            }
        }

        [Rpc(SendTo.NotMe, RequireOwnership = true)]
        private void PreLoadingRpc()
        {
            SceneTransitionWindow.Instance.Show();
        }
    }
}