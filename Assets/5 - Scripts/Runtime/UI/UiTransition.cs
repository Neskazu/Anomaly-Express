using System;
using System.Collections.Generic;
using Attributes;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public enum TransitionDirection
    {
        LeftToRight,
        TopToBottom
    }

    public class UiTransition : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform container;
        [SerializeField] private RectTransform[] targets;
        [SerializeField] private CanvasGroup[] targetsGroups;

        [Header("Settings")]
        [SerializeField] private bool playOnStart = false;
        [SerializeField] private TransitionDirection direction = TransitionDirection.LeftToRight;
        [SerializeField] private float offsetDistance = 150f;
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private float delay = 0.03f;

        private VerticalLayoutGroup _layoutGroup;
        private ContentSizeFitter _fitter;
        private LayoutElement _layoutElement;

        private Vector2[] _initial;
        private bool _isPrepared;
        private bool _isPreparing;
        private int _transitionSequence;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void Start()
        {
            if (playOnStart)
            {
                Show().Forget();
            }
            else if (gameObject.activeInHierarchy)
            {
                PrepareAsync().Forget();
            }
        }

        private void EnsureInitialized()
        {
            if (!container)
            {
                container = GetComponent<RectTransform>();
            }

            if (container)
            {
                if (!_layoutGroup) container.TryGetComponent(out _layoutGroup);
                if (!_fitter) container.TryGetComponent(out _fitter);
            }

            if (!_layoutElement) TryGetComponent(out _layoutElement);

            ValidateTargets();
        }

        private void ValidateTargets()
        {
            if (targets == null || targets.Length == 0)
            {
                if (container)
                {
                    targets = GetComponentsInDirectChildren<RectTransform>(container).ToArray();
                }
            }

            if (targets == null || targets.Length == 0)
                return;

            if (targetsGroups == null || targetsGroups.Length != targets.Length)
            {
                targetsGroups = new CanvasGroup[targets.Length];
            }

            for (int i = 0; i < targets.Length; i++)
            {
                if (!targets[i]) continue;

                if (!targetsGroups[i])
                {
                    if (!targets[i].TryGetComponent(out targetsGroups[i]))
                    {
                        targetsGroups[i] = targets[i].gameObject.AddComponent<CanvasGroup>();
                    }
                }
            }
        }

        private async UniTask PrepareAsync()
        {
            if (_isPrepared)
                return;

            if (_isPreparing)
            {
                while (_isPreparing)
                    await UniTask.Yield();
                return;
            }

            _isPreparing = true;

            try
            {
                await PrepareInternal();
            }
            finally
            {
                _isPreparing = false;
            }
        }

        private async UniTask PrepareInternal()
        {
            EnsureInitialized();

            bool wasActive = gameObject.activeSelf;

            if (!wasActive)
            {
                SetTargetsAlpha(0f, false);
                gameObject.SetActive(true);
            }
            else
            {
                SetTargetsAlpha(1f, true);
            }

            if (_layoutGroup) _layoutGroup.enabled = true;
            if (_fitter) _fitter.enabled = true;

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i])
                    LayoutRebuilder.ForceRebuildLayoutImmediate(targets[i]);
            }

            Canvas.ForceUpdateCanvases();
            if (container)
                LayoutRebuilder.ForceRebuildLayoutImmediate(container);

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            Canvas.ForceUpdateCanvases();
            if (container)
                LayoutRebuilder.ForceRebuildLayoutImmediate(container);

            _initial = new Vector2[targets.Length];
            for (int i = 0; i < targets.Length; i++)
            {
                if (!targets[i]) continue;
                _initial[i] = targets[i].anchoredPosition;
            }

            if (_fitter) _fitter.enabled = false;
            if (_layoutGroup) _layoutGroup.enabled = false;

            if (!wasActive)
            {
                gameObject.SetActive(false);
            }
            else
            {
                SetTargetsAlpha(1f, true);
            }

            _isPrepared = true;
        }

        private Vector2 GetShowStartOffset()
        {
            return direction == TransitionDirection.LeftToRight
                ? new Vector2(-offsetDistance, 0f)
                : new Vector2(0f, offsetDistance);
        }

        private Vector2 GetHideEndOffset()
        {
            return direction == TransitionDirection.LeftToRight
                ? new Vector2(offsetDistance, 0f)
                : new Vector2(0f, -offsetDistance);
        }

        private void SetTargetsAlpha(float alpha, bool blocksRaycasts)
        {
            if (targetsGroups == null) return;

            for (int i = 0; i < targetsGroups.Length; i++)
            {
                if (targetsGroups[i] != null)
                {
                    targetsGroups[i].alpha = alpha;
                    targetsGroups[i].blocksRaycasts = blocksRaycasts;
                }
            }
        }

        public void Toggle()
        {
            if (gameObject.activeSelf)
                Hide().Forget();
            else
                Show().Forget();
        }

        [Button]
        public async UniTask Show()
        {
            EnsureInitialized();

            _transitionSequence++;
            int currentSequence = _transitionSequence;

            await PrepareAsync();

            Vector2 startOffset = GetShowStartOffset();

            for (int i = 0; i < targets.Length; i++)
            {
                if (!targets[i]) continue;

                targets[i].DOKill();
                targets[i].anchoredPosition = _initial[i] + startOffset;

                if (targetsGroups[i] != null)
                {
                    targetsGroups[i].DOKill();
                    targetsGroups[i].alpha = 0f;
                    targetsGroups[i].blocksRaycasts = false;
                }
            }

            if (_layoutElement)
                _layoutElement.ignoreLayout = false;

            gameObject.SetActive(true);

            for (int i = 0; i < targets.Length; i++)
            {
                if (!targets[i]) continue;

                int index = i;
                float itemDelay = index * delay;

                targets[index]
                    .DOAnchorPos(_initial[index], duration)
                    .SetEase(Ease.InOutSine)
                    .SetDelay(itemDelay);

                CanvasGroup group = targetsGroups[index];
                if (group != null)
                {
                    group
                        .DOFade(1f, duration)
                        .SetEase(Ease.InOutSine)
                        .SetDelay(itemDelay)
                        .OnComplete(() =>
                        {
                            if (currentSequence != _transitionSequence)
                                return;

                            group.blocksRaycasts = true;
                        });
                }
            }

            float totalDuration = duration + delay * Mathf.Max(0, targets.Length - 1);

            await UniTask.Delay(
                TimeSpan.FromSeconds(totalDuration),
                cancellationToken: this.GetCancellationTokenOnDestroy());

            if (currentSequence != _transitionSequence)
                return;

            for (int i = 0; i < targets.Length; i++)
            {
                if (!targets[i]) continue;

                targets[i].anchoredPosition = _initial[i];

                if (targetsGroups[i] != null)
                {
                    targetsGroups[i].alpha = 1f;
                    targetsGroups[i].blocksRaycasts = true;
                }
            }
        }

        [Button]
        public async UniTask Hide()
        {
            EnsureInitialized();

            _transitionSequence++;
            int currentSequence = _transitionSequence;

            await PrepareAsync();

            if (_layoutElement)
                _layoutElement.ignoreLayout = true;

            Vector2 endOffset = GetHideEndOffset();

            for (int i = 0; i < targets.Length; i++)
            {
                if (!targets[i]) continue;

                int index = i;
                float itemDelay = index * delay;

                targets[index].DOKill();
                targets[index]
                    .DOAnchorPos(_initial[index] + endOffset, duration)
                    .SetEase(Ease.InOutSine)
                    .SetDelay(itemDelay);

                CanvasGroup group = targetsGroups[index];
                if (group != null)
                {
                    group.DOKill();
                    group.blocksRaycasts = false;
                    group
                        .DOFade(0f, duration)
                        .SetEase(Ease.InOutSine)
                        .SetDelay(itemDelay);
                }
            }

            float totalDuration = duration + delay * Mathf.Max(0, targets.Length - 1);

            await UniTask.Delay(
                TimeSpan.FromSeconds(totalDuration),
                cancellationToken: this.GetCancellationTokenOnDestroy());

            if (currentSequence != _transitionSequence)
                return;

            gameObject.SetActive(false);
        }

        private static List<T> GetComponentsInDirectChildren<T>(Transform parent) where T : Component
        {
            List<T> results = new();
            foreach (Transform child in parent)
            {
                T component = child.GetComponent<T>();
                if (component != null)
                    results.Add(component);
            }
            return results;
        }

#if UNITY_EDITOR
        [Button("Set Targets")]
        private void SetTargets_Editor()
        {
            if (!container)
            {
                container = GetComponent<RectTransform>();
            }

            targets = GetComponentsInDirectChildren<RectTransform>(container).ToArray();
            targetsGroups = new CanvasGroup[targets.Length];

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i])
                    targets[i].TryGetComponent(out targetsGroups[i]);
            }
        }
#endif
    }
}