using UnityEngine;

namespace Parallax
{
    public class ParallaxUI : MonoBehaviour
    {
        [SerializeField] private RectTransform[] targets;

        [SerializeField] private Vector2 direction;
        [SerializeField] private float speed;

        private void FixedUpdate()
        {
            for (var i = 0; i < targets.Length; i++)
            {
                if (targets[i].anchoredPosition.x <= -Screen.width)
                {
                    var last = i == 0 ? targets[^1] : targets[i - 1];

                    var resetPosition = last.anchoredPosition;
                    resetPosition.x += targets[i].rect.width;

                    targets[i].anchoredPosition = resetPosition;
                }

                targets[i].anchoredPosition += direction * (speed * Time.deltaTime);
            }
        }
    }
}