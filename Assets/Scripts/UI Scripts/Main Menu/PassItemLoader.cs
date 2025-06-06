using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PassItemLoader : MonoBehaviour
{
    public GameObject passBlockPrefab;  // Prefab for blocks
    public Transform contentPanel;      // Parent that holds the grid of blocks
    public Color greyishColor = Color.gray;  // Define the greyish color for locked items
    public int levelMultiplier = 1;     // Multiplier for LevelText (default is 1 for BattlePassPage)

    void Start()
    {
        int rows = 10;
        int columns = 3;

        RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
        float contentWidth = contentRect.rect.width;

        float blockWidth = contentWidth / 6f;  // Block width is 1/6 of Content width
        float blockHeight = blockWidth;        // Block height equals block width
        float spacingX = contentWidth / 6f;    // Spacing between columns
        float spacingY = blockHeight / 3f;     // Spacing between rows
        float paddingLR = contentWidth / 12f;  // Left and right padding

        GridLayoutGroup gridLayout = contentPanel.GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(blockWidth, blockHeight); // Set dynamic cell size
        gridLayout.spacing = new Vector2(spacingX, spacingY);       // Set spacing for both X and Y
        gridLayout.padding = new RectOffset(
            Mathf.RoundToInt(paddingLR),  // Left padding
            Mathf.RoundToInt(paddingLR),  // Right padding
            Mathf.RoundToInt(spacingY),   // Top padding set to spacing Y
            Mathf.RoundToInt(spacingY)    // Bottom padding set to spacing Y
        );

        for (int i = 0; i < rows; i++) // Iterate through each row
        {
            for (int j = 0; j < columns; j++) // Iterate through each column
            {
                GameObject block = Instantiate(passBlockPrefab, contentPanel);
                Transform lockTransform = block.transform.Find("Lock");
                Image blockImage = block.transform.Find("Image")?.GetComponent<Image>();
                Transform labelTransform = block.transform.Find("Label");
                TMP_Text levelText = labelTransform?.Find("LevelText")?.GetComponent<TMP_Text>();

                // Unlock logic: First two rows are unlocked; others are locked
                bool isLocked = (i >= 2);
                if (lockTransform != null)
                {
                    lockTransform.gameObject.SetActive(isLocked);
                }

                // Update block image color based on lock status
                if (blockImage != null)
                {
                    blockImage.color = isLocked ? greyishColor : Color.white;
                }

                // Label logic: Only active for the first block in each row
                if (j == 0 && labelTransform != null) // Only for the first block of the row
                {
                    labelTransform.gameObject.SetActive(true); // Activate the label for the first block

                    if (levelText != null)
                    {
                        // Use the levelMultiplier to calculate LevelText
                        levelText.text = (levelMultiplier * (i + 1)).ToString();

                        // Update Label and LevelText appearance based on isLocked
                        if (isLocked)
                        {
                            labelTransform.GetComponent<Image>().color = greyishColor; // Label image turns greyish if locked
                            // levelText.color = Color.grey; // LevelText turns grey if locked
                        }
                        else
                        {
                            labelTransform.GetComponent<Image>().color = Color.white; // Label image turns normal if unlocked
                            // levelText.color = Color.black; // LevelText turns black if unlocked
                        }
                    }
                }
                else if (labelTransform != null) // For non-first blocks in the row
                {
                    labelTransform.gameObject.SetActive(false); // Deactivate the label for other blocks
                }

            }
        }
    }
}

