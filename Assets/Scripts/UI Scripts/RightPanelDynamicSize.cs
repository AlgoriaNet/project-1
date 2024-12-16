using UnityEngine;

public class RightPanelDynamicSize : MonoBehaviour
{
    public RectTransform upArrow;     // Assign the Up Arrow RectTransform in the Inspector
    public RectTransform buttonGroup; // Assign the Button Group RectTransform in the Inspector

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
        float panelHeight = parentHeight * 0.3f * (8f / 9f);
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);

        // Position the panel relative to its parent (instead of the raw screen):
        float rightMargin = panelWidth * 0.25f;
        float topMargin = parentHeight * 0.15f;

        // ***** NEW: Adjust topMargin for notches/rounded corners *****
        Rect safeArea = Screen.safeArea;
        // Convert safe area top offset to parent space
        float safeAreaTopInParent = (safeArea.y / Screen.height) * parentHeight;
        // Add this offset to the topMargin to push the panel down further
        topMargin += safeAreaTopInParent;

        panelRect.anchoredPosition = new Vector2(-(rightMargin + panelWidth), -topMargin);

        // Set Button Group size (5/6 of panel height)
        float buttonGroupHeight = panelHeight * (5f / 6f);
        buttonGroup.sizeDelta = new Vector2(panelWidth, buttonGroupHeight);

        // Align Button Group to top-center of the Right Panel
        buttonGroup.anchorMin = new Vector2(0.5f, 1f);
        buttonGroup.anchorMax = new Vector2(0.5f, 1f);
        buttonGroup.pivot = new Vector2(0.5f, 1f);
        buttonGroup.anchoredPosition = Vector2.zero;

        // Align Up Arrow to bottom-center of the Button Group
        upArrow.anchorMin = new Vector2(0.5f, 0f);
        upArrow.anchorMax = new Vector2(0.5f, 0f);
        upArrow.pivot = new Vector2(0.5f, 0f);

        float upArrowHeight = upArrow.rect.height;
        float upArrowPosY = upArrowHeight * 0.2f;
        upArrow.anchoredPosition = new Vector2(0, upArrowPosY);
    }
}