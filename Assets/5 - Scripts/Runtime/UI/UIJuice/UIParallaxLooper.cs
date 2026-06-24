using UnityEngine;

[System.Serializable]
public class ParallaxUILevel
{
    public RectTransform a;
    public RectTransform b;
    public float speed = 50f;
}

public class UIParallaxLooper : MonoBehaviour
{
    public ParallaxUILevel far;
    public ParallaxUILevel mid;
    public ParallaxUILevel near;

    private void Update()
    {
        MoveLevel(far);
        MoveLevel(mid);
        MoveLevel(near);
    }

    void MoveLevel(ParallaxUILevel level)
    {
        if (level == null || level.a == null || level.b == null)
            return;

        float delta = level.speed * Time.deltaTime;

        level.a.anchoredPosition += Vector2.left * delta;
        level.b.anchoredPosition += Vector2.left * delta;

        float widthA = level.a.rect.width;
        float widthB = level.b.rect.width;

        if (level.a.anchoredPosition.x <= -widthA)
        {
            level.a.anchoredPosition = new Vector2(
                level.b.anchoredPosition.x + widthB,
                level.a.anchoredPosition.y
            );
        }

        if (level.b.anchoredPosition.x <= -widthB)
        {
            level.b.anchoredPosition = new Vector2(
                level.a.anchoredPosition.x + widthA,
                level.b.anchoredPosition.y
            );
        }
    }
}