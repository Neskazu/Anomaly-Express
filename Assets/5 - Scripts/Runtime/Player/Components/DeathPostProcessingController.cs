using UnityEngine;
using UnityEngine.Rendering;
using Unity.Netcode;
using Nac;
using R3;
using Network;

namespace Train.Effects
{
    public class DeathPostProcessingController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Volume mainVolume;

        [Header("Profiles")]
        [SerializeField] private VolumeProfile normalProfile;
        [SerializeField] private VolumeProfile deathProfile;

        private void Start()
        {
            PlayersManager.Instance.OnPlayerUpdated
                .Subscribe(OnPlayerUpdatedData)
                .AddTo(this);
        }

        private void OnPlayerUpdatedData(PlayerData data)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
                return;

            if (data.Owner == NetworkManager.Singleton.LocalClientId)
            {
                if (mainVolume != null)
                {
                    mainVolume.profile = data.IsDead ? deathProfile : normalProfile;
                }
            }
        }
    }
}