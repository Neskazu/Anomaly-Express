using DG.Tweening;
using Managers;
using Unity.Netcode;
using UnityEngine;
public class PlayerInteract : MonoBehaviour
{
    [Tooltip("Max interaction range")]
    [SerializeField] private float range = 2f;
    [Tooltip("Layer with all interactables")]
    [SerializeField] private LayerMask interactableLayerMask;
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private float interactDelay = 0.15f;
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
        if (target == null) return;

        playerAnimator?.TriggerInteract();

        DOVirtual.DelayedCall(interactDelay, () =>
        {
            if (this != null && target != null)
            {
                target.Interact(gameObject);
            }
        }).SetTarget(this);
    }
    private void OnDestroy()
    {
        Input.OnInteract -= HandleInteract;
    }
}
