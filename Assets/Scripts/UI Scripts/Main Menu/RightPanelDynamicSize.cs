using UnityEngine;

public class RightPanelDynamicSize : MonoBehaviour
{
    public RectTransform upArrow;     // Assign the Up Arrow RectTransform in the Inspector
    public RectTransform buttonGroup; // Assign the Button Group RectTransform in the Inspector
    public RectTransform referenceButton; // Assign a reference button RectTransform in the Inspector

    void Start()
    {
        RectTransform panelRect = GetComponent<RectTransform>();

        // ***** NEW: Get parent rect dimensions instead of using Screen dimensions *****
        RectTransform parentRect = panelRect.parent as RectTransform;
        if (parentRect == null)
        {
            Debug.LogWarning("No parent RectTransform found. Cannot adjust panel size dynamically.");
            return;
        }

        float parentWidth = parentRect.rect.width;
        float parentHeight = parentRect.rect.height;

        // Set width (8% of parent width) and height (30% * 8/9 of parent height)
        float panelWidth = parentWidth * 0.08f;
        float panelHeight = parentHeight * 0.3f * (7f / 9f);
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);

        // Position the panel relative to its parent (instead of the raw screen):
        float rightMargin = panelWidth * 0.25f;
        float topMargin = parentHeight * 0.15f;

        // Adjust topMargin for safe area
        Rect safeArea = Screen.safeArea;
        float safeAreaTopInParent = (safeArea.y / Screen.height) * parentHeight;
        topMargin += safeAreaTopInParent;

        panelRect.anchoredPosition = new Vector2(-(rightMargin + panelWidth), -topMargin);

        // Set Button Group size (5/6 of panel height)
        float buttonGroupHeight = panelHeight * (6f / 7f);
        buttonGroup.sizeDelta = new Vector2(panelWidth, buttonGroupHeight);

        // Align Button Group to top-center of the Right Panel
        buttonGroup.anchorMin = new Vector2(0.5f, 1f);
        buttonGroup.anchorMax = new Vector2(0.5f, 1f);
        buttonGroup.pivot = new Vector2(0.5f, 1f);
        buttonGroup.anchoredPosition = Vector2.zero;

        // Force layout update
        Canvas.ForceUpdateCanvases();

        // Align Up Arrow width to the reference button width
        float upArrowWidth = referenceButton.rect.width;
        upArrow.sizeDelta = new Vector2(upArrowWidth, upArrow.sizeDelta.y);

        // Debug.Log($"Reference Button Width: {upArrowWidth}");

        float upArrowHeight = upArrow.rect.height;
        float upArrowPosY = upArrowHeight * 0.2f;
        upArrow.anchoredPosition = new Vector2(0, upArrowPosY);

        // Debug.Log($"Final Up Arrow SizeDelta: {upArrow.sizeDelta}");
    }
}