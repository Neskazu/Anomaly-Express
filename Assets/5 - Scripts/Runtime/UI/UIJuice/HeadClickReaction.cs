using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class HeadClickReaction : MonoBehaviour
{
    [SerializeField] private VrmEmotionController emotionController;
    [SerializeField] private string reactionId = "nose";
    [SerializeField] private int clicksToTrigger = 3;
    [SerializeField] private float clickWindow = 2f;

    private readonly Queue<float> clickTimes = new Queue<float>();

    private void Reset()
    {
        emotionController = GetComponentInParent<VrmEmotionController>();
    }

    private void OnMouseDown()
    {
        if (emotionController == null || emotionController.IsReactionActive)
            return;

        float now = Time.time;
        clickTimes.Enqueue(now);

        while (clickTimes.Count > 0 && now - clickTimes.Peek() > clickWindow)
            clickTimes.Dequeue();

        if (clickTimes.Count >= clicksToTrigger)
        {
            clickTimes.Clear();
            emotionController.TriggerReaction(reactionId);
        }
    }
}