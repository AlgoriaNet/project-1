using UnityEngine;
using UnityEngine.UI;

public class AlliesGridSetup : MonoBehaviour
{
    public GridLayoutGroup grid; // Assign the GridLayoutGroup
    public RectTransform contentPanel; // Assign the ContentPanel RectTransform
    public int totalImages = 20;

    void Start()
    {
        // Get the content panel width
        float panelWidth = contentPanel.rect.width;

        // Calculate block width
        int constraintCount = grid.constraintCount;
        float blockWidth = panelWidth / (constraintCount * 13f / 12f + 0.25f);

        // Calculate padding and spacing
        float leftPadding = blockWidth / 6;
        float rightPadding = leftPadding;
        float spacingX = leftPadding / 2;
        float spacingY = spacingX;
        float topPadding = leftPadding * 1.5f;
        float bottomPadding = topPadding;

        // Apply calculated padding and spacing
        grid.padding.left = Mathf.RoundToInt(leftPadding);
        grid.padding.right = Mathf.RoundToInt(rightPadding);
        grid.padding.top = Mathf.RoundToInt(topPadding);
        grid.padding.bottom = Mathf.RoundToInt(bottomPadding);
        grid.spacing = new Vector2(spacingX, spacingY);

        // Apply calculated block size
        grid.cellSize = new Vector2(blockWidth, blockWidth);

        // Dynamically create images
        for (int i = 0; i < totalImages; i++)
        {
            GameObject newImage = new GameObject($"Image_{i + 1}", typeof(RectTransform), typeof(Image));
            newImage.transform.SetParent(grid.transform, false);

            // Set the image's default properties
            Image imgComponent = newImage.GetComponent<Image>();
            imgComponent.color = Color.gray; // Set a default color (for visibility)
            imgComponent.preserveAspect = true;
        }
    }
}