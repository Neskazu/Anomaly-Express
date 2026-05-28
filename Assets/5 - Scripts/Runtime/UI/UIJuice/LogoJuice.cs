using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class LogoJuice : MonoBehaviour
{

    [SerializeField] private UISoundPlayer audioPlayer;
    [Header("Target")]
    [SerializeField] private RectTransform target;

    [Header("Tween")]
    [SerializeField] private float pressScale = 0.92f;
    [SerializeField] private float punchDuration = 0.12f;
    [SerializeField] private Ease punchEase = Ease.OutQuad;

    [Header("Secret click combo")]
    [SerializeField] private int requiredClicks = 5;
    [SerializeField] private float clickWindow = 1.0f;
    [SerializeField] private UnityEvent onFiveFastClicks;

    private Button button;
    private Vector3 startScale;
    private readonly Queue<float> clicks = new Queue<float>();
    private DG.Tweening.Tween scaleTween;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (target == null)
            target = GetComponent<RectTransform>();

        startScale = target.localScale;
    }

    private void OnEnable()
    {
        button.onClick.AddListener(HandleClick);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(HandleClick);
        scaleTween?.Kill();
    }

    private void HandleClick()
    {
        PlayPressTween();
        RegisterClick();
    }

    private void PlayPressTween()
    {
        scaleTween?.Kill();

        scaleTween = target
            .DOScale(startScale * pressScale, punchDuration * 0.5f)
            .SetEase(punchEase)
            .OnComplete(() =>
            {
                scaleTween = target
                    .DOScale(startScale, punchDuration * 0.5f)
                    .SetEase(Ease.OutBack);
            });
    }

    private void RegisterClick()
    {
        float now = Time.unscaledTime;
        clicks.Enqueue(now);

        while (clicks.Count > 0 && now - clicks.Peek() > clickWindow)
            clicks.Dequeue();

        if (clicks.Count >= requiredClicks)
        {
            clicks.Clear();
            audioPlayer?.PlaySecret();
            onFiveFastClicks?.Invoke();
        }
    }
}