using UnityEngine;

public class LeftPanelDynamicSize : MonoBehaviour
{
    public RectTransform downArrow; 
    public RectTransform buttonGroup;
    public RectTransform referenceButton; // Assign the specific button in the Inspector

    void Start()
    {
        RectTransform panelRect = GetComponent<RectTransform>();
        
        // Get parent container size (assuming the parent is a Canvas or another RectTransform)
        RectTransform parentRect = panelRect.parent as RectTransform;
        if (parentRect == null)
        {
            Debug.LogWarning("No parent RectTransform found. Using Screen dimensions as fallback.");
            return;
        }

        float parentWidth = parentRect.rect.width;
        float parentHeight = parentRect.rect.height;

        // Calculate panel size based on parent’s dimensions rather than Screen
        float panelWidth = parentWidth * 0.08f;
        float panelHeight = parentHeight * 0.3f;
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);

        // Use a margin that scales with panel width/height rather than screen sizes
        float leftMargin = panelWidth * 0.25f; 
        float topMargin = parentHeight * 0.12f; 

        // Adjust for safe area
        Rect safeArea = Screen.safeArea;
        float safeAreaTopInParent = (safeArea.y / Screen.height) * parentHeight;
        topMargin += safeAreaTopInParent;
        
        panelRect.anchoredPosition = new Vector2(leftMargin, -topMargin);

        // Adjust Button Group size relative to the panel
        float buttonGroupHeight = panelHeight * (8f / 9f);
        buttonGroup.sizeDelta = new Vector2(panelWidth, buttonGroupHeight);

        buttonGroup.anchorMin = new Vector2(0.5f, 1f);
        buttonGroup.anchorMax = new Vector2(0.5f, 1f);
        buttonGroup.pivot = new Vector2(0.5f, 1f);

        // Force layout updates
        Canvas.ForceUpdateCanvases();

        // Adjust the down arrow's width based on the reference button
        float downArrowWidth = referenceButton.rect.width;
        downArrow.sizeDelta = new Vector2(downArrowWidth, downArrow.sizeDelta.y);

        // Adjust the button group's position based on the new down arrow width
        float buttonGroupPosY = -downArrowWidth * 1.2f;
        buttonGroup.anchoredPosition = new Vector2(0, buttonGroupPosY);
    }
}