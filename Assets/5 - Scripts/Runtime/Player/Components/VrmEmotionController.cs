using DG.Tweening;
using UniVRM10;
using UnityEngine;

[DisallowMultipleComponent]
public class VrmEmotionController : MonoBehaviour
{
    public enum Emotion
    {
        Neutral,
        Happy,
        Angry,
        Sad
    }

    [Header("VRM 1.0")]
    [SerializeField] private Vrm10Instance vrm10;
    [SerializeField] private Transform headBone;
    [SerializeField] private Transform neckBone;

    [Header("Blink")]
    [SerializeField] private bool enableBlink = true;
    [SerializeField] private Vector2 blinkDelayRange = new Vector2(2.5f, 5f);
    [SerializeField] private float blinkCloseTime = 0.07f;
    [SerializeField] private float blinkOpenTime = 0.10f;

    [Header("Emotion")]
    [SerializeField] private Emotion baseEmotion = Emotion.Neutral;
    [SerializeField] private float emotionLerpSpeed = 7f;

    [Header("Reaction")]
    [SerializeField] private float defaultReactionDuration = 0.85f;
    [SerializeField] private float reactionBlendInTime = 0.12f;
    [SerializeField] private float reactionBlendOutTime = 0.16f;

    [Header("Head Shake")]
    [SerializeField] private float headShakeAngle = 14f;
    [SerializeField] private float headShakeWaves = 3.5f;
    [SerializeField] private float neckShare = 0.45f;

    private struct FaceWeights
    {
        public float Happy;
        public float Angry;
        public float Sad;
        public float Relaxed;

        public static FaceWeights Lerp(FaceWeights a, FaceWeights b, float t)
        {
            t = Mathf.Clamp01(t);
            return new FaceWeights
            {
                Happy = Mathf.Lerp(a.Happy, b.Happy, t),
                Angry = Mathf.Lerp(a.Angry, b.Angry, t),
                Sad = Mathf.Lerp(a.Sad, b.Sad, t),
                Relaxed = Mathf.Lerp(a.Relaxed, b.Relaxed, t),
            };
        }
    }

    private FaceWeights currentFace;
    private FaceWeights targetFace;

    private float blinkValue;
    private float reactionMix;
    private Emotion reactionEmotion = Emotion.Angry;
    private bool reactionActive;

    private DG.Tweening.Tween blinkLoopTween;
    private DG.Tweening.Tween blinkTween;
    private DG.Tweening.Tween reactionTween;
    private DG.Tweening.Tween headShakeTween;

    private Quaternion headRestRotation;
    private Quaternion neckRestRotation;
    private bool restPoseCached;

    private float headShakeY;
    private float neckShakeY;

    private void Awake()
    {
        if (vrm10 == null)
            vrm10 = GetComponentInChildren<Vrm10Instance>(true);
    }

    private void OnEnable()
    {
        CacheRestPose();
        currentFace = EmotionToWeights(baseEmotion);
        targetFace = currentFace;

        if (enableBlink)
            StartBlinkLoop();
    }

    private void OnDisable()
    {
        blinkLoopTween?.Kill();
        blinkTween?.Kill();
        reactionTween?.Kill();
        headShakeTween?.Kill();

        blinkLoopTween = null;
        blinkTween = null;
        reactionTween = null;
        headShakeTween = null;
    }

    private void LateUpdate()
    {
        UpdateTargets(Time.deltaTime);
        ApplyFace();
        ApplyHeadPose();
    }

    public void SetBaseEmotion(Emotion emotion)
    {
        baseEmotion = emotion;
    }

    public void TriggerNoseReaction()
    {
        TriggerReaction(Emotion.Angry, defaultReactionDuration);
    }

    public void TriggerReaction(Emotion emotion, float duration)
    {
        reactionEmotion = emotion;

        reactionTween?.Kill();
        headShakeTween?.Kill();

        reactionTween = CreateReactionTween(duration);
        headShakeTween = CreateHeadShakeTween(duration, headShakeAngle);
    }

    private void UpdateTargets(float dt)
    {
        FaceWeights baseTarget = EmotionToWeights(baseEmotion);

        if (reactionActive)
        {
            FaceWeights reactionTarget = EmotionToWeights(reactionEmotion);
            targetFace = FaceWeights.Lerp(baseTarget, reactionTarget, reactionMix);
        }
        else
        {
            targetFace = baseTarget;
        }

        float t = 1f - Mathf.Exp(-emotionLerpSpeed * dt);
        currentFace = FaceWeights.Lerp(currentFace, targetFace, t);
    }

    private void ApplyFace()
    {
        if (vrm10 == null || vrm10.Runtime == null || vrm10.Runtime.Expression == null)
            return;

        var expr = vrm10.Runtime.Expression;

        expr.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.blink), blinkValue);
        expr.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.happy), currentFace.Happy);
        expr.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.angry), currentFace.Angry);
        expr.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.sad), currentFace.Sad);
        expr.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.relaxed), currentFace.Relaxed);
    }

    private void StartBlinkLoop()
    {
        blinkLoopTween?.Kill();
        ScheduleNextBlink();
    }

    private void ScheduleNextBlink()
    {
        if (!enableBlink)
            return;

        float delay = Random.Range(blinkDelayRange.x, blinkDelayRange.y);

        blinkLoopTween = DOVirtual.DelayedCall(delay, () =>
        {
            if (!reactionActive)
                StartBlinkTween();

            ScheduleNextBlink();
        });
    }

    private void StartBlinkTween()
    {
        blinkTween?.Kill();

        blinkTween = DOTween.Sequence()
            .Append(DOTween.To(() => blinkValue, x => blinkValue = x, 1f, blinkCloseTime).SetEase(Ease.InQuad))
            .Append(DOTween.To(() => blinkValue, x => blinkValue = x, 0f, blinkOpenTime).SetEase(Ease.OutQuad));
    }

    private DG.Tweening.Tween CreateReactionTween(float duration)
    {
        reactionActive = true;
        reactionMix = 0f;

        float inTime = Mathf.Min(reactionBlendInTime, duration);
        float outTime = Mathf.Min(reactionBlendOutTime, duration);
        float holdTime = Mathf.Max(0f, duration - inTime - outTime);

        return DOTween.Sequence()
            .Append(DOTween.To(() => reactionMix, x => reactionMix = x, 1f, inTime).SetEase(Ease.OutSine))
            .AppendInterval(holdTime)
            .Append(DOTween.To(() => reactionMix, x => reactionMix = x, 0f, outTime).SetEase(Ease.InSine))
            .OnComplete(() => reactionActive = false)
            .OnKill(() => reactionActive = false);
    }

    private DG.Tweening.Tween CreateHeadShakeTween(float duration, float angle)
    {
        if (headBone == null && neckBone == null)
            return null;

        headShakeY = 0f;
        neckShakeY = 0f;

        float part = Mathf.Max(0.01f, duration / 4f);

        return DOTween.Sequence()
            .Append(DOTween.To(() => headShakeY, x => headShakeY = x, angle, part).SetEase(Ease.OutSine))
            .Join(DOTween.To(() => neckShakeY, x => neckShakeY = x, angle * neckShare, part).SetEase(Ease.OutSine))
            .Append(DOTween.To(() => headShakeY, x => headShakeY = x, -angle, part).SetEase(Ease.InOutSine))
            .Join(DOTween.To(() => neckShakeY, x => neckShakeY = x, -angle * neckShare, part).SetEase(Ease.InOutSine))
            .Append(DOTween.To(() => headShakeY, x => headShakeY = x, angle, part).SetEase(Ease.InOutSine))
            .Join(DOTween.To(() => neckShakeY, x => neckShakeY = x, angle * neckShare, part).SetEase(Ease.InOutSine))
            .Append(DOTween.To(() => headShakeY, x => headShakeY = x, 0f, part).SetEase(Ease.InSine))
            .Join(DOTween.To(() => neckShakeY, x => neckShakeY = x, 0f, part).SetEase(Ease.InSine));
    }

    private void ApplyHeadPose()
    {
        if (!restPoseCached)
            return;

        if (headBone != null)
            headBone.localRotation = headRestRotation * Quaternion.Euler(0f, headShakeY, 0f);

        if (neckBone != null)
            neckBone.localRotation = neckRestRotation * Quaternion.Euler(0f, neckShakeY, 0f);
    }

    private void CacheRestPose()
    {
        if (restPoseCached)
            return;

        if (headBone != null)
            headRestRotation = headBone.localRotation;

        if (neckBone != null)
            neckRestRotation = neckBone.localRotation;

        restPoseCached = true;
    }

    private static FaceWeights EmotionToWeights(Emotion emotion)
    {
        switch (emotion)
        {
            case Emotion.Happy:
                return new FaceWeights
                {
                    Happy = 0.8f,
                    Relaxed = 0.18f
                };

            case Emotion.Angry:
                return new FaceWeights
                {
                    Angry = 0.8f
                };

            case Emotion.Sad:
                return new FaceWeights
                {
                    Sad = 0.8f
                };

            default:
                return new FaceWeights();
        }
    }
}