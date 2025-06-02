using UnityEngine;
using Managers;
using Unity.Netcode;
public class PlayerInteract : MonoBehaviour
{
    [Tooltip("Max interaction range")]
    [SerializeField] private float range = 2f;
    [Tooltip("Layer with all interactables")]
    [SerializeField] private LayerMask interactableLayerMask;
    private InputManager Input
            => InputManager.Singleton;
    private RaycastHit hit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Input.OnInteract += HandleInteract;
    }

    private void HandleInteract()
    {
        if (!Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, range, interactableLayerMask))
            return;

        var target = hit.collider.GetComponent<IInteractable>();
        target.Interact(gameObject);
    }
    private void OnDestroy()
    {
        Input.OnInteract -= HandleInteract;
    }
}
