using Controls;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputActionReference action;

    [Header("Settings")]
    [SerializeField] private float range = 2f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float interactDelay = 0.15f;

    [Header("Notification")]
    [SerializeField] private PlayerAnimator playerAnimator;

    private RaycastHit hit;

    void Start()
    {
        InputManager.Singleton
            .Subscribe(action, ReactiveInputPhase.Started, Interact)
            .AddTo(this);
    }

    private void Interact()
    {
        if (!Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, range, interactableLayer))
        {
            return;
        }

        var target = hit.collider.GetComponent<IInteractable>();
        if (target == null)
        {
            return;
        }

        playerAnimator?.TriggerInteract();

        DOVirtual.DelayedCall(interactDelay, () =>
        {
            if (this)
            {
                target?.Interact(gameObject);
            }
        }).SetTarget(this);
    }
}