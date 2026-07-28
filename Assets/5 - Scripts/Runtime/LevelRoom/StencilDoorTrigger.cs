using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StencilDoorTrigger : MonoBehaviour
{
    [SerializeField] private Renderer[] roomRenderers;
    [SerializeField] private Material visibleMaterial;
    [SerializeField] private Material hiddenMaterial;
    [SerializeField] private GameObject colliders;
    private void Awake()
    {
        SetRoomMaterial(hiddenMaterial);
    }

    private void OnTriggerEnter(Collider other)
    {
        var netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsLocalPlayer)
        {
            SetRoomMaterial(visibleMaterial);
            SetColliders(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsLocalPlayer)
        {
            Vector3 directionToPlayer = other.transform.position - transform.position;

            float side = Vector3.Dot(transform.forward, directionToPlayer);

            if (side < 0)
            {
                SetRoomMaterial(visibleMaterial);
                SetColliders(true);
            }
            else
            {
                SetRoomMaterial(hiddenMaterial);
                SetColliders(false);
            }
        }
    }
    private void SetColliders(bool isEnable)
    {
        colliders.SetActive(isEnable);
    }
    private void SetRoomMaterial(Material targetMaterial)
    {
        if (targetMaterial == null) return;

        foreach (var rend in roomRenderers)
        {
            if (rend == null) continue;
            rend.sharedMaterial = targetMaterial;
        }
    }
}