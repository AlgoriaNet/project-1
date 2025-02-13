using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class lineupPopupController : MonoBehaviour
{
    public GameObject lineupPopup;       // Assign in Inspector
    public GridLayoutGroup grid;         // Assign GridLayoutGroup inside LineupPopup
    public RectTransform contentPanel;   // Assign Content Panel RectTransform (child of lineupPopup)
    public GameObject allyItemPrefab;    // Assign AllyItem prefab
    public int blocksPerRow = 5;         // Number of columns

    // These will be computed based on panel size (following AlliesGridSetup)
    private float blockWidth;
    private float nameTextWidth;
    private float nameTextHeight;
    private float starGroupWidth;
    private float starGroupHeight;
    private float skillIconWidth;

    private List<(string index, string name)> unlockedAllies;  // Store unlocked allies
    private Dictionary<string, int> starLevels;               // Store star levels

    void Start()
    {
        LoadAlliesData();            // Prepare unlockedAllies & starLevels
        AdjustGridForFivePerRow();   // Compute UI sizes and set grid properties
        LoadAllyItems();             // Instantiate and adjust each ally item
    }

    private void LoadAlliesData()
    {
        // Same character list as in AlliesGridSetup
        string[] characterNames = {
            "Zorath", "Gideon", "Sylas", "Aurelia", "Lyanna", "Zhara", "Elenya", "Rowan",
            "Liraen", "Cedric", "Selena", "Morgath", "Zyphira", "Kaelith", "Velan", "Ragnar",
            "Lucien", "Ugra", "Eleanor", "Nyx"
        };

        // Unlocked icons from previous setup
        List<string> unlockedIcons = new List<string> { "00", "03", "06", "08", "11", "16", "19" };

        // Star levels from previous setup
        starLevels = new Dictionary<string, int>
        {
            { "03", 2 }, { "06", 1 }, { "08", 4 },
            { "11", 3 }, { "16", 5 }, { "19", 3 }
        };

        // Filter only unlocked allies (skip those not in unlockedIcons)
        unlockedAllies = new List<(string index, string name)>();
        for (int i = 0; i < characterNames.Length; i++)
        {
            string index = (i + 1).ToString("D2");
            if (unlockedIcons.Contains(index))
            {
                unlockedAllies.Add((index, characterNames[i]));
            }
        }
    }

    private void AdjustGridForFivePerRow()
    {
        // Get the content panel width.
        // If the popup is inactive, the rect width may be zero.
        float panelWidth = contentPanel.rect.width;
        if (panelWidth == 0)
        {
            // Temporarily activate the popup to force layout calculations.
            bool wasActive = lineupPopup.activeSelf;
            if (!wasActive)
            {
                lineupPopup.SetActive(true);
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel);
                panelWidth = contentPanel.rect.width;
                lineupPopup.SetActive(false);
            }
        }

        // Use the same calculation as in AlliesGridSetup:
        int constraintCount = blocksPerRow;  // (i.e. 5)
        blockWidth = panelWidth / (constraintCount * 13f / 12f + 0.25f);
        nameTextWidth   = blockWidth * 0.6f;
        nameTextHeight  = blockWidth * 0.12f;
        starGroupWidth  = blockWidth * 0.5f;
        starGroupHeight = blockWidth * 0.1f;
        skillIconWidth  = blockWidth * 0.2f;

        // Set grid properties (cell size, spacing, and padding)
        grid.cellSize = new Vector2(blockWidth, blockWidth * 1.342f);

        float leftPadding = blockWidth / 6f;
        float spacingX = leftPadding / 2f;
        float spacingY = spacingX;
        grid.padding.left = Mathf.RoundToInt(leftPadding);
        grid.padding.right = Mathf.RoundToInt(leftPadding);
        grid.padding.top = Mathf.RoundToInt(leftPadding * 1.5f);
        grid.padding.bottom = Mathf.RoundToInt(leftPadding * 1.5f);
        grid.spacing = new Vector2(spacingX, spacingY);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = blocksPerRow;
    }

    private void LoadAllyItems()
    {
        foreach (var ally in unlockedAllies)
        {
            GameObject newAlly = Instantiate(allyItemPrefab, grid.transform, false);
            newAlly.name = $"AllyItem_{ally.index}";

            // --- Adjust NameText ---
            TextMeshProUGUI nameText = newAlly.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            nameText.text = ally.name;
            RectTransform nameTextRect = nameText.GetComponent<RectTransform>();
            nameTextRect.sizeDelta = new Vector2(nameTextWidth, nameTextHeight);
            // Set Y position similar to AlliesGridSetup (starGroupHeight + 13)
            nameTextRect.anchoredPosition = new Vector2(nameTextRect.anchoredPosition.x, starGroupHeight + 13);

            // --- Adjust AllyImage ---
            Image allyImage = newAlly.transform.Find("AllyImage").GetComponent<Image>();
            string imagePath = $"UILoading/CharacterImages/CardDisplay/C_{ally.index}_{ally.name}";
            Sprite allySprite = Resources.Load<Sprite>(imagePath);
            if (allySprite != null)
            {
                allyImage.sprite = allySprite;
            }
            else
            {
                Debug.LogWarning($"❌ Image not found at path: {imagePath}");
            }

            // --- Adjust SkillIcon ---
            Image skillIcon = newAlly.transform.Find("SkillIcon").GetComponent<Image>();
            RectTransform skillIconRect = skillIcon.GetComponent<RectTransform>();
            skillIconRect.sizeDelta = new Vector2(skillIconWidth, skillIconRect.sizeDelta.y);

            // --- Adjust StarGroup ---
            Transform starGroup = newAlly.transform.Find("StarGroup");
            RectTransform starGroupRect = starGroup.GetComponent<RectTransform>();
            starGroupRect.sizeDelta = new Vector2(starGroupWidth, starGroupHeight);

            // (Optional) If the StarGroup has a GridLayoutGroup, adjust its cell size as in AlliesGridSetup:
            GridLayoutGroup starGrid = starGroup.GetComponent<GridLayoutGroup>();
            if (starGrid != null)
            {
                // Square cells using the starGroup’s height.
                starGrid.cellSize = new Vector2(starGroupHeight, starGroupHeight);
            }

            // --- Setup Stars ---
            // Use the structure from AlliesGridSetup:
            int starLevel = starLevels.ContainsKey(ally.index) ? starLevels[ally.index] : 0;
            for (int i = 1; i <= 5; i++)
            {
                // Look for the yellow star using the same path as AlliesGridSetup
                Transform starYellow = starGroup.Find($"Star{i}/yellow");
                if (starYellow != null)
                {
                    starYellow.gameObject.SetActive(i <= starLevel);
                }
                else
                {
                    Debug.LogWarning($"❌ 'yellow' star not found in Star{i} for ally {ally.index}");
                }
            }
        }
    }

    public void OpenLineup()
    {
        lineupPopup.SetActive(true);
    }

    public void CloseLineup()
    {
        lineupPopup.SetActive(false);
    }
}