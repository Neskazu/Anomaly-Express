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

            if (!IsServer) return;

            SceneTransitionManager.Instance.PreLoading
                .Subscribe(PreLoadingRpc)
                .AddTo(this);

            SceneTransitionManager.Instance.PostLoading
                .Subscribe(PostLoadingRpc)
                .AddTo(this);
        }

        [Rpc(SendTo.NotMe, RequireOwnership = true)]
        private void PreLoadingRpc()
        {
            SceneTransitionWindow.Instance.Show();
        }

        [Rpc(SendTo.NotMe, RequireOwnership = true)]
        private void PostLoadingRpc()
        {
            SceneTransitionWindow.Instance.Hide();
        }
    }
}