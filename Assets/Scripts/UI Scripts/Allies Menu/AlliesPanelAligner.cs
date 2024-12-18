using UnityEngine;

public class AlliesPanelAligner: MonoBehaviour
{
    public RectTransform topPanel; // Reference to the Top Panel in Common Page
    public RectTransform bottomPanel; // Reference to the Bottom Panel in Common Page
    public RectTransform panel; // Reference to this Panel in Allies Menu

    private void Update()
    {
        if (topPanel && bottomPanel && panel)
        {
            // Define the padding
            float padding = 20f;

            // Get the heights of the Top Panel and Bottom Panel
            float topHeight = topPanel.rect.height + padding;
            float bottomHeight = bottomPanel.rect.height + padding;

            // Calculate safe area adjustment using the top safe area offset
            Rect safeArea = Screen.safeArea;
            float safeAreaAdjustment = safeArea.y / Screen.height * Screen.height;

            // Include the safe area adjustment in the top height
            topHeight += safeAreaAdjustment;

            // Apply these heights with padding and safe area adjustment to the Center Panel's RectTransform
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.offsetMin = new Vector2(panelRect.offsetMin.x, bottomHeight); // Bottom
            panelRect.offsetMax = new Vector2(panelRect.offsetMax.x, -topHeight);  // Top
        }
    }
}