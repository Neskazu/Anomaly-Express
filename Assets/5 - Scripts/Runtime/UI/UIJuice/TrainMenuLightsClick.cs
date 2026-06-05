using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TrainMenuLightsClick : MonoBehaviour
{
    [SerializeField] private TrainMenuLights trainMenuLights;
    [SerializeField] private int wagonIndex = 1;

    private void OnMouseDown()
    {
        if (trainMenuLights == null)
            return;

        switch (wagonIndex)
        {
            case 1:
                trainMenuLights.ToggleWagon1();
                break;
            case 2:
                trainMenuLights.ToggleWagon2();
                break;
            case 3:
                trainMenuLights.ToggleWagon3();
                break;
        }
    }
}