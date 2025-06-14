using System.Collections.Generic;
using Attributes;
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
            _initial = new Vector2[targets.Length];

            for (var i = 0; i < targets.Length; i++)
                _initial[i] = targets[i].anchoredPosition;

            if (_fitter) _fitter.enabled = false;
            if (_layoutGroup) _layoutGroup.enabled = false;
        }

        [Button]
        public void Show()
        {
            container.TryGetComponent(out _layoutGroup);
            container.TryGetComponent(out _fitter);

            gameObject.SetActive(true);

            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].DOAnchorPosX(_initial[i].x, duration)
                    .From(_initial[i] + Vector2.left * 150)
                    .SetEase(Ease.InOutSine)
                    .SetDelay(i * delay);

                int notMutable = i;

                targetsGroups[i].DOFade(1, duration)
                    .From(0)
                    .SetEase(Ease.InOutSine)
                    .SetDelay(i * delay)
                    .OnComplete(() => { targetsGroups[notMutable].blocksRaycasts = true; });
            }
        }

        [Button]
        public void Hide()
        {
            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].DOAnchorPosX(_initial[i].x + 150, duration)
                    .From(_initial[i])
                    .SetEase(Ease.InOutSine)
                    .SetDelay(i * delay);

                targetsGroups[i].blocksRaycasts = false;

                targetsGroups[i].DOFade(0, duration)
                    .From(1)
                    .SetEase(Ease.InOutSine)
                    .SetDelay(i * delay);
            }
        }

#if UNITY_EDITOR
        [Button("Set Targets")]
        private void SetTargets_Editor()
        {
            if (!container)
            {
                Debug.LogWarning($"Missing container");
                return;
            }


            targets = GetComponentsInDirectChildren<RectTransform>(container).ToArray();
            targetsGroups = new CanvasGroup[targets.Length];

            for (int i = 0; i < targets.Length; i++)
                targets[i].TryGetComponent(out targetsGroups[i]);
        }

        private static List<T> GetComponentsInDirectChildren<T>(Transform parent) where T : Component
        {
            List<T> results = new List<T>();

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