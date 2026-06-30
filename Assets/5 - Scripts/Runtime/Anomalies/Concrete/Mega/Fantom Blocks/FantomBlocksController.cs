using Managers;
using Player.Components;
using R3;
using Unity.Netcode;

namespace Anomalies
{
    public class FantomBlocksController : NetworkBehaviour
    {
        private void Awake()
        {
            GameManager.Instance.OnLocalPlayerSpawn
                .Subscribe(EnableJump)
                .AddTo(this);
        }

        private void EnableJump(ulong id)
        {
            var player = NetworkManager.Singleton.LocalClient.PlayerObject;
            var jumpComp = player.GetComponent<PlayerJump>();

            if (jumpComp != null)
            {
                jumpComp.IsJumpEnabled = true;
            }
        }
    }
}