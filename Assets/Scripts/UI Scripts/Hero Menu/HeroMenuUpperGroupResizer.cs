using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HorizontalLayoutGroup))]
public class HeroMenuUpperGroupResizer : MonoBehaviour
{
    public RectTransform upperGroup; // Reference to Upper Group
    public RectTransform leftPanel; // Reference to Left Panel
    public RectTransform rightPanel; // Reference to Right Panel
    public RectTransform buttonInRightPanel; // Reference to the Button in Right Panel
    public GridLayoutGroup gridInRightPanel; // Reference to the Grid in Right Panel
    public HorizontalLayoutGroup layoutGroup;

    public float initialLeftPadding = 0f; // Initial left padding set in Inspector
    public float initialRightPadding = 50f; // Initial right padding set in Inspector
    // private float initialWidth = 940f; // Initial width: 30+30+580+300
    private float initialWidth; // Initial width: dynamically calculated
    private float leftPanelInitialWidth = 580f;
    private float leftPanelInitialHeight = 580f;
    private float rightPanelInitialWidth = 300f;
    private float rightPanelInitialHeight = 580f;
    private float buttonInitialHeight = 100f; // Initial height of the button

    private void Start()
    {
        // Calculate the initial width in Start
        initialWidth = 580f + 300f + initialLeftPadding + initialRightPadding;

        if (!upperGroup || !leftPanel || !rightPanel || !layoutGroup || !buttonInRightPanel || !gridInRightPanel)
        {
            Debug.LogError("Please assign all required references in the inspector!");
            return;
        }

        AdjustLayout();
    }

    private void Update()
    {
        AdjustLayout();
    }

    private void AdjustLayout()
    {
        float upperGroupWidth = upperGroup.rect.width;

        if (upperGroupWidth >= initialWidth)
        {
            // Reset to initial values
            layoutGroup.padding.left = (int)initialLeftPadding;
            layoutGroup.padding.right = (int)initialRightPadding;

            leftPanel.sizeDelta = new Vector2(leftPanelInitialWidth, leftPanelInitialHeight);
            rightPanel.sizeDelta = new Vector2(rightPanelInitialWidth, rightPanelInitialHeight);
            buttonInRightPanel.sizeDelta = new Vector2(buttonInRightPanel.sizeDelta.x, buttonInitialHeight);

            // Reset Grid Cell Size
            float gridWidth = rightPanelInitialWidth;
            float spacing = gridInRightPanel.spacing.x;
            float blockSize = (gridWidth - spacing) / 2;
            gridInRightPanel.cellSize = new Vector2(blockSize, blockSize);
        }
        else
        {
            // Calculate the shrink factor
            float shrinkFactor = upperGroupWidth / initialWidth;

            // Adjust left and right paddings proportionally
            layoutGroup.padding.left = Mathf.RoundToInt(initialLeftPadding * shrinkFactor);
            layoutGroup.padding.right = Mathf.RoundToInt(initialRightPadding * shrinkFactor);

            // Adjust Left Panel size
            float newLeftWidth = leftPanelInitialWidth * shrinkFactor;
            float newLeftHeight = leftPanelInitialHeight * shrinkFactor;
            leftPanel.sizeDelta = new Vector2(newLeftWidth, newLeftHeight);

            // Adjust Right Panel size
            float newRightWidth = rightPanelInitialWidth * shrinkFactor;
            float newRightHeight = rightPanelInitialHeight * shrinkFactor;
            rightPanel.sizeDelta = new Vector2(newRightWidth, newRightHeight);

            // Adjust Button height in Right Panel
            float newButtonHeight = buttonInitialHeight * shrinkFactor;
            buttonInRightPanel.sizeDelta = new Vector2(buttonInRightPanel.sizeDelta.x, newButtonHeight);

            // Adjust Grid Cell Size
            float gridWidth = newRightWidth;
            float spacing = gridInRightPanel.spacing.x;
            float blockSize = (gridWidth - spacing) / 2;
            gridInRightPanel.cellSize = new Vector2(blockSize, blockSize);
        }

        // Force the layout group to refresh
        LayoutRebuilder.ForceRebuildLayoutImmediate(upperGroup);
    }
}