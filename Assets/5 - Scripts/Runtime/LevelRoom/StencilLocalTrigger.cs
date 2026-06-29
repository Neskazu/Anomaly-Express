using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StencilLocalTrigger : MonoBehaviour
{
    [SerializeField] private Renderer[] roomRenderers;

    [SerializeField] private Material visibleMaterial;

    [SerializeField] private Material hiddenMaterial;

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
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsLocalPlayer)
        {
            SetRoomMaterial(hiddenMaterial);
        }
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