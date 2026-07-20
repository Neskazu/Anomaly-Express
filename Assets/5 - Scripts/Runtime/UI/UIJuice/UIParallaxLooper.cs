using UnityEngine;

[System.Serializable]
public class ParallaxUILevel
{
    public RectTransform[] elements;
    public float speed = 50f;
}

public class UIParallaxLooper : MonoBehaviour
{
    [SerializeField] private ParallaxUILevel far;
    [SerializeField] private ParallaxUILevel mid;
    [SerializeField] private ParallaxUILevel near;

    private RectTransform _canvasRect;

    private void Start()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            _canvasRect = canvas.rootCanvas.transform as RectTransform;
        }
        else
        {
            foreach (var el in far.elements)
            {
                if (el != null)
                {
                    _canvasRect = el.GetComponentInParent<Canvas>()?.rootCanvas.transform as RectTransform;
                    break;
                }
            }
        }
    }

    private void Update()
    {
        if (_canvasRect == null) return;

        MoveLevel(far);
        MoveLevel(mid);
        MoveLevel(near);
    }

    private void MoveLevel(ParallaxUILevel level)
    {
        if (level == null || level.elements == null || level.elements.Length < 2)
            return;

        float delta = level.speed * Time.deltaTime;

        foreach (var element in level.elements)
        {
            if (element == null) continue;
            element.anchoredPosition += Vector2.left * delta;
        }

        Vector3[] currentCorners = new Vector3[4];
        Vector3[] rightMostCorners = new Vector3[4];

        RectTransform rightMost = null;
        float maxRightX = float.MinValue;

        foreach (var element in level.elements)
        {
            if (element == null) continue;
            element.GetWorldCorners(currentCorners);

            if (currentCorners[2].x > maxRightX)
            {
                maxRightX = currentCorners[2].x;
                rightMost = element;
                System.Array.Copy(currentCorners, rightMostCorners, 4);
            }
        }

        Vector3[] canvasCorners = new Vector3[4];
        _canvasRect.GetWorldCorners(canvasCorners);
        float screenLeftEdgeWorld = canvasCorners[0].x; 

        foreach (var element in level.elements)
        {
            if (element == null) continue;
            element.GetWorldCorners(currentCorners);

            if (currentCorners[2].x <= screenLeftEdgeWorld)
            {
                RectTransform parentRect = element.parent as RectTransform;
                if (parentRect == null) continue;

                Vector3 rightMostRightEdgeWorld = rightMostCorners[2];
                Vector3 localPosOfRightEdge = parentRect.InverseTransformPoint(rightMostRightEdgeWorld);

                float pivotOffset = element.rect.width * element.pivot.x;
                float newLocalX = localPosOfRightEdge.x + pivotOffset;

                element.anchoredPosition = new Vector2(newLocalX, element.anchoredPosition.y);

                rightMost = element;
                element.GetWorldCorners(rightMostCorners);
            }
        }
    }
}