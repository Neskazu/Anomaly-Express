using Controls;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Player.Components;
using Scene;
using Unity.Netcode;
using UnityEngine;

namespace MegaAnomalies
{
    public class AnomalyTransitionAnimation : NetworkBehaviour
    {
        public static AnomalyTransitionAnimation Instance { get; private set; }

        [SerializeField] private PlayerCamera playerCamera;
        [SerializeField] private GameObject crack;
        [SerializeField] private GameObject hole;

        [Header("Inputs")]
        [SerializeField] private InputPreset cutsceneInputPreset;
        [SerializeField] private InputPreset defaultInputPreset;

        private Transform camTransform;
        private bool x = true;

        private void Start()
        {
            camTransform = Camera.main.transform;

            if (Instance != null)
            {
                Destroy(this);
            }

            Instance = this;
        }

        public async UniTask Play()
        {
            playerCamera.enabled = false;

            InputManager.Singleton.ActivatePreset(cutsceneInputPreset);

            var seq = DOTween.Sequence();

            // left
            seq.Append(camTransform.DOLocalRotate(new Vector3(5, -30, -5), 0.7f).SetEase(Ease.OutSine));
            seq.AppendInterval(0.6f);

            // right
            seq.Append(camTransform.DOLocalRotate(new Vector3(5, 40, 5), 0.8f).SetEase(Ease.InOutSine));
            seq.AppendInterval(0.4f)
                .AppendCallback(ShowCrackRpc);

            // down
            seq.Append(camTransform.DOLocalRotate(new Vector3(75, 0, 0), 0.5f).SetEase(Ease.InCubic));
            seq.AppendInterval(0.4f)
                .AppendCallback(ShowHoleRpc);
            seq.AppendInterval(0.1f);

            // fall
            seq.Append(camTransform.DOMoveY(camTransform.position.y - 1f, 0.3f).SetEase(Ease.InExpo));
            seq.Join(camTransform.DOLocalRotate(new Vector3(-10, 0, 0), 0.2f).SetEase(Ease.OutSine))
                .AppendCallback(delegate { SceneTransitionWindow.Instance.Show().Forget(); });

            seq.Append(camTransform.DOMoveY(camTransform.position.y - 2f, 0.3f).SetEase(Ease.InExpo));
            seq.Join(camTransform.DOLocalRotate(new Vector3(-70, 0, 0), 0.2f).SetEase(Ease.InOutSine));

            await seq.Play();

            camTransform.DOShakePosition(0.5f, 0.7f, 20, 90);
            camTransform.DOShakeRotation(0.5f, 10f, 20, 90);

            await UniTask.WaitForSeconds(1);

            if (playerCamera)
            {
                playerCamera.enabled = true;
            }

            InputManager.Singleton.ActivatePreset(defaultInputPreset);
        }

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        private void ShowCrackRpc()
        {
            crack.SetActive(true);
            hole.SetActive(false);
        }

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        private void ShowHoleRpc()
        {
            crack.SetActive(false);
            hole.SetActive(true);
        }

#if UNITY_EDITOR || DEBUG
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T) && x)
            {
                Play().Forget();
                x = false;
            }
        }
#endif
    }
}