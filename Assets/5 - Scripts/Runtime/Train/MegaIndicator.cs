using UnityEngine;

public class MegaIndicator : MonoBehaviour
{
    [SerializeField] private MeshRenderer targetRenderer;
    [SerializeField] private string propertyName = "_BaseMap";

    [Header("Offset Settings")]
    [SerializeField] private float startOffsetX = 0.925f;
    [SerializeField] private float stepX = 0.025f;
    [SerializeField] private float offsetY = 0f;

    private Material _material;
    private int _propID;

    private void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<MeshRenderer>();

        _material = targetRenderer.material;
        _propID = Shader.PropertyToID(propertyName);
    }

    public void SetIndicator(int completedMegas)
    {
        float targetX = Mathf.Clamp(startOffsetX + (completedMegas * stepX), startOffsetX, 1f);
        _material.SetTextureOffset(_propID, new Vector2(targetX, offsetY));
    }
}