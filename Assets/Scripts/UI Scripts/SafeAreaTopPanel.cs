using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class AdjustTopPanelForSafeArea : MonoBehaviour
{
    void Awake()
    {
        ApplySafeArea();
    }

    private void ApplySafeArea()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Rect safeArea = Screen.safeArea;

        Vector2 anchorMin = rectTransform.anchorMin;
        Vector2 anchorMax = rectTransform.anchorMax;

        float originalHeightRatio = anchorMax.y - anchorMin.y;
        float yOffset = safeArea.y / Screen.height;

        // ***** CHANGED: Use subtraction instead of addition *****
        // This will move the panel downward, not upward.
        anchorMax.y -= yOffset;
        anchorMin.y = anchorMax.y - originalHeightRatio;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;

        // Debug.Log($"Safe Area: {safeArea}");
        // Debug.Log($"Anchor Min: {anchorMin}, Anchor Max: {anchorMax}");
    }
}