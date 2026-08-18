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
        [SerializeField] private TransitionDirection direction = TransitionDirection.LeftToRight;
        [SerializeField] private float offsetDistance = 150f;
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private float delay = 0.03f;

        private VerticalLayoutGroup _layoutGroup;
        private ContentSizeFitter _fitter;
        private Vector2[] _initial;

        private LayoutElement _layoutElement;
        private int _transitionSequence;

        private void Awake()
        {
            container.TryGetComponent(out _layoutGroup);
            container.TryGetComponent(out _fitter);
            TryGetComponent(out _layoutElement);

            Prepare().Forget();
        }

        private async UniTask Prepare()
        {
            if (_initial != null)
                return;

            LayoutRebuilder.ForceRebuildLayoutImmediate(container);
            await UniTask.DelayFrame(1);

            _initial = new Vector2[targets.Length];

            for (int i = 0; i < targets.Length; i++)
            {
                _initial[i] = targets[i].anchoredPosition;
            }

            if (_fitter)
                _fitter.enabled = false;

            if (_layoutGroup)
                _layoutGroup.enabled = false;
        }

        private Vector2 GetShowStartOffset()
        {
            return direction == TransitionDirection.LeftToRight
                ? new Vector2(-offsetDistance, 0)
                : new Vector2(0, offsetDistance);
        }

        private Vector2 GetHideEndOffset()
        {
            return direction == TransitionDirection.LeftToRight
                ? new Vector2(offsetDistance, 0)
                : new Vector2(0, -offsetDistance);
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
            if (_layoutElement) _layoutElement.ignoreLayout = false;
            _transitionSequence++;
            await Prepare();

            // Если объект был полностью выключен, мгновенно сдвигаем его на стартовую позицию,
            // чтобы анимация красиво выезжала, а не начиналась из центра
            if (!gameObject.activeSelf)
            {
                Vector2 startOffset = GetShowStartOffset();
                for (int i = 0; i < targets.Length; i++)
                {
                    targets[i].anchoredPosition = _initial[i] + startOffset;
                    if (targetsGroups[i] != null)
                        targetsGroups[i].alpha = 0;
                }
            }

            gameObject.SetActive(true);

            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].DOKill();

                // Анимируем ИЗ текущей позиции В изначальную (без использования .From)
                targets[i]
                    .DOAnchorPos(_initial[i], duration)
                    .SetEase(Ease.InOutSine)
                    .SetDelay(i * delay);

                if (targetsGroups[i] == null)
                    continue;

                targetsGroups[i].DOKill();

                int index = i;

                targetsGroups[i]
                    .DOFade(1, duration)
                    .SetEase(Ease.InOutSine)
                    .SetDelay(i * delay)
                    .OnComplete(() =>
                    {
                        targetsGroups[index].blocksRaycasts = true;

                        LayoutRebuilder.ForceRebuildLayoutImmediate(
                            targetsGroups[index].transform as RectTransform);
                    });
            }
        }

        [Button]
        public async UniTask Hide()
        {
            if (_layoutElement) _layoutElement.ignoreLayout = true;
            _transitionSequence++;
            int currentSequence = _transitionSequence;

            await Prepare();

            Vector2 endOffset = GetHideEndOffset();

            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].DOKill();

                targets[i]
                    .DOAnchorPos(_initial[i] + endOffset, duration)
                    .SetEase(Ease.InOutSine)
                    .SetDelay(i * delay);

                if (targetsGroups[i] == null)
                    continue;

                targetsGroups[i].DOKill();
                targetsGroups[i].blocksRaycasts = false;

                targetsGroups[i]
                    .DOFade(0, duration)
                    .SetEase(Ease.InOutSine)
                    .SetDelay(i * delay);
            }

            float totalDuration = duration + delay * Mathf.Max(0, targets.Length - 1);

            await UniTask.Delay(TimeSpan.FromSeconds(totalDuration));

            // Выключаем объект ТОЛЬКО если с момента начала Hide() не запустили Show()
            if (currentSequence == _transitionSequence)
            {
                gameObject.SetActive(false);
            }
        }

#if UNITY_EDITOR
        [Button("Set Targets")]
        private void SetTargets_Editor()
        {
            if (!container)
            {
                Debug.LogWarning("Missing container");
                return;
            }

            targets = GetComponentsInDirectChildren<RectTransform>(container).ToArray();
            targetsGroups = new CanvasGroup[targets.Length];

            for (int i = 0; i < targets.Length; i++)
                targets[i].TryGetComponent(out targetsGroups[i]);
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
#endif
    }
}