using UnityEngine;

public class LeftPanelDynamicSize : MonoBehaviour
{
    public RectTransform downArrow; 
    public RectTransform buttonGroup;

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

        // ***** NEW: Adjust topMargin for notches/rounded corners *****
        Rect safeArea = Screen.safeArea;
        // Convert safe area top offset to parent space
        float safeAreaTopInParent = (safeArea.y / Screen.height) * parentHeight;
        // Add this offset to the topMargin to push the panel down further
        topMargin += safeAreaTopInParent;
        
        panelRect.anchoredPosition = new Vector2(leftMargin, -topMargin);

        // Adjust Button Group size relative to the panel
        float buttonGroupHeight = panelHeight * (8f / 9f);
        buttonGroup.sizeDelta = new Vector2(panelWidth, buttonGroupHeight);

        // Positioning the Button Group using anchors and pivot as before
        buttonGroup.anchorMin = new Vector2(0.5f, 1f);
        buttonGroup.anchorMax = new Vector2(0.5f, 1f);
        buttonGroup.pivot = new Vector2(0.5f, 1f);

        float downArrowHeight = downArrow.rect.height;
        float buttonGroupPosY = -downArrowHeight * 1.2f;
        buttonGroup.anchoredPosition = new Vector2(0, buttonGroupPosY);
    }
}