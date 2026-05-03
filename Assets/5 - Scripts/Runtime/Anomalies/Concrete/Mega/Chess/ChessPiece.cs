using UnityEngine;
using System.Collections;

public class ChessPiece : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] _renderers;
    [SerializeField] private Color _highlightColor = Color.yellow;

    private Color[] _originalColors;
    private bool _isSelected;

    private void Awake()
    {
        _originalColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _originalColors[i] = _renderers[i].material.color;
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i].material.color = selected ? _highlightColor : _originalColors[i];
        }
    } 

    public void MoveTo(Vector3 targetPos)
    {
        StopAllCoroutines();
        StartCoroutine(MoveRoutine(targetPos));
    }

    private IEnumerator MoveRoutine(Vector3 target)
    {
        float elapsed = 0;
        float duration = 0.5f;
        Vector3 startPos = transform.localPosition;

        while (elapsed < duration)
        {
            transform.localPosition = Vector3.Lerp(startPos, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = target;
    }

    public void PlayCaptureAndDestroy()
    {
        StopAllCoroutines();
        StartCoroutine(CaptureRoutine());
    }

    private IEnumerator CaptureRoutine()
    {
        float elapsed = 0;
        float duration = 0.4f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / duration);
            transform.localPosition += Vector3.up * Time.deltaTime * 2f;
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}