using UnityEngine;

public class CreditsScroller : MonoBehaviour
{
    [SerializeField] private RectTransform credits;
    [SerializeField] private float speed = 60f;

    private void Update()
    {
        credits.anchoredPosition += Vector2.up * speed * Time.deltaTime;
    }
}