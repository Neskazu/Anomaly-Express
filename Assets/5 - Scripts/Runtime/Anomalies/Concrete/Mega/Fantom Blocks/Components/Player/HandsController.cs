using Managers;
using Network.Players;
using R3;
using Unity.Netcode;
using UnityEngine;

namespace Anomalies
{
    public class HandsController : MonoBehaviour
    {
        private static PlayerDataProvider Players => MultiplayerManager.Players;

        [SerializeField] private Transform handsRoot;
        [SerializeField] private GameObject[] personalPrefabs;
        [SerializeField] private HandsAnimations handsAnimation;

        private readonly ReactiveProperty<bool> active = new();
        private readonly ReactiveProperty<bool> photoMode = new();

        public ReadOnlyReactiveProperty<bool> Active => active;
        public ReadOnlyReactiveProperty<bool> PhotoMode => photoMode;

        public void Start()
        {
            var hands = CreateHands();
            var phone = hands.GetComponentInChildren<PhoneController>();

            handsAnimation.Setup(this, hands);
            phone.Setup(this);
        }

        private GameObject CreateHands()
        {
            var owner = NetworkManager.Singleton.LocalClientId;
            var data = Players.Get(owner);

            foreach (Transform child in handsRoot)
            {
                Destroy(child.gameObject);
            }

            var handInstance = Instantiate(personalPrefabs[data.CharacterId], handsRoot);

            handInstance.transform.localPosition = Vector3.zero;
            handInstance.transform.localRotation = Quaternion.identity;
            handInstance.transform.localScale = Vector3.one;

            return handInstance;
        }

        public void UpdatePhoneState(bool showed)
        {
            active.Value = showed;
        }

        public void UpdatePhotoModeState(bool enable)
        {
            photoMode.Value = enable;
        }
    }
}