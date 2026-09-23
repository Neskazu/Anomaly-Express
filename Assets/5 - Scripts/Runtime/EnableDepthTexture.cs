using UnityEngine;

[RequireComponent(typeof(Camera))]
public class EnableDepthTexture : MonoBehaviour
{
    private void OnEnable()
    {
        Camera cam = GetComponent<Camera>();

        cam.depthTextureMode |= DepthTextureMode.Depth;

    }
}