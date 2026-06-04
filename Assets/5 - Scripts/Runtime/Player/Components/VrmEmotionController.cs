using System.Collections.Generic;
using DG.Tweening;
using UniVRM10;
using UnityEngine;

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

    [Header("Reactions")]
    [SerializeField] private string defaultReactionId = "nose";
    [SerializeField] private List<VrmReaction> reactions = new List<VrmReaction>();

    private Vector3 currentHeadOffset;
    private Vector3 currentNeckOffset;
    internal void SetHeadReactionEuler(Vector3 euler) => currentHeadOffset = euler;
    internal void SetNeckReactionEuler(Vector3 euler) => currentNeckOffset = euler;

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
    private Emotion reactionEmotion = Emotion.Angry;
    private bool reactionActive;

    private DG.Tweening.Tween blinkLoopTween;
    private DG.Tweening.Tween blinkTween;
    private DG.Tweening.Tween reactionTween;

    private Quaternion headRestRotation;
    private Quaternion neckRestRotation;
    private bool restPoseCached;

    public bool IsReactionActive => reactionActive;

    private readonly Dictionary<string, VrmReaction> reactionLookup = new Dictionary<string, VrmReaction>();

    private void Awake()
    {
        if (vrm10 == null)
            vrm10 = GetComponentInChildren<Vrm10Instance>(true);

        RebuildReactionLookup();
    }

    private void OnValidate()
    {
        RebuildReactionLookup();
    }

    private void RebuildReactionLookup()
    {
        reactionLookup.Clear();

        for (int i = 0; i < reactions.Count; i++)
        {
            var r = reactions[i];
            if (r == null || string.IsNullOrWhiteSpace(r.id))
                continue;

            reactionLookup[r.id] = r;
        }
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

        blinkLoopTween = null;
        blinkTween = null;
        reactionTween = null;
    }

    private void LateUpdate()
    {
        UpdateTargets(Time.deltaTime);
        ApplyFace();

        if (reactionActive)
        {
            if (headBone != null)
                headBone.localRotation = headRestRotation * Quaternion.Euler(currentHeadOffset);
            if (neckBone != null)
                neckBone.localRotation = neckRestRotation * Quaternion.Euler(currentNeckOffset);
        }
    }

    public void SetBaseEmotion(Emotion emotion)
    {
        baseEmotion = emotion;
    }

    public void TriggerReaction(string reactionId)
    {
        if (reactionActive)
            return;

        if (!reactionLookup.TryGetValue(reactionId, out var reaction) || reaction == null)
            return;

        reactionEmotion = reaction.emotion;

        reactionTween?.Kill();
        reactionTween = reaction.CreateTween(this);
    }

    public void TriggerNoseReaction()
    {
        TriggerReaction(defaultReactionId);
    }

    internal void SetReactionActive(bool active)
    {
        reactionActive = active;
    }

    internal void SetHeadRotation(Quaternion localRotation)
    {
        if (headBone != null)
            headBone.localRotation = localRotation;
    }

    internal void SetNeckRotation(Quaternion localRotation)
    {
        if (neckBone != null)
            neckBone.localRotation = localRotation;
    }

    internal Quaternion HeadRestRotation => headRestRotation;
    internal Quaternion NeckRestRotation => neckRestRotation;

    internal Emotion ReactionEmotion => reactionEmotion;

    private void UpdateTargets(float dt)
    {
        FaceWeights baseTarget = EmotionToWeights(baseEmotion);
        targetFace = reactionActive ? FaceWeights.Lerp(baseTarget, EmotionToWeights(reactionEmotion), 0.8f) : baseTarget;

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
                return new FaceWeights { Happy = 0.8f, Relaxed = 0.18f };
            case Emotion.Angry:
                return new FaceWeights { Angry = 0.8f };
            case Emotion.Sad:
                return new FaceWeights { Sad = 0.8f };
            default:
                return new FaceWeights();
        }
    }
}