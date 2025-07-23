using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using model;
using PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.Gemstones;
using PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.PopUpBox; // This is required for using Dictionary
using TMPro;
using Debug = UnityEngine.Debug;

public class HeroBlockSetup : MonoBehaviour
{
    public GridLayoutGroup grid; // Assign the GridLayoutGroup in Inspector
    public RectTransform contentPanel; // Assign the ContentPanel RectTransform
    public GameObject blockPrefab; 

    public int blocksPerRow = 5; // Fixed columns (5 per row)
    public int totalBlocks; // Total number of blocks (dynamically fetched from PlayerPrefs)

    private float blockWidth; // This will dynamically adjust the block width based on available space
    private float leftPadding;
    private float rightPadding;
    private float spacingX;
    private float spacingY;
    public GameObject heroStep2Panel; // Reference to Hero Step 2 Panel
    public GameObject allyStep2Panel; // Reference to Ally Step 2 Panel

    [SerializeField]
    private ItemLoader itemLoader;


    // A dictionary to store the mapping between gem image file names and their localized names
    private Dictionary<string, string> gemNameLocalization = new Dictionary<string, string>
    {
        { "Gem_01", "Common Gem" },
        { "Gem_02", "Superior Gem" },
        { "Gem_03", "Rare Gem" },
        { "Gem_04", "Epic Gem" },
        { "Gem_05", "Legendary Gem" },
        { "Gem_06", "Mythic Gem" },
        { "Gem_07", "Ultimate Gem" }
    };

    // Flag to track if we need to refresh pack UI when GameObject becomes active
    private bool needsRefreshOnEnable = false;

    void Start()
    {
        // Initially update the grid
        UpdateTotalBlocks(); // Fetch and update the grid layout based on TotalItemsCount.
        
        // Listen to specific notifications - Equipment and Gem tabs should only refresh 
        // when equipment or gems are actually added, not for hero draws (shards)
        PlayerProfile.Data.AddListener(UpdateUI, "Equipments");
        PlayerProfile.Data.AddListener(UpdateUI, "Gemstones");
        
        // Also listen to OtherItems for the Others tab refresh
        PlayerProfile.Data.AddListener(UpdateOthersTabOnly, "OtherItems");
    }
    
    private void OnEnable()
    {
        // If we have a pending refresh, do it now that the GameObject is active
        if (needsRefreshOnEnable)
        {
            needsRefreshOnEnable = false;
            StartCoroutine(DelayedUpdateTotalBlocks());
        }
    }
    
    private void OnDestroy()
    {
        // Clean up listeners to prevent memory leaks
        if (PlayerProfile.Data != null)
        {
            PlayerProfile.Data.RemoveListener(UpdateUI, "Equipments");
            PlayerProfile.Data.RemoveListener(UpdateUI, "Gemstones");
            PlayerProfile.Data.RemoveListener(UpdateOthersTabOnly, "OtherItems");
        }
    }

    private void UpdateUI(ApplicationModel model)
    {
        // Check if the GameObject is active before starting coroutine
        if (gameObject.activeInHierarchy)
        {
            // Add a small delay to ensure player data is fully updated before refreshing pack UI
            StartCoroutine(DelayedUpdateTotalBlocks());
        }
        else
        {
            // If the GameObject is inactive, mark that we need to update when it becomes active
            needsRefreshOnEnable = true;
        }
    }
    
    private void UpdateOthersTabOnly(ApplicationModel model)
    {
        // Only refresh the Others tab when OtherItems change (e.g., from hero draws)
        UpdateOthersTab();
    }
    
    private System.Collections.IEnumerator DelayedUpdateTotalBlocks()
    {
        // Wait one frame to ensure all data updates are complete
        yield return null;
        UpdateTotalBlocks();
    }
    
    public void UpdateTotalBlocks()
    {
        if (itemLoader.currentItemType == ItemLoader.ItemType.Equipment)
        {
            var equipments = PlayerProfile.Data.GetEquipmentsInPack();
            totalBlocks = equipments?.Count ?? 0;
            Debug.Log($"[HeroBlockSetup] Equipment count: {totalBlocks}");
        }
        else if (itemLoader.currentItemType == ItemLoader.ItemType.Gem)
        {
            var gemstones = PlayerProfile.Data.GetGemstonesInPack();
            totalBlocks = gemstones?.Count ?? 0;
            Debug.Log($"[HeroBlockSetup] Gemstone count: {totalBlocks}");
        }

        // Update the grid layout dynamically to match the number of blocks
        UpdateGridLayout();
    }

    private void UpdateGridLayout()
    {
        // Dynamically calculate block width based on content panel width
        float panelWidth = contentPanel.rect.width;

        // Calculate block width and padding dynamically
        blockWidth = panelWidth / (blocksPerRow + 1);
        leftPadding = blockWidth * 0.25f;
        rightPadding = blockWidth * 0.25f;

        spacingX = blockWidth * 0.125f;
        spacingY = blockWidth * 0.125f;
        grid.padding.top = Mathf.RoundToInt(spacingY);

        // Adjust GridLayoutGroup settings
        grid.cellSize = new Vector2(blockWidth, blockWidth); // Apply calculated block width
        grid.spacing = new Vector2(spacingX, spacingY);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = blocksPerRow;

        // Apply padding to the grid
        grid.padding.left = Mathf.RoundToInt(leftPadding);
        grid.padding.right = Mathf.RoundToInt(rightPadding);

        // Adjust Content Panel RectTransform to stretch horizontally
        contentPanel.anchorMin = new Vector2(0, 0); // Align to the bottom-left of the parent
        contentPanel.anchorMax = new Vector2(1, 1); // Stretch horizontally and vertically
        contentPanel.offsetMin = new Vector2(leftPadding, contentPanel.offsetMin.y); // Left padding
        contentPanel.offsetMax = new Vector2(-rightPadding, contentPanel.offsetMax.y); // Right padding

        // Calculate total rows dynamically without enforcing a minimum row count
        int totalRows = Mathf.Max(1, Mathf.CeilToInt((float)totalBlocks / blocksPerRow));

        // Adjust Content size to fit the blocks
        RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
        float contentHeight = totalRows * (blockWidth + spacingY) - spacingY; // Total height based on rows and spacing
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentHeight);

        // Create the blocks dynamically based on the number of blocks
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject); // Clear previous blocks
        }
        
        Debug.Log($"[HeroBlockSetup] Creating {totalBlocks} blocks for {itemLoader.currentItemType}");

        for (int i = 0; i < totalBlocks; i++)
        {
            GameObject newBlock = Instantiate(blockPrefab, contentPanel);
            newBlock.name = $"Block_{i + 1}"; // Optional naming for easy identification

            // Add a Button Component to the Block (if not already added)
            Button blockButton = newBlock.GetComponent<Button>();
            if (blockButton == null)
            {
                blockButton = newBlock.AddComponent<Button>();
            }
            
            if (itemLoader.currentItemType == ItemLoader.ItemType.Equipment)
            {
                List<Equipment> equipments = PlayerProfile.Data.GetEquipmentsInPack();
                if (i < equipments.Count)
                {
                    Equipment equipment = equipments[i];
                    Debug.Log($"[HeroBlockSetup] Loading equipment block {i}: {equipment?.Name}");
                    itemLoader.LoadEquipmentItems(newBlock.transform, equipment);
                    blockButton.onClick.AddListener(() => 
                    {
                        // Detect context based on active panels
                        EquipmentComparisonManager.EquippedOn context = EquipmentComparisonManager.EquippedOn.Hero;
                        int contextId = 0; // Hero uses 0, Sidekick uses actual ID
                        
                        Debug.Log($"[HeroBlockSetup] Context Detection - allyStep2Panel null: {allyStep2Panel == null}");
                        if (allyStep2Panel != null)
                        {
                            Debug.Log($"[HeroBlockSetup] Context Detection - allyStep2Panel active: {allyStep2Panel.activeInHierarchy}");
                        }
                        Debug.Log($"[HeroBlockSetup] Context Detection - heroStep2Panel null: {heroStep2Panel == null}");
                        if (heroStep2Panel != null)
                        {
                            Debug.Log($"[HeroBlockSetup] Context Detection - heroStep2Panel active: {heroStep2Panel.activeInHierarchy}");
                        }
                        
                        if (allyStep2Panel != null && allyStep2Panel.activeInHierarchy)
                        {
                            context = EquipmentComparisonManager.EquippedOn.Sidekick;
                            contextId = GetCurrentSidekickIdFromAllies(); // Get sidekick ID for Ally context
                            Debug.Log($"[HeroBlockSetup] Detected Ally context - using Sidekick with ID: {contextId}");
                        }
                        else
                        {
                            Debug.Log("[HeroBlockSetup] Detected Hero context - using Hero");
                        }
                        
                        // Check if there's currently equipped equipment for this part and context
                        Equipment currentEquipment = GetCurrentlyEquippedForContext(equipment.Part, context, contextId);
                        
                        if (currentEquipment != null)
                        {
                            // Equipment slot is occupied - show comparison page
                            Debug.Log($"[HeroBlockSetup] Slot occupied by equipment ID {currentEquipment.Id} - showing comparison");
                            EquipmentComparisonManager.Instance.Init(context, equipment?.Id, contextId);
                        }
                        else
                        {
                            // Equipment slot is empty - show single equipment detail for equipping
                            Debug.Log($"[HeroBlockSetup] Slot empty - showing single detail for equipping");
                            EquipmentDetailBox.Instance.InitForEquipping(equipment, context, contextId);
                        }
                    });
                }
                
            }
            else if (itemLoader.currentItemType == ItemLoader.ItemType.Gem)
            {
                var gemstones = PlayerProfile.Data.GetGemstonesInPack();
                if (i < gemstones.Count)
                {
                    Gemstone gemstone = gemstones[i];
                    Debug.Log($"[HeroBlockSetup] Loading gem block {i}: Gem_{gemstone?.Level:D2}");
                    itemLoader.LoadGemItems(newBlock.transform, gemstone);
                    blockButton.onClick.AddListener(() => 
                    {
                        GemDetailWithInlaid.Instance.Init(gemstone, null); 
                    });
                }
            }
        }
    }

 
    // Call this when the 'Other' tab is selected to refresh the UI
    public void OnOtherTabSelected()
    {
        Debug.Log("[OtherTab] Selected. Updating Others tab UI.");
        UpdateOthersTab();
    }

    // copilot agent 2025-06-29: Update the Others tab to display all items from ItemsJson (hero only)
    public void UpdateOthersTab()
    {
        var otherItemsRaw = model.PlayerProfile.Data.GetOtherItemsInPack();
        Debug.Log($"[OtherTab] ItemsJson count: {otherItemsRaw?.Count ?? 0}");
        if (otherItemsRaw == null || otherItemsRaw.Count == 0)
        {
            Debug.LogWarning("[OtherTab] ItemsJson is empty! Add test data or check server sync.");
            return;
        }

        // 1. Exclude unwanted keys
        var excludeKeys = new HashSet<string> { "heroKey", "rareKey", "epicKey" };

        // 2. Group and sum by true item name (shard/skillbook logic)
        var grouped = new Dictionary<string, int>(); // key: fileName, value: total qnty
        var typeMap = new Dictionary<string, string>(); // key: fileName, value: "shard" or "skillbook"
        foreach (var kvp in otherItemsRaw)
        {
            string rawKey = kvp.Key;
            int qnty = kvp.Value;
            if (excludeKeys.Contains(rawKey)) continue;

            // GachaController logic: key format is like "20_Nyx" for shards, "SKb_SkillbookName" for skillbooks
            string fileName;
            string type;
            if (rawKey.StartsWith("SKb_"))
            {
                fileName = rawKey;
                type = "skillbook";
            }
            else
            {
                fileName = rawKey;
                type = "shard";
            }
            if (!grouped.ContainsKey(fileName))
            {
                grouped[fileName] = 0;
                typeMap[fileName] = type;
            }
            grouped[fileName] += qnty;
        }

        // 3. Clear existing UI
        foreach (Transform child in contentPanel)
            Destroy(child.gameObject);

        Debug.Log($"[OtherTab] Creating blocks for {grouped.Count} grouped items");

        // 4. Create blocks for each grouped item (skip items with quantity 0)
        int blocksCreated = 0;
        foreach (var kvp in grouped)
        {
            string fileName = kvp.Key;
            int totalQnty = kvp.Value;
            string type = typeMap[fileName];

            // Skip items with quantity 0 - they should not appear in the pack
            if (totalQnty <= 0)
            {
                Debug.Log($"[OtherTab] Skipping {fileName} with quantity {totalQnty}");
                continue;
            }

            blocksCreated++;
            GameObject newBlock = Instantiate(blockPrefab, contentPanel);
            newBlock.name = $"OtherItem_{fileName}";

            // Set item image
            Image image = newBlock.transform.Find("Image").GetComponent<Image>();
            image.sprite = LoadOtherItemSprite_GachaStyle(fileName, type);
            if (image.sprite != null)
            {
                image.color = Color.white;
            }
            else
            {
                image.color = Color.clear;
                Debug.LogWarning($"[OtherTab] Sprite not found for fileName: {fileName}");
            }

            // Set quantity
            TextMeshProUGUI qntyText = newBlock.transform.Find("Qnty").GetComponent<TextMeshProUGUI>();
            qntyText.text = totalQnty.ToString();

            // Add click handler for Other items
            Button blockButton = newBlock.GetComponent<Button>();
            if (blockButton == null)
            {
                blockButton = newBlock.AddComponent<Button>();
            }
            
            // Capture variables for the lambda
            string capturedFileName = fileName;
            int capturedQuantity = totalQnty;
            string capturedType = type;
            
            blockButton.onClick.AddListener(() => 
            {
                Debug.Log($"[OtherTab] Block clicked: {capturedFileName}");
                if (OtherDetailBox.Instance != null)
                {
                    OtherDetailBox.Instance.Init(capturedFileName, capturedQuantity, capturedType);
                }
                else
                {
                    Debug.LogError("[OtherTab] OtherDetailBox.Instance is null!");
                }
            });
        }
        
        Debug.Log($"[OtherTab] UpdateOthersTab complete: {blocksCreated} blocks created from {grouped.Count} total items");
    }

    // Helper: match GachaController logic for image path
    private Sprite LoadOtherItemSprite_GachaStyle(string fileName, string type)
    {
        string path = type == "skillbook"
            ? $"UILoading/CharacterImages/Skillbook/{fileName}"
            : $"UILoading/CharacterImages/Shard/{fileName}";
        Sprite s = Resources.Load<Sprite>(path);
        if (s == null) Debug.LogWarning($"[OtherTab] Sprite not found at {path}");
        return s;
    }

    // Add these references at the top of the class
    public Button equipmentTabButton;
    public Button gemTabButton;
    public Button otherTabButton;

    // Programmatically select the Equipment tab (index 0) and refresh UI
    public void SelectEquipmentTab()
    {
        if (equipmentTabButton != null)
        {
            equipmentTabButton.onClick.Invoke(); // Simulate user click for full tab logic
            Debug.Log("[HeroBlockSetup] Equipment tab selected via onClick.Invoke().");
            return;
        }
        Debug.LogWarning("[HeroBlockSetup] equipmentTabButton not assigned!");
    }

    /// <summary>
    /// Get the current sidekick ID when in Ally context by finding AlliesBlockSetup
    /// </summary>
    /// <returns>The current sidekick ID, or 0 if not found</returns>
    private int GetCurrentSidekickIdFromAllies()
    {
        // Find AlliesBlockSetup in the scene and use its method
        AlliesBlockSetup alliesBlockSetup = FindObjectOfType<AlliesBlockSetup>();
        if (alliesBlockSetup != null)
        {
            // Use reflection to call the private GetCurrentSidekickId method
            var method = alliesBlockSetup.GetType().GetMethod("GetCurrentSidekickId", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                return (int)method.Invoke(alliesBlockSetup, null);
            }
        }
        
        Debug.LogWarning("[HeroBlockSetup] Could not get current sidekick ID from AlliesBlockSetup");
        return 0;
    }

    /// <summary>
    /// Check if there's currently equipped equipment for a specific part and context
    /// </summary>
    /// <param name="equipmentPart">The equipment part to check (e.g., "Helm", "Chest")</param>
    /// <param name="context">The context (Hero or Sidekick)</param>
    /// <param name="contextId">The context ID (0 for Hero, sidekick ID for Sidekick)</param>
    /// <returns>The currently equipped Equipment, or null if no equipment is equipped</returns>
    private Equipment GetCurrentlyEquippedForContext(string equipmentPart, EquipmentComparisonManager.EquippedOn context, int contextId)
    {
        if (PlayerProfile.Data?.Player?.Equipments == null)
        {
            return null;
        }

        Equipment currentEquipment = null;
        
        if (context == EquipmentComparisonManager.EquippedOn.Hero)
        {
            // Find equipment that matches the part and is equipped to Hero
            currentEquipment = PlayerProfile.Data.Player.Equipments.Find(equipment =>
                equipment.Part == equipmentPart && equipment.EquipWithHeroId > 0);
        }
        else // Sidekick context
        {
            // Find equipment that matches the part and is equipped to this sidekick
            currentEquipment = PlayerProfile.Data.Player.Equipments.Find(equipment =>
                equipment.Part == equipmentPart && equipment.EquipWithSidekickId == contextId);
        }

        Debug.Log($"[HeroBlockSetup] Checking equipped {equipmentPart} for {context} {contextId}: {(currentEquipment != null ? $"Found ID {currentEquipment.Id}" : "None")}");
        return currentEquipment;
    }
}