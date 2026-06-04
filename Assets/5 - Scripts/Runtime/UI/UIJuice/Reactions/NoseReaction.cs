using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(menuName = "VRM/Reactions/Nose Reaction")]
public class NoseReaction : VrmReaction
{
    [Header("Head")]
    public float downPitch = 12f;
    public float neckDownPitch = 5f;

    [Header("Shake")]
    public float yawAngle = 14f;
    public float neckYawShare = 0.45f;

    [Header("Timing")]
    public float downTime = 0.12f;
    public float shakeTime = 0.12f;
    public float returnTime = 0.14f;

    public override DG.Tweening.Tween CreateTween(VrmEmotionController controller)
    {
        controller.SetReactionActive(true);

        Vector3 headEuler = Vector3.zero;
        Vector3 neckEuler = Vector3.zero;

        Sequence seq = DOTween.Sequence();

        //pitch
        seq.Append(DOTween.To(() => headEuler, x =>
        {
            headEuler = x;
            Apply(controller, headEuler, neckEuler);
        }, new Vector3(downPitch, 0f, 0f), downTime).SetEase(Ease.OutSine));

        seq.Join(DOTween.To(() => neckEuler, x =>
        {
            neckEuler = x;
            Apply(controller, headEuler, neckEuler);
        }, new Vector3(neckDownPitch, 0f, 0f), downTime).SetEase(Ease.OutSine));
        //1 left right
        seq.Append(DOTween.To(() => headEuler, x =>
        {
            headEuler = x;
            Apply(controller, headEuler, neckEuler);
        }, new Vector3(downPitch, -yawAngle, 0f), shakeTime).SetEase(Ease.InOutSine));

        seq.Join(DOTween.To(() => neckEuler, x =>
        {
            neckEuler = x;
            Apply(controller, headEuler, neckEuler);
        }, new Vector3(neckDownPitch, -yawAngle * neckYawShare, 0f), shakeTime).SetEase(Ease.InOutSine));

        seq.Append(DOTween.To(() => headEuler, x =>
        {
            headEuler = x;
            Apply(controller, headEuler, neckEuler);
        }, new Vector3(downPitch, yawAngle, 0f), shakeTime).SetEase(Ease.InOutSine));

        seq.Join(DOTween.To(() => neckEuler, x =>
        {
            neckEuler = x;
            Apply(controller, headEuler, neckEuler);
        }, new Vector3(neckDownPitch, yawAngle * neckYawShare, 0f), shakeTime).SetEase(Ease.InOutSine));
        //2 left right
        seq.Append(DOTween.To(() => headEuler, x =>
        {
            headEuler = x;
            Apply(controller, headEuler, neckEuler);
        }, new Vector3(downPitch, -yawAngle, 0f), shakeTime).SetEase(Ease.InOutSine));

        seq.Join(DOTween.To(() => neckEuler, x =>
        {
            neckEuler = x;
            Apply(controller, headEuler, neckEuler);
        }, new Vector3(neckDownPitch, -yawAngle * neckYawShare, 0f), shakeTime).SetEase(Ease.InOutSine));

        seq.Append(DOTween.To(() => headEuler, x =>
        {
            headEuler = x;
            Apply(controller, headEuler, neckEuler);
        }, new Vector3(downPitch, yawAngle, 0f), shakeTime).SetEase(Ease.InOutSine));

        seq.Join(DOTween.To(() => neckEuler, x =>
        {
            neckEuler = x;
            Apply(controller, headEuler, neckEuler);
        }, new Vector3(neckDownPitch, yawAngle * neckYawShare, 0f), shakeTime).SetEase(Ease.InOutSine));
        //return
        seq.Append(DOTween.To(() => headEuler, x =>
        {
            headEuler = x;
            Apply(controller, headEuler, neckEuler);
        }, Vector3.zero, returnTime).SetEase(Ease.InSine));

        seq.Join(DOTween.To(() => neckEuler, x =>
        {
            neckEuler = x;
            Apply(controller, headEuler, neckEuler);
        }, Vector3.zero, returnTime).SetEase(Ease.InSine));

        seq.OnComplete(() => controller.SetReactionActive(false));
        seq.OnKill(() => controller.SetReactionActive(false));

        return seq;
    }

    private void Apply(VrmEmotionController controller, Vector3 headEuler, Vector3 neckEuler)
    {
        controller.SetHeadReactionEuler(headEuler);
        controller.SetNeckReactionEuler(neckEuler);
    }
}