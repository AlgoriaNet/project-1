using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class AlliesGridSetup : MonoBehaviour
{
    public GridLayoutGroup grid; // Assign the GridLayoutGroup
    public RectTransform contentPanel; // Assign the ContentPanel RectTransform
    public GameObject allyItemPrefab; // Assign the AllyItem prefab
    public int totalAllies = 20;
    private List<string> unlockedIcons;
    public GameObject step1Panel;
    private float blockWidth;
    private int currentAllyIndex;
    private List<(string index, string name)> unlockedAllies; // Store unlocked allies
    private Dictionary<string, int> starLevels;
    private Dictionary<string, GameObject> allyItemsDict = new Dictionary<string, GameObject>();
    private float nameTextWidth;
    private float nameTextHeight;
    private float starGroupWidth;
    private float starGroupHeight;
    private float skillIconWidth;
    public GameObject step2Panel;
    public GameObject lowerGroup;
    public GameObject Board;
    public GameObject Pack;
    public Image step2AllyImage;
    public Transform step2StarGroup; // Assign in the Inspector
    public Button leftArrowButton;
    public Button rightArrowButton;

    public Image shardImage; // Assign the Shard Image object in Inspector
    public Image skillBookImage; // Assign the SkillBook Image object in Inspector

    void Start()
    {
        // Get the content panel width
        float panelWidth = contentPanel.rect.width;

        // Calculate block width
        int constraintCount = grid.constraintCount;

        blockWidth = panelWidth / (constraintCount * 13f / 12f + 0.25f); // 🔹 Store blockWidth
        nameTextWidth = blockWidth * 0.6f;
        nameTextHeight = blockWidth * 0.12f;
        starGroupWidth = blockWidth * 0.5f;
        starGroupHeight = blockWidth * 0.1f;
        skillIconWidth = blockWidth * 0.2f;

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
        grid.cellSize = new Vector2(blockWidth, blockWidth * 1.342f); // Adjust height proportionally

        LoadAllyItems();
    }

    private void LoadAllyItems()
    {
        // Character names matching the file names
        string[] characterNames = {
            "Zorath", "Gideon", "Sylas", "Aurelia", "Lyanna", "Zhara", "Elenya", "Rowan",
            "Liraen", "Cedric", "Selena", "Morgath", "Zyphira", "Kaelith", "Velan", "Ragnar",
            "Lucien", "Ugra", "Eleanor", "Nyx"
        };

        // Simulated unlocked icons for testing
        unlockedIcons = new List<string> { "00", "03", "06", "08", "11", "16", "19" }; // Example unlocked items

        // Simulated star levels for unlocked allies
        starLevels = new Dictionary<string, int>
        {
            { "03", 2 }, // 2-star level
            { "06", 1 }, // 1-star level
            { "08", 4 }, // 4-star level
            { "11", 3 }, // 3-star level
            { "16", 5 }, // 5-star level
            { "19", 3 }  // 3-star level
        };

        // Prepare lists for sorting
        List<(string index, string name, bool isUnlocked)> sortedAllies = new List<(string, string, bool)>();

        for (int i = 0; i < totalAllies; i++)
        {
            string index = (i + 1).ToString("D2");

            // Skip "00" as it doesn't correspond to any AllyItem
            if (index == "00") continue;

            bool isUnlocked = unlockedIcons.Contains(index);
            sortedAllies.Add((index, characterNames[i], isUnlocked));
        }

        // Sort by unlock status first, then by index in ascending order
        sortedAllies.Sort((a, b) =>
        {
            int unlockComparison = b.isUnlocked.CompareTo(a.isUnlocked); // Unlocked first
            return unlockComparison == 0 ? a.index.CompareTo(b.index) : unlockComparison;
        });

        // ***** NEW: Populate unlockedAllies with only unlocked entries *****
        unlockedAllies = new List<(string index, string name)>();
        foreach (var ally in sortedAllies)
        {
            if (ally.isUnlocked)
            {
                unlockedAllies.Add((ally.index, ally.name));
            }
        }

        // Instantiate ally items
        foreach (var ally in sortedAllies)
        {
            GameObject newAlly = Instantiate(allyItemPrefab, grid.transform, false);

            // ***** RENAME THE GAMEOBJECT FOR substring() TO WORK *****
            newAlly.name = $"AllyItem_{ally.index}";

            // Store reference
            allyItemsDict[ally.index] = newAlly;

            string imagePath = $"UILoading/CharacterImages/CardDisplay/C_{ally.index}_{ally.name}";

            // Assign Ally Image
            Image allyImage = newAlly.transform.Find("AllyImage").GetComponent<Image>();
            Image skillIcon = newAlly.transform.Find("SkillIcon").GetComponent<Image>();
            Sprite allySprite = Resources.Load<Sprite>(imagePath);

            if (allySprite != null)
            {
                allyImage.sprite = allySprite;
            }
            else
            {
                Debug.LogWarning($"❌ Image not found at path: {imagePath}");
            }

            // Assign Ally Name
            TextMeshProUGUI nameText = newAlly.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            nameText.text = ally.name;

            // Assign Gray Cover for Locked/Unlocked Logic
            allyImage.color = ally.isUnlocked ? Color.white : Color.gray;
            skillIcon.color = ally.isUnlocked ? Color.white : Color.gray;
            nameText.color = ally.isUnlocked ? Color.white : Color.gray;

            // Find StarGroup inside newAlly
            Transform starGroup = newAlly.transform.Find("StarGroup");

            // Check if this ally has a predefined star level
            int starLevel = starLevels.ContainsKey(ally.index) ? starLevels[ally.index] : 0;

            for (int i = 1; i <= 5; i++)
            {
                Transform starYellow = starGroup.Find($"Star{i}/yellow"); // Direct lookup

                if (starYellow != null)
                {
                    starYellow.gameObject.SetActive(i <= starLevel); // Activate based on star level
                }
                else
                {
                    Debug.LogWarning($"❌ 'yellow' NOT FOUND inside Star{i}!");
                }
            }

            // Retain Gray Tint for Locked Allies
            foreach (Image star in starGroup.GetComponentsInChildren<Image>())
            {
                star.color = ally.isUnlocked ? Color.white : Color.gray;
            }

            // 🔹 NEW: Adjust UI sizes dynamically
            AdjustAllyItemSize(newAlly); // 🔹 Now using stored blockWidth

            // Keep existing setup function call
            SetupAllyButton(newAlly, ally.index, ally.name, ally.isUnlocked);
        }
    }

    private void AdjustAllyItemSize(GameObject allyItem)
    {
        // Adjust NameText width (60% of block width)
        TextMeshProUGUI nameText = allyItem.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        RectTransform nameTextRect = nameText.GetComponent<RectTransform>();
        nameTextRect.sizeDelta = new Vector2(nameTextWidth, nameTextHeight);

        // Set NameText position (Y = starGroupHeight + 5)
        nameTextRect.anchoredPosition = new Vector2(nameTextRect.anchoredPosition.x, starGroupHeight + 13);

        // Adjust StarGroup width and height (50% width, 10% height)
        Transform starGroup = allyItem.transform.Find("StarGroup");
        RectTransform starGroupRect = starGroup.GetComponent<RectTransform>();
        starGroupRect.sizeDelta = new Vector2(starGroupWidth, starGroupHeight);

        // Adjust GridLayoutGroup inside StarGroup
        GridLayoutGroup starGrid = starGroup.GetComponent<GridLayoutGroup>();
        if (starGrid != null)
        {
            starGrid.cellSize = new Vector2(starGroupRect.sizeDelta.y, starGroupRect.sizeDelta.y); // Square cell
        }

        // Adjust SkillIcon width (20% of block width)
        Image skillIcon = allyItem.transform.Find("SkillIcon").GetComponent<Image>();
        RectTransform skillIconRect = skillIcon.GetComponent<RectTransform>();
        skillIconRect.sizeDelta = new Vector2(skillIconWidth, skillIconRect.sizeDelta.y);
    }

    private void SetupAllyButton(GameObject allyItem, string index, string name, bool isUnlocked)
    {
        Button allyButton = allyItem.GetComponent<Button>();

        if (allyButton == null)
        {
            allyButton = allyItem.AddComponent<Button>();
        }

        allyButton.interactable = isUnlocked;

        if (isUnlocked)
        {
            allyButton.onClick.AddListener(() => OpenStep2(allyItem, index, name));
        }
    }

    private void OpenStep2(GameObject allyItem, string index, string name)
    {
        if (step2AllyImage == null)
        {
            Debug.LogError("❌ step2AllyImage is NOT assigned in Inspector!");
            return;
        }

        // Switch panels
        step1Panel.SetActive(false);
        step2Panel.SetActive(true);
        lowerGroup.SetActive(true);
        Board.SetActive(true);
        Pack.SetActive(false);

        // Construct and load illustration
        string illustrationPath = $"UILoading/CharacterImages/Stand_Illustration/P_{index}_{name}";
        Sprite illustrationSprite = Resources.Load<Sprite>(illustrationPath);

        if (illustrationSprite != null)
        {
            step2AllyImage.sprite = illustrationSprite;
        }
        else
        {
            Debug.LogWarning($"❌ Illustration not found at path: {illustrationPath}");
        }


        // Assign Shard Image
        string shardPath = $"UILoading/CharacterImages/Shard/{index}_{name}";
        Sprite shardSprite = Resources.Load<Sprite>(shardPath);

        if (shardSprite != null && shardImage != null)
        {
            shardImage.sprite = shardSprite;
            shardImage.color = Color.white; // Ensure it's visible
        }
        else
        {
            Debug.LogWarning($"❌ Shard image not found at path: {shardPath}");
            if (shardImage != null) shardImage.color = new Color(0, 0, 0, 0); // Make it invisible if not found
        }

        // Assign SkillBook Image
        string skillBookPath = $"UILoading/CharacterImages/Skillbook/Skb_{index}_{name}";
        Sprite skillBookSprite = Resources.Load<Sprite>(skillBookPath);

        if (skillBookSprite != null && skillBookImage != null)
        {
            skillBookImage.sprite = skillBookSprite;
            skillBookImage.color = Color.white; // Ensure it's visible
        }
        else
        {
            Debug.LogWarning($"❌ SkillBook image not found at path: {skillBookPath}");
            if (skillBookImage != null) skillBookImage.color = new Color(0, 0, 0, 0); // Make it invisible if not found
        }

        // Track the current ally index for navigation
        currentAllyIndex = unlockedAllies.FindIndex(a => a.index == index);

        // Assign button events
        leftArrowButton.onClick.RemoveAllListeners();
        rightArrowButton.onClick.RemoveAllListeners();
        leftArrowButton.onClick.AddListener(() => NavigateStep2Ally(-1));
        rightArrowButton.onClick.AddListener(() => NavigateStep2Ally(1));
    
        // ***** NEW: Enable/disable arrows based on position *****
        leftArrowButton.interactable = (currentAllyIndex > 0);
        rightArrowButton.interactable = (currentAllyIndex < unlockedAllies.Count - 1);  

        // 🔹 Get StarGroup from the clicked allyItem in Step 1
        Transform step1StarGroup = allyItem.transform.Find("StarGroup");
        if (step1StarGroup == null)
        {
            Debug.LogWarning($"❌ Step 1 StarGroup not found for Ally {index} ({name})!");
            return;
        }

        // 🔹 Mirror StarGroup from Step 1 to Step 2
        for (int i = 1; i <= 5; i++)
        {
            Transform step1Star = step1StarGroup.Find($"Star{i}/yellow");
            Transform step2Star = step2StarGroup.Find($"Star{i}/yellow");

            if (step1Star != null && step2Star != null)
            {
                step2Star.gameObject.SetActive(step1Star.gameObject.activeSelf);
            }
            else
            {
                Debug.LogWarning($"❌ Star{i}/yellow not found in Step 1 or Step 2!");
            }
        }

        // Delay the size adjustment to allow layout updates
        Invoke(nameof(AdjustStep2StarGroupSize), 0.1f);
    }

    private void AdjustStep2StarGroupSize()
    {
        GridLayoutGroup step2Grid = step2StarGroup.GetComponent<GridLayoutGroup>();
        RectTransform step2StarGroupRect = step2StarGroup.GetComponent<RectTransform>();

        if (step2Grid != null && step2StarGroupRect != null)
        {
            float updatedWidth = step2StarGroupRect.rect.width;
            Debug.Log($"🔄 DELAYED UPDATE: Step2StarGroup Width = {updatedWidth}, Old CellSize = {step2Grid.cellSize}");

            float starCellSize = updatedWidth * 0.2f;
            step2Grid.cellSize = new Vector2(starCellSize, starCellSize);

            Debug.Log($"✅ FINAL Update: Step2StarGroup Width = {updatedWidth}, New CellSize = {step2Grid.cellSize}");
        }
    }

    private void NavigateStep2Ally(int direction)
    {
        int newIndex = currentAllyIndex + direction;
        if (newIndex >= 0 && newIndex < unlockedAllies.Count)
        {
            currentAllyIndex = newIndex;
            string newIndexStr = unlockedAllies[newIndex].index;
            string newName = unlockedAllies[newIndex].name;

            // 🔹 Use dictionary instead of GameObject.Find()
            if (allyItemsDict.TryGetValue(newIndexStr, out GameObject newAllyItem))
            {
                OpenStep2(newAllyItem, newIndexStr, newName);
            }
            else
            {
                Debug.LogWarning($"❌ AllyItem_{newIndexStr} not found in dictionary!");
            }
        }
    }
}