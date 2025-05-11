using DG.Tweening;
using UnityEngine;

namespace UI
{
    public class UiTransition : MonoBehaviour
    {
        [SerializeField] private RectTransform container;

        private Sequence sequence;

        public void Awake()
        {
            sequence = DOTween.Sequence();
        }

        public void Start()
        {
            foreach (RectTransform child in container.transform)
            {
                child.DOAnchorPos(child.anchoredPosition + Vector2.up * 0.5f, 0.3f);
            }
        }
    }
}