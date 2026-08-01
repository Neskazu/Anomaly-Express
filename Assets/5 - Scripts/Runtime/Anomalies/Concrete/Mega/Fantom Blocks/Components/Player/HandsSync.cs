using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

namespace Anomalies
{
    public class HandsSync : NetworkBehaviour
    {
        [Rpc(SendTo.NotMe)]
        public void SyncPhoneRpc(bool active, bool photo, RpcParams rpcParams = default)
        {
            var senderClientId = rpcParams.Receive.SenderClientId;

            if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(senderClientId, out var networkClient))
            {
                return;
            }

            var go = networkClient.PlayerObject?.gameObject;
            var hands = go?.GetComponentInChildren<HandsComponent>();

            if (!hands)
            {
                return;
            }

            hands.Phone.gameObject.SetActive(active);
            Debug.Log(active);

            var offset = photo ? hands.PhonePhotoOffset : hands.PhoneRestOffset;
            hands.Phone.DOLocalMove(offset, 1.0f);
        }
    }
}