using Cysharp.Threading.Tasks;
using DG.Tweening;
using Player.Components;
using Scene;
using UnityEngine;

namespace MegaAnomalies
{
    public class AnomalyTransitionAnimation : MonoBehaviour
    {
        [SerializeField] private PlayerCamera playerCameraController;
        [SerializeField] private GameObject crack;
        [SerializeField] private GameObject hole;

        private Transform camTransform;
        private bool x = true;

        public static AnomalyTransitionAnimation Instance { get; private set; }

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
            playerCameraController.enabled = false;

            var seq = DOTween.Sequence();

            // left
            seq.Append(camTransform.DOLocalRotate(new Vector3(5, -30, -5), 0.7f).SetEase(Ease.OutSine));
            seq.AppendInterval(0.6f);

            // right
            seq.Append(camTransform.DOLocalRotate(new Vector3(5, 40, 5), 0.8f).SetEase(Ease.InOutSine));
            seq.AppendInterval(0.4f)
                .AppendCallback(delegate { crack.SetActive(true); });

            // down
            seq.Append(camTransform.DOLocalRotate(new Vector3(75, 0, 0), 0.5f).SetEase(Ease.InCubic));
            seq.AppendInterval(0.4f)
                .AppendCallback(delegate
                {
                    hole.SetActive(true);
                    crack.SetActive(false);
                });
            seq.AppendInterval(0.1f);

            // fall
            seq.Append(camTransform.DOMoveY(camTransform.position.y - 1f, 0.3f).SetEase(Ease.InExpo));
            seq.Join(camTransform.DOLocalRotate(new Vector3(-10, 0, 0), 0.2f).SetEase(Ease.OutSine))
                .AppendCallback(delegate { SceneTransitionWindow.Instance.Show().Forget(); });

            seq.Append(camTransform.DOMoveY(camTransform.position.y - 2f, 0.3f).SetEase(Ease.InExpo));
            seq.Join(camTransform.DOLocalRotate(new Vector3(-70, 0, 0), 0.2f).SetEase(Ease.InOutSine));

            await seq.Play().ToUniTask();
 
            camTransform.DOShakePosition(0.5f, 0.7f, 20, 90);
            camTransform.DOShakeRotation(0.5f, 10f, 20, 90);
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