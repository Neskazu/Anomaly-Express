using UnityEngine;

public class WheelRotator : MonoBehaviour
{
    public Transform[] wheels;

    public float rotationSpeed = 25f;

    void Update()
    {
        float rotation = rotationSpeed * Time.deltaTime;

        foreach (Transform wheel in wheels)
        {
            if (wheel != null)
            {
                wheel.Rotate(rotation, 0f, 0f);
            }
        }
    }
}