using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class LineupController : MonoBehaviour
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
    
    private GameObject selectedAlly = null; // Track the currently selected AllyItem
    private HashSet<GameObject> officiallySelectedAllies = new HashSet<GameObject>(); // Stores permanently grey Allies
    private Dictionary<Button, GameObject> lineupDictionary = new Dictionary<Button, GameObject>(); // Tracks selected slots

    // private Dictionary<Button, GameObject> slotAssignments = new Dictionary<Button, GameObject>(); // Track slot assignments

    public Button slot1, slot2, slot3, slot4; // Assign in Inspector

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
        // int constraintCount = blocksPerRow;  // (i.e. 5)
        // blockWidth = panelWidth / (constraintCount * 13f / 12f + 0.25f);
        blockWidth = panelWidth / 5.75f; // *** test
        nameTextWidth   = blockWidth * 0.6f;
        nameTextHeight  = blockWidth * 0.12f;
        starGroupWidth  = blockWidth * 0.5f;
        starGroupHeight = blockWidth * 0.1f;
        skillIconWidth  = blockWidth * 0.2f;

        // Set grid properties (cell size, spacing, and padding)
        grid.cellSize = new Vector2(blockWidth, blockWidth * 1.342f);

        // float leftPadding = blockWidth / 6f;
        float leftPadding = blockWidth / 4f; // *** test
        // float spacingX = leftPadding / 2f;
        float spacingX = leftPadding / 4f; // *** test
        float spacingY = spacingX;
        grid.padding.left = Mathf.RoundToInt(leftPadding);
        grid.padding.right = Mathf.RoundToInt(leftPadding);
        grid.padding.top = Mathf.RoundToInt(leftPadding * 1.2f);
        grid.padding.bottom = Mathf.RoundToInt(leftPadding * 1.2f);
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
    
            // --- Add Button Click Event ---
            Button allyButton = newAlly.GetComponent<Button>();
            if (allyButton == null)
            {
                allyButton = newAlly.AddComponent<Button>();
            }
            allyButton.onClick.AddListener(() => SelectAlly(newAlly));
            }
    }

    private void SelectAlly(GameObject allyItem)
    {
        // ✅ Stop pre-selection if 4 slots are already filled
        if (lineupDictionary.Count >= 4)
        {
            Debug.LogWarning("❌ Lineup is full! You cannot select more allies.");
            return;
        }

        if (selectedAlly == allyItem) return; // If already selected, do nothing

        // Reset the previous selection (only if it wasn't officially selected)
        if (selectedAlly != null && !officiallySelectedAllies.Contains(selectedAlly))
        {
            // if (prevImage != null) prevImage.color = Color.white; // Restore original color
            SetAllyColor(selectedAlly, Color.white); // Restore all elements to normal
        }

        // Update the new selection
        selectedAlly = allyItem;

        // ✅ Apply greyish color to all elements (temporary until officially selected)
        SetAllyColor(selectedAlly, new Color(0.4f, 0.4f, 0.4f, 1f)); // Dark grey
    }

    private void SetAllyColor(GameObject allyItem, Color color)
    {
        if (allyItem == null) return;

        // ✅ Ally Image
        Image allyImage = allyItem.transform.Find("AllyImage")?.GetComponent<Image>();
        if (allyImage != null) allyImage.color = color;

        // ✅ Name Text
        TextMeshProUGUI nameText = allyItem.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null) nameText.color = color;

        // ✅ Skill Icon
        Image skillIcon = allyItem.transform.Find("SkillIcon")?.GetComponent<Image>();
        if (skillIcon != null) skillIcon.color = color;

        // ✅ StarGroup (affects both white & yellow stars)
        Transform starGroup = allyItem.transform.Find("StarGroup");
        if (starGroup != null)
        {
            foreach (Transform star in starGroup)
            {
                Image whiteStar = star.Find("white")?.GetComponent<Image>();
                Image yellowStar = star.Find("yellow")?.GetComponent<Image>();

                if (whiteStar != null) whiteStar.color = color;
                if (yellowStar != null) yellowStar.color = color;
            }
        }
    }

    public void EnableBattleIcon(Button slotButton)
    {
        // ✅ If the slot already has an assigned AllyItem, remove it
        if (lineupDictionary.ContainsKey(slotButton))
        {
            GameObject assignedAlly = lineupDictionary[slotButton];

            // ✅ Disable the battleIcon inside this slot
            Transform battleIcon = slotButton.transform.Find("battleIcon");
            if (battleIcon != null) battleIcon.gameObject.SetActive(false);

            // ✅ Restore the AllyItem's color to normal
            SetAllyColor(assignedAlly, Color.white);

            // ✅ Remove the AllyItem from officially selected list
            officiallySelectedAllies.Remove(assignedAlly);

            // ✅ Remove from lineup dictionary
            lineupDictionary.Remove(slotButton);

            Debug.Log($"🔄 Slot {slotButton.name} cleared! Ally is now unselected.");
            return; // Exit function since we removed the assignment
        }

        // ✅ If no AllyItem is selected, do nothing
        if (selectedAlly == null)
        {
            Debug.LogWarning("❌ No ally selected! Select an AllyItem before clicking a slot.");
            return;
        }

        // ✅ Check if this slot is already filled (shouldn't reach here, but for safety)
        if (lineupDictionary.ContainsKey(slotButton))
        {
            Debug.LogWarning("❌ Slot already filled! Choose an empty slot.");
            return;
        }

        // ✅ Get the selected Ally’s index & name
        string allyIndex = selectedAlly.name.Split('_')[1]; // Extract "03" from "AllyItem_03"
        string allyName = selectedAlly.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text;

        // ✅ Load the correct battle icon from Resources
        string battleIconPath = $"UILoading/CharacterImages/UserIcons/BattlYellowIcons/L_{allyIndex}_{allyName}";
        Sprite battleIconSprite = Resources.Load<Sprite>(battleIconPath);
        if (battleIconSprite == null)
        {
            Debug.LogWarning($"❌ Battle icon not found at path: {battleIconPath}");
            return;
        }

        // ✅ Enable and assign the battle icon inside the Slot (Slot1/battleIcon)
        Transform slotBattleIcon = slotButton.transform.Find("battleIcon");
        if (slotBattleIcon != null)
        {
            slotBattleIcon.gameObject.SetActive(true); // Enable battle icon
            Image slotBattleIconImage = slotBattleIcon.GetComponent<Image>();
            if (slotBattleIconImage != null)
            {
                slotBattleIconImage.sprite = battleIconSprite; // Upload the correct image
                slotBattleIconImage.color = Color.white; // Ensure it's fully visible
            }
        }
        else
        {
            Debug.LogWarning($"❌ battleIcon not found inside {slotButton.name}!");
        }

        // ✅ Permanently mark this AllyItem as officially selected
        if (!officiallySelectedAllies.Contains(selectedAlly))
        {
            officiallySelectedAllies.Add(selectedAlly);
        }

        // ✅ Add to lineupDictionary (track which ally is in which slot)
        lineupDictionary[slotButton] = selectedAlly;

        // ✅ Stop pre-selection if 4 slots are filled
        if (lineupDictionary.Count >= 4)
        {
            selectedAlly = null; // Clear selection (no more selections allowed)
        }
    }

    // Dummy Power Level Dictionary (higher is stronger)
    private Dictionary<string, int> powerLevels = new Dictionary<string, int>
    {
        { "03", 1200 }, { "06", 1000 }, { "08", 1400 }, { "11", 1200 },
        { "16", 900 }, { "19", 1300 }
    };

    public void AutoDeploy()
    {
        List<(string index, string name, int power, int stars, GameObject allyItem)> availableAllies = new List<(string, string, int, int, GameObject)>();

        // ✅ Collect all unlocked allies
        foreach (var ally in unlockedAllies)
        {
            string allyIndex = ally.index;
            string allyName = ally.name;
            int power = powerLevels.ContainsKey(allyIndex) ? powerLevels[allyIndex] : 0;
            int stars = starLevels.ContainsKey(allyIndex) ? starLevels[allyIndex] : 0;

            // ✅ Find the corresponding AllyItem GameObject
            GameObject allyItem = GameObject.Find($"AllyItem_{allyIndex}");
            if (allyItem == null) continue;

            // ✅ Skip allies that are already in the lineup
            if (officiallySelectedAllies.Contains(allyItem)) continue;

            // ✅ Add to sorting list
            availableAllies.Add((allyIndex, allyName, power, stars, allyItem));
        }

        // ✅ Sort by PowerLevel (descending), then StarLevel (descending)
        availableAllies.Sort((a, b) =>
        {
            int powerComparison = b.power.CompareTo(a.power); // Higher power first
            return powerComparison != 0 ? powerComparison : b.stars.CompareTo(a.stars); // Higher stars if power is the same
        });

        // ✅ Get empty slots
        List<Button> emptySlots = new List<Button> { slot1, slot2, slot3, slot4 }
            .FindAll(slot => !lineupDictionary.ContainsKey(slot));

        // ✅ Assign top allies to available slots
        for (int i = 0; i < emptySlots.Count && i < availableAllies.Count; i++)
        {
            GameObject allyItem = availableAllies[i].allyItem;
            selectedAlly = allyItem; // Simulate selecting the ally
            EnableBattleIcon(emptySlots[i]); // Assign to the slot

            // ✅ Apply grey effect (officially selected)
            SetAllyColor(allyItem, new Color(0.4f, 0.4f, 0.4f, 1f)); // Dark grey
            officiallySelectedAllies.Add(allyItem);
        }
    }

    public void OpenLineup()
    {
        lineupPopup.SetActive(true);

        // ✅ Remove all old listeners before adding new ones
        slot1.onClick.RemoveAllListeners();
        slot2.onClick.RemoveAllListeners();
        slot3.onClick.RemoveAllListeners();
        slot4.onClick.RemoveAllListeners();

        // Attach event listeners when the lineup opens
        slot1.onClick.AddListener(() => EnableBattleIcon(slot1));
        slot2.onClick.AddListener(() => EnableBattleIcon(slot2));
        slot3.onClick.AddListener(() => EnableBattleIcon(slot3));
        slot4.onClick.AddListener(() => EnableBattleIcon(slot4)); 

        // ✅ Check if all slots are empty (no active children)
        if (AreAllSlotsEmpty())
        {
            Debug.Log("🔄 Slots are empty! Restoring lineup from saved data.");
            LoadLineup();
        }
        else
        {
            Debug.Log("✅ Lineup is still intact, no need to reload.");
        }  
    }

    private bool AreAllSlotsEmpty()
    {
        return !slot1.transform.Find("battleIcon").gameObject.activeSelf &&
            !slot2.transform.Find("battleIcon").gameObject.activeSelf &&
            !slot3.transform.Find("battleIcon").gameObject.activeSelf &&
            !slot4.transform.Find("battleIcon").gameObject.activeSelf;
    }

    private void LoadLineup()
    {
        if (!PlayerPrefs.HasKey("SavedLineup")) return;

        string savedData = PlayerPrefs.GetString("SavedLineup");
        string[] savedAllies = savedData.Split(',');

        Debug.Log($"📂 Loading Saved Lineup: {savedData}");

        foreach (string entry in savedAllies)
        {
            string[] parts = entry.Split(':');
            if (parts.Length != 2) continue;

            string slotName = parts[0];  // e.g., "Slot_1"
            string allyIndex = parts[1]; // e.g., "03"

            GameObject allyItem = GameObject.Find($"AllyItem_{allyIndex}");
            if (allyItem == null)
            {
                Debug.LogWarning($"❌ AllyItem_{allyIndex} not found in scene!");
                continue;
            }

            // ✅ Find the correct slot
            Button slotButton = null;
            if (slotName == "Slot_1") slotButton = slot1;
            if (slotName == "Slot_2") slotButton = slot2;
            if (slotName == "Slot_3") slotButton = slot3;
            if (slotName == "Slot_4") slotButton = slot4;

            if (slotButton != null)
            {
                selectedAlly = allyItem;  // Simulate selection
                EnableBattleIcon(slotButton); // ✅ Assign ally to the slot properly

                // ✅ Grey out the officially selected AllyItem
                officiallySelectedAllies.Add(allyItem);
                SetAllyColor(allyItem, new Color(0.4f, 0.4f, 0.4f, 1f)); // Dark grey
            }
            else
            {
                Debug.LogWarning($"❌ Slot '{slotName}' not found in script!");
            }
        }
    }

    public void CloseLineup()
    {
        lineupPopup.SetActive(false);
        SaveLineup();
    }

    private void SaveLineup()
    {
        List<string> lineupData = new List<string>();

        foreach (var slot in lineupDictionary.Keys)
        {
            GameObject ally = lineupDictionary[slot];
            string allyIndex = ally.name.Split('_')[1]; // Extract ally ID
            lineupData.Add($"{slot.name}:{allyIndex}"); // Format: "Slot1:03"
        }

        PlayerPrefs.SetString("SavedLineup", string.Join(",", lineupData));
        PlayerPrefs.Save();

        Debug.Log($"💾 Saved Lineup: {string.Join(", ", lineupData)}");
    }
}