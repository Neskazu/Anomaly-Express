using UnityEngine;
using DG.Tweening;

public class WagonNumberAnimate : MonoBehaviour
{
    [SerializeField] private MeshRenderer targetRenderer;
    [SerializeField] private string propertyName = "_BaseMap";
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private float stepY = -0.25f;
    [SerializeField] private int fullSpins = 4;
    [Header("Calibration")]
    [SerializeField] private float startXOffset = -0.05f;

    private Material _material;
    private int _propID;

    private void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<MeshRenderer>();
        _material = targetRenderer.material;
        _propID = Shader.PropertyToID(propertyName);
        
    }
    public void SetWagon(int targetIndex)
    {
        _material.SetTextureOffset(_propID, new Vector2(startXOffset, stepY * targetIndex));
    }

    public void PlayScroll(int targetIndex)
    {
        _material.DOKill();
        float currentY = _material.GetTextureOffset(_propID).y;
        float currentNormalizedY = Mathf.Repeat(currentY, 1.0f);
        float targetNormalizedY = Mathf.Repeat(targetIndex * stepY, 1.0f);
        float distanceToTarget = currentNormalizedY - targetNormalizedY;
        if (distanceToTarget <= 0)
        {
            distanceToTarget += 1.0f;
        }
        float totalScroll = (fullSpins * 1.0f) + distanceToTarget;
        float finalValue = currentY - totalScroll;
        _material.DOOffset(new Vector2(startXOffset, finalValue), _propID, duration)
            .SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                _material.SetTextureOffset(_propID, new Vector2(startXOffset, targetNormalizedY));
            });
    }
}