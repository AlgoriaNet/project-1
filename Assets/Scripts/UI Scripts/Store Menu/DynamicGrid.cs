using UnityEngine;
using UnityEngine.UI;

public class DynamicGrid : MonoBehaviour
{
    public GridLayoutGroup gridLayout; // Assign GridLayoutGroup in Inspector
    private RectTransform gridRect; // Reference to the Grid's RectTransform

    private float aspectRatio = 1.25f; // Slot aspect ratio (220:275)

    void Start()
    {
        gridRect = gridLayout.GetComponent<RectTransform>(); // Get the RectTransform
        AdjustGridLayout();
    }

    private void AdjustGridLayout()
    {
        Canvas.ForceUpdateCanvases(); 

        float gridHeight = gridRect.rect.height;
        float gridWidth = gridRect.rect.width;

        float slotHeight = gridHeight / 2.4f; 
        float paddingAndSpacingY = (gridHeight - (slotHeight * 2)) / 3; // Correct Y padding

        float slotWidth = slotHeight / aspectRatio; 
        float paddingAndSpacingX = (gridWidth - (slotWidth * 3)) / 4; // Correct X padding

        // Apply calculated values
        gridLayout.cellSize = new Vector2(slotWidth, slotHeight);
        gridLayout.spacing = new Vector2(paddingAndSpacingX, paddingAndSpacingY);
        gridLayout.padding.left = Mathf.RoundToInt(paddingAndSpacingX);
        gridLayout.padding.right = Mathf.RoundToInt(paddingAndSpacingX);
        gridLayout.padding.top = Mathf.RoundToInt(paddingAndSpacingY);
        gridLayout.padding.bottom = Mathf.RoundToInt(paddingAndSpacingY);

        // Debug Logs
        Debug.Log($"✅ Grid Layout Updated");
        Debug.Log($"Grid Width: {gridWidth}, Grid Height: {gridHeight}");
        Debug.Log($"Slot Width: {slotWidth}, Slot Height: {slotHeight}");
        Debug.Log($"Padding X: {paddingAndSpacingX}, Padding Y: {paddingAndSpacingY}");
        Debug.Log($"Spacing X: {gridLayout.spacing.x}, Spacing Y: {gridLayout.spacing.y}");
    }
}