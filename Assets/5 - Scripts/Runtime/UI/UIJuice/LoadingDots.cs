using UnityEngine;
using TMPro;

public class LoadingDots : MonoBehaviour
{
    public TMP_Text loadingText;
    public float interval = 0.5f;

    private float timer;
    private int dots;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0;
            dots++;

            if (dots > 3)
                dots = 0;

            loadingText.text = new string('.', dots);
        }
    }
}