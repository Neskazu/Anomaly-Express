using DG.Tweening;
using UnityEngine;

public abstract class VrmReaction : ScriptableObject
{
    public string id = "reaction";
    public VrmEmotionController.Emotion emotion = VrmEmotionController.Emotion.Angry;

    public abstract DG.Tweening.Tween CreateTween(VrmEmotionController controller);
}