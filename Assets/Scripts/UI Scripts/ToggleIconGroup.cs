using System;
using UnityEngine;
using DG.Tweening;

public class ToggleIconGroup : MonoBehaviour
{
    public GameObject buttonGroup; // Assign Icon Group in the Inspector
    public float duration = 0.5f;
    
    private RectTransform _rectTransform;
    private Vector2 _originSizeDelta;
    private bool isVisible = true;

    private void Start()
    {
        _rectTransform = buttonGroup.GetComponent<RectTransform>();
    }

    public void ToggleVisibilityToBottom()
    {
        // Show or hide the Icon Group
        if (isVisible)
        {
            _originSizeDelta = _rectTransform.sizeDelta;
            FadeFromTopToBottom();
        }
        else
        {
            AppearFromBottomToTop();
        }
        isVisible = !isVisible;
    }

    public void ToggleVisibilityToTop()
    {
        // Show or hide the Icon Group
        if (isVisible)
        {
            _originSizeDelta = _rectTransform.sizeDelta;
            FadeFromBottomToTop();
        }
        else
        {
            AppearFromTopToBottom();
        }
        
        isVisible = !isVisible;
    }

    private void FadeFromTopToBottom()
    {
        _rectTransform.DOSizeDelta(new Vector2(_originSizeDelta.x, 0), duration);
        _rectTransform.DOAnchorPosY(- _originSizeDelta.y, duration);
    }

    private void AppearFromBottomToTop()
    {
        _rectTransform.DOSizeDelta(_originSizeDelta, duration);
        _rectTransform.DOAnchorPosY(0, duration);
    }

    private void FadeFromBottomToTop()
    {
        _rectTransform.DOSizeDelta(new Vector2(_originSizeDelta.x, 0), duration);
    }

    private void AppearFromTopToBottom()
    {
        _rectTransform.DOSizeDelta(_originSizeDelta, duration);
    }
}