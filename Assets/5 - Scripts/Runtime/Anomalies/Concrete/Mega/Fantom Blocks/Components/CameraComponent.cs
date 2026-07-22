using UnityEngine;

namespace Anomalies
{
    public class CameraComponent : MonoBehaviour, IInteractable
    {
        [SerializeField] private ToggleSensorComponent[] sensors;

        public void Interact(GameObject interactor)
        {
            foreach (var sensor in sensors)
            {
                sensor.Set(true);
            }
        }
    }
}