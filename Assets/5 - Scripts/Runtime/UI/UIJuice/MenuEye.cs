using System;
using DG.Tweening;
using R3;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Menu
{
    public class MenuEye : MonoBehaviour
    {
        [Header("Hierarchy References")]
        [SerializeField] private Transform driftPivot;
        [SerializeField] private Transform shakePivot;
        [SerializeField] private Transform eyeVisual;

        [Header("Flight Points")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform animationPoint;
        [SerializeField] private Transform endPoint;

        [Header("Chance")]
        [Range(0f, 1f)]
        [SerializeField] private float activationChance = 0.35f;

        [Header("Flight Settings")]
        [SerializeField] private float flyInDuration = 1.0f;
        [SerializeField] private float flyOutDuration = 1.0f;
        [SerializeField] private float activeTime = 4.0f;

        [Header("Floating Settings")]
        [SerializeField] private float driftRadius = 33f;
        [SerializeField] private float minDriftDuration = 0.15f;
        [SerializeField] private float maxDriftDuration = 2f;
        [SerializeField] private float jitterStrength = 0.5f;
        [SerializeField] private float minDriftPause = 0.5f;
        [SerializeField] private float maxDriftPause = 1.5f;

        [Header("Rotation Settings")]
        [SerializeField] private float maxLookAngle = 40.0f;
        [SerializeField] private float minRotationDuration = 0.2f;
        [SerializeField] private float maxRotationDuration = 0.6f;

        [Header("Logic")]
        [Range(0, 1)][SerializeField] private float spinChance = 0.15f;
        [SerializeField] private float minInterval = 0.5f;
        [SerializeField] private float maxInterval = 2.5f;

        private Sequence _driftSeq;
        private Sequence _rotSeq;
        private Sequence _mainSeq;
        private DG.Tweening.Tween _jitterTween;
        private IDisposable _logicSub;

        private Vector3 _animationPos;
        private bool _isLeaving;

        private void Start()
        {
            if (Random.value > activationChance)
            {
                gameObject.SetActive(false);
                return;
            }

            if (startPoint == null || animationPoint == null || endPoint == null)
                return;

            _animationPos = animationPoint.position;

            driftPivot.position = startPoint.position;
            eyeVisual.localRotation = Quaternion.identity;

            PlayIntro();
        }

        private void PlayIntro()
        {
            KillAllLocalTweens();

            _mainSeq = DOTween.Sequence();

            _mainSeq.Append(driftPivot.DOMove(_animationPos, flyInDuration)
                .SetEase(Ease.OutCubic));

            _mainSeq.AppendCallback(() =>
            {
                StartAnimationLogic();
            });

            _mainSeq.AppendInterval(activeTime);

            _mainSeq.AppendCallback(() =>
            {
                PlayOutro();
            });
        }

        private void StartAnimationLogic()
        {
            if (_isLeaving)
                return;

            if (shakePivot != null)
            {
                _jitterTween = shakePivot.DOShakePosition(1f, jitterStrength, 15)
                    .SetLoops(-1, LoopType.Restart)
                    .SetEase(Ease.Linear);
            }

            StartHugeDrift();
            ScheduleNextAction();
        }

        private void StartHugeDrift()
        {
            if (_isLeaving)
                return;

            _driftSeq?.Kill();
            _driftSeq = DOTween.Sequence();

            Vector3 targetPos = _animationPos + Random.insideUnitSphere * driftRadius;
            float duration = Random.Range(minDriftDuration, maxDriftDuration);
            float pause = Random.Range(minDriftPause, maxDriftPause);
            Ease easeType = duration < 0.4f ? Ease.OutExpo : Ease.InOutQuad;

            _driftSeq.Append(driftPivot.DOMove(targetPos, duration)
                .SetEase(easeType));

            _driftSeq.AppendInterval(pause);

            _driftSeq.OnComplete(StartHugeDrift);
        }

        private void ScheduleNextAction()
        {
            if (_isLeaving)
                return;

            _logicSub?.Dispose();

            _logicSub = Observable
                .Timer(TimeSpan.FromSeconds(Random.Range(minInterval, maxInterval)))
                .Subscribe(_ =>
                {
                    if (_isLeaving)
                        return;

                    PerformAction();
                    ScheduleNextAction();
                })
                .AddTo(this);
        }

        private void PerformAction()
        {
            if (_isLeaving)
                return;

            _rotSeq?.Kill();
            _rotSeq = DOTween.Sequence();

            bool isSpinning = Random.value < spinChance;
            float duration = Random.Range(minRotationDuration, maxRotationDuration);

            if (isSpinning)
            {
                float spin = (Random.value > 0.5f ? 720f : 1080f) * (Random.value > 0.5f ? 1 : -1);

                _rotSeq.Append(
                    eyeVisual.DOLocalRotate(new Vector3(0, 0, spin), duration * 3f, RotateMode.LocalAxisAdd)
                        .SetEase(Ease.OutBack, 1.5f));
            }
            else
            {
                Vector3 targetRot = new Vector3(
                    Random.Range(-maxLookAngle, maxLookAngle),
                    Random.Range(-maxLookAngle, maxLookAngle),
                    0f
                );

                _rotSeq.Append(
                    eyeVisual.DOLocalRotate(targetRot, duration)
                        .SetEase(Ease.OutExpo));
            }
        }

        private void PlayOutro()
        {
            _isLeaving = true;

            _logicSub?.Dispose();
            _driftSeq?.Kill();
            _rotSeq?.Kill();
            _jitterTween?.Kill();

            _mainSeq?.Kill();
            _mainSeq = DOTween.Sequence();

            _mainSeq.Append(driftPivot.DOMove(endPoint.position, flyOutDuration)
                .SetEase(Ease.InCubic));

            _mainSeq.OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }

        private void KillAllLocalTweens()
        {
            _mainSeq?.Kill();
            _driftSeq?.Kill();
            _rotSeq?.Kill();
            _jitterTween?.Kill();
            _logicSub?.Dispose();
        }

        private void OnDestroy()
        {
            KillAllLocalTweens();
        }
    }
}