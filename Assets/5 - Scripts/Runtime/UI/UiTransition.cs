using System;
using System.Collections.Generic;
using Attributes;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UiTransition : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform container;
        [SerializeField] private RectTransform[] targets;
        [SerializeField] private CanvasGroup[] targetsGroups;

        [Header("Settings")]
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private float delay = 0.03f;

        private VerticalLayoutGroup _layoutGroup;
        private ContentSizeFitter _fitter;
        private Vector2[] _initial;

        private void Awake()
        {
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
            await Prepare();

            gameObject.SetActive(true);

            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].DOKill();

                targets[i]
                    .DOAnchorPosX(_initial[i].x, duration)
                    .From(_initial[i] - Vector2.right * 150)
                    .SetEase(Ease.InOutSine)
                    .SetDelay(i * delay);

                if (targetsGroups[i] == null)
                    continue;

                targetsGroups[i].DOKill();

                int index = i;

                targetsGroups[i]
                    .DOFade(1, duration)
                    .From(0)
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
            await Prepare();

            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].DOKill();

                targets[i]
                    .DOAnchorPosX(_initial[i].x + 150, duration)
                    .From(_initial[i])
                    .SetEase(Ease.InOutSine)
                    .SetDelay(i * delay);

                if (targetsGroups[i] == null)
                    continue;

                targetsGroups[i].DOKill();
                targetsGroups[i].blocksRaycasts = false;

                targetsGroups[i]
                    .DOFade(0, duration)
                    .From(1)
                    .SetEase(Ease.InOutSine)
                    .SetDelay(i * delay);
            }

            float totalDuration = duration + delay * Mathf.Max(0, targets.Length - 1);

            await UniTask.Delay(TimeSpan.FromSeconds(totalDuration));

            gameObject.SetActive(false);
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