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
                    var target = RpcTarget.Single(sceneEvent.ClientId, RpcTargetUse.Temp);
                    PostLoadingRpc(target);
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

        [Rpc(SendTo.SpecifiedInParams, RequireOwnership = true)]
        private void PostLoadingRpc(RpcParams rpcParams = default)
        {
            SceneTransitionWindow.Instance.Hide();
        }
    }
}