using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChessPiece : MonoBehaviour
{
    [Header("Highlight Settings")]
    [SerializeField] private MeshRenderer[] _renderers;
    [SerializeField] private Color _highlightColor = Color.yellow;
    [Header("Error Flash Settings")]
    [SerializeField] private Color _errorColor = Color.red;
    [SerializeField] private float _flashDuration = 0.15f;
    [SerializeField] private int _flashCount = 2;

    [Header("Movement Settings")]
    [SerializeField] private float liftHeight = 0.25f;

    private List<Color> _originalColors = new List<Color>();
    private bool _isSelected;
    private Coroutine _flashCoroutine;

    private void Awake()
    {
        if (_renderers == null || _renderers.Length == 0)
        {
            _renderers = GetComponentsInChildren<MeshRenderer>();
        }

        foreach (var rend in _renderers)
        {
            if (rend != null && rend.material != null)
            {
                _originalColors.Add(rend.material.color);
            }
        }
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;

        if (_flashCoroutine != null) return;

        ApplyCurrentColor();
    }

    public void FlashError()
    {
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < _flashCount; i++)
        {
            SetColorToAll(_errorColor);
            yield return new WaitForSeconds(_flashDuration);

            ApplyCurrentColor();
            yield return new WaitForSeconds(_flashDuration);
        }

        _flashCoroutine = null;
    }

    private void ApplyCurrentColor()
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] == null) continue;
            _renderers[i].material.color = _isSelected ? _highlightColor : _originalColors[i];
        }
    }

    private void SetColorToAll(Color color)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null)
            {
                _renderers[i].material.color = color;
            }
        }
    }

    public void MoveTo(Vector3 targetPos)
    {
        StartCoroutine(MoveRoutine(targetPos));
    }

    private IEnumerator MoveRoutine(Vector3 target)
    {
        Vector3 start = transform.localPosition;

        Vector3 upStart = start + Vector3.up * liftHeight;
        Vector3 upTarget = target + Vector3.up * liftHeight;

        yield return MoveLerp(start, upStart, 0.15f);
        yield return MoveLerp(upStart, upTarget, 0.3f);
        yield return MoveLerp(upTarget, target, 0.15f);
    }

    private IEnumerator MoveLerp(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            transform.localPosition = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = to;
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