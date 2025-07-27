using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq; // For LINQ operations
using model; // For IEnumerator and coroutines
using TMPro;
using PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.PopUpBox; // For EquipmentDismantleManager and GemMergePageManager
using PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.Allies_Menu; // For AlliesEquipments
using EquipmentUtils; // For AutoEquipUtility

public class ItemLoader : MonoBehaviour
{
    public enum ItemType { Equipment, Gem, Other }
    public ItemType currentItemType = ItemType.Equipment;

    public Dictionary<ItemType, int> itemCounts = new Dictionary<ItemType, int>
    {
        { ItemType.Equipment, 13 },
        { ItemType.Gem, 8 },
        { ItemType.Other, 5 }
    };
    public static Dictionary<int, Color> quantityColor = new Dictionary<int, Color>
    {
        {1, Color.white},
        {2, Color.green},
        {3, Color.blue},
        {4, Color.magenta},
        {5, Color.yellow},
        {6, Color.red},
    };

    public string resourcesPath = "ItemImages";
    public delegate void OnItemCountChanged(int itemCount); // Event for data transfer
    public event OnItemCountChanged ItemCountChanged;

    private MenuController menuController; // Declare MenuController variable

    // Action button and its text
    public Button actionButton;
    public TextMeshProUGUI actionButtonText;

    // Reference to HeroMenuPopup to open the Gem Merge page
    public HeroMenuPopup heroMenuPopup;

    void Awake()
    {
        menuController = FindObjectOfType<MenuController>();
    }

    public void SwitchItemType(ItemType itemType)
    {
        Debug.Log($"SwitchItemType called with {itemType} (Before: {currentItemType})");
        currentItemType = itemType;
        Debug.Log($"Updated currentItemType: {currentItemType}");
        
        // Dynamically update action button based on selected item type
        UpdateActionButton(itemType);

        if (menuController == null)
        {
            Debug.LogError("menuController is NULL in SwitchItemType!");
            return;
        }

        if (menuController.IsMenuActive(0)) // Allies Menu
        {
            var alliesBlockSetup = FindObjectOfType<AlliesBlockSetup>();
            if (alliesBlockSetup != null)
            {
                alliesBlockSetup.UpdateTotalBlocks();
            }
            else
            {
                Debug.LogWarning("[ItemLoader] AlliesBlockSetup not found when switching to Allies Menu");
            }
        }
        else if (menuController.IsMenuActive(1)) // Hero Menu
        {
            var heroBlockSetup = FindObjectOfType<HeroBlockSetup>();
            if (heroBlockSetup != null)
            {
                heroBlockSetup.UpdateTotalBlocks();
            }
            else
            {
                Debug.LogWarning("[ItemLoader] HeroBlockSetup not found when switching to Hero Menu");
            }
        }
    }

    private void UpdateActionButton(ItemType itemType)
    {
        // Update action button text and listener based on the selected item type
        if (menuController.IsMenuActive(0)) // Allies Menu
        {
            // Allies Menu
            switch (itemType)
            {
                case ItemType.Equipment:
                    actionButton.gameObject.SetActive(true); // Ensure the button is active
                    actionButtonText.text = "Dismantle";
                    actionButton.onClick.RemoveAllListeners(); // Remove any previous listeners
                    actionButton.onClick.AddListener(OpenDismantlePage); // Changed from Auto Equip to Dismantle
                    break;

                case ItemType.Gem:
                    actionButton.gameObject.SetActive(true); // Ensure the button is active
                    actionButtonText.text = "Auto Merge";
                    actionButton.onClick.RemoveAllListeners(); // Remove any previous listeners
                    actionButton.onClick.AddListener(OpenGemMergePage); // Changed from Auto Embed to Auto Merge
                    break;
            }
        }
        else if (menuController.IsMenuActive(1)) // Hero Menu
        {
            // Hero Menu
            switch (itemType)
            {
                case ItemType.Equipment:
                    actionButton.gameObject.SetActive(true); // Show the button
                    actionButtonText.text = "Dismantle";
                    actionButton.onClick.RemoveAllListeners(); // Remove any previous listeners
                    actionButton.onClick.AddListener(OpenDismantlePage); // Add listener for dismantle action
                    break;

                case ItemType.Gem:
                    actionButton.gameObject.SetActive(true); // Show the button
                    actionButtonText.text = "Auto Merge";
                    actionButton.onClick.RemoveAllListeners(); // Remove any previous listeners
                    actionButton.onClick.AddListener(OpenGemMergePage); // Add listener for auto merge action
                    break;

                case ItemType.Other:
                    actionButton.gameObject.SetActive(false); // Hide the button
                    break;
            }
        }
    }

    private void OpenAutoEquipPage()
    {
        Debug.Log("[ItemLoader] ✅ Auto Equip button clicked - starting auto equip process for Ally");
        
        // Get current sidekick ID from AlliesGridSetup
        int currentSidekickId = GetCurrentSidekickId();
        if (currentSidekickId == 0)
        {
            Debug.LogWarning("[ItemLoader] No current sidekick selected - cannot auto equip");
            return;
        }
        
        Debug.Log($"[ItemLoader] Auto equipping for sidekick ID: {currentSidekickId}");
        
        // Use the shared AutoEquipUtility for allies
        AutoEquipUtility.AutoEquipAll(AutoEquipUtility.EquipContext.Ally, currentSidekickId, () => {
            Debug.Log("[ItemLoader] Auto equip completed - refreshing Ally UI");
            StartCoroutine(RefreshEquipmentUIAfterAutoEquip());
        });
    }

    private void OpenAutoEmbedPage()
    {
        // Logic for Auto Embed (to be implemented)
        Debug.Log("Opening Auto Embed Page...");
    }

    private void OpenDismantlePage()
    {
        // Use EquipmentDismantleManager with appropriate context
        if (EquipmentDismantleManager.Instance != null)
        {
            // Determine context based on which menu is active
            if (menuController.IsMenuActive(0)) // Allies Menu
            {
                EquipmentDismantleManager.Instance.OpenDismantlePage(EquipmentDismantleManager.DismantleContext.Ally);
                Debug.Log("[ItemLoader] Opening Dismantle Page for Allies via EquipmentDismantleManager...");
            }
            else // Hero Menu
            {
                EquipmentDismantleManager.Instance.OpenDismantlePage(EquipmentDismantleManager.DismantleContext.Hero);
                Debug.Log("[ItemLoader] Opening Dismantle Page for Hero via EquipmentDismantleManager...");
            }
        }
        else
        {
            Debug.LogError("❌ EquipmentDismantleManager.Instance is null!");
        }
    }

    private void OpenGemMergePage()
    {
        // Use the independent GemMergePageManager for both Hero and Allies menus
        if (GemMergePageManager.Instance != null)
        {
            GemMergePageManager.Instance.OpenGemMergePage();
            Debug.Log("[ItemLoader] Opening Gem Merge Page via GemMergePageManager");
        }
        else
        {
            Debug.LogError("[ItemLoader] GemMergePageManager.Instance not found - cannot open gem merge page");
        }
    }

    private void OpenOtherPage()
    {
        // Logic for Other Action (to be implemented)
        Debug.Log("Opening Other Page...");
    }

    private IEnumerator PopulateItemsDelayed()
    {
        yield return null; // Wait one frame to ensure all UI elements are initialized
        PopulateItems();
    }
    

    public void PopulateItems()
    {
        int itemCount = PlayerPrefs.GetInt("TotalItemsCount", itemCounts[currentItemType]); // Get the actual number of items

        // Set resourcesPath based on currentItemType
        switch (currentItemType)
        {
            case ItemType.Equipment:
                resourcesPath = "UILoading/Equipment"; // Equipment path
                break;
            case ItemType.Gem:
                resourcesPath = "UILoading/Gem/Stone"; // Gem path
                break;
            case ItemType.Other:
                resourcesPath = "UILoading/Other"; // Other path
                break;
        }

        // Determine the correct contentPanel based on the active menu
        Transform contentPanel = null;

        if (menuController.IsMenuActive(0)) // Allies Menu
        {
            var alliesBlockSetup = FindObjectOfType<AlliesBlockSetup>();
            if (alliesBlockSetup != null)
            {
                contentPanel = alliesBlockSetup.contentPanel;
            }
            else
            {
                Debug.LogWarning("[ItemLoader] AlliesBlockSetup not found when populating items");
                return;
            }
        }
        else if (menuController.IsMenuActive(1)) // Hero Menu
        {
            var heroBlockSetup = FindObjectOfType<HeroBlockSetup>();
            if (heroBlockSetup != null)
            {
                contentPanel = heroBlockSetup.contentPanel;
            }
            else
            {
                Debug.LogWarning("[ItemLoader] HeroBlockSetup not found when populating items");
                return;
            }
        }

        if (contentPanel == null)
        {
            Debug.LogWarning("[ItemLoader] contentPanel is null - cannot populate items");
            return;
        }

        // Iterate over each block in the contentPanel
        foreach (Transform block in contentPanel)
        {
            int blockIndex = block.GetSiblingIndex();

            // Fill only blocks for actual items
            if (blockIndex < itemCount)
            {
                // Generate random item details
                var (itemName, quantity) = GenerateRandomItemDetails();

                // Assign sprite to the Image based on resourcesPath
                Sprite itemSprite = Resources.Load<Sprite>($"{resourcesPath}/{itemName}");
                if (itemSprite != null)
                {
                    Image blockImage = block.Find("Image").GetComponent<Image>();
                    blockImage.sprite = itemSprite;
                    blockImage.color = Color.white; // Ensure the color is not transparent
                }
                else
                {
                    Debug.LogWarning($"Image not found for: {itemName}");
                }

                // Assign quantity to the Qnty child
                Transform qntyTransform = block.Find("Qnty");
                if (qntyTransform != null)
                {
                    TextMeshProUGUI qntyText = qntyTransform.GetComponent<TextMeshProUGUI>();
                    if (qntyText != null)
                    {
                        qntyText.text = quantity.ToString();
                    }
                }

                // Load part image for Gem items only
                if (currentItemType == ItemType.Gem)
                {
                    string[] equipmentParts = { "Helm", "Shoulder", "Chest", "Pants", "Gloves", "Boots" };
                    string partName = equipmentParts[Random.Range(0, equipmentParts.Length)]; // Pick a random part name
                    Sprite partSprite = Resources.Load<Sprite>($"UILoading/Gem/Part/{partName}");
                    if (partSprite != null)
                    {
                        Image partImage = block.Find("Part").GetComponent<Image>();
                        partImage.sprite = partSprite;
                        partImage.color = Color.white; // Ensure the color is not transparent
                    }
                    else
                    {
                        Debug.LogWarning($"Part image not found for: {partName}");
                    }
                }
            }
            else
            {
                // Clear the image and set it to transparent
                Transform imageTransform = block.Find("Image");
                if (imageTransform != null)
                {
                    Image blockImage = imageTransform.GetComponent<Image>();
                    if (blockImage != null)
                    {
                        blockImage.sprite = null;
                        blockImage.color = new Color(0, 0, 0, 0); // Set to transparent
                    }
                }

                // Clear quantity text
                Transform qntyTransform = block.Find("Qnty");
                if (qntyTransform != null)
                {
                    TextMeshProUGUI qntyText = qntyTransform.GetComponent<TextMeshProUGUI>();
                    if (qntyText != null)
                    {
                        qntyText.text = "";
                    }
                }

                // Clear part image if not Gem
                Transform partTransform = block.Find("Part");
                if (partTransform != null && currentItemType != ItemType.Gem)
                {
                    Image partImage = partTransform.GetComponent<Image>();
                    partImage.sprite = null; // Set to transparent
                    partImage.color = new Color(0, 0, 0, 0); // Set to fully transparent
                }
            }
        }
    }

    public (string itemName, int quantity) GenerateRandomItemDetails()
    {
        string itemName = "";
        int quantity = Random.Range(1, 10); // Quantity 1-9
    
        if (currentItemType == ItemType.Equipment)
        {
            string[] equipmentNames = { "Helm", "Shoulder", "Chest", "Pants", "Gloves", "Boots" };
            itemName = equipmentNames[Random.Range(0, equipmentNames.Length)];
            int itemLevel = Random.Range(1, 7); // Levels 1-6
            itemName = $"{itemName}_{itemLevel:D2}"; // Equipment name like "Helm_01"
        }
        else if (currentItemType == ItemType.Gem)
        {
            itemName = $"Gem_{Random.Range(1, 8):D2}"; // Gem name like "Gem_01", "Gem_02", ...
        }
        else if (currentItemType == ItemType.Other)
        {
            itemName = "OtherItem"; // Define a different name for "Other"
        }
    
        return (itemName, quantity);
    }
    
    
    public void LoadEquipmentItems(Transform block, Equipment equipment)
    {

        var itemName = equipment.Name;
        var quantity = equipment.Quality;
        resourcesPath = "UILoading/Equipment";

        // Assign sprite to the Image based on resourcesPath
        Sprite itemSprite = Resources.Load<Sprite>($"{resourcesPath}/{itemName}");
        if (itemSprite != null)
        {
            Image blockImage = block.Find("Image").GetComponent<Image>();
            blockImage.sprite = itemSprite;
            blockImage.color = Color.white; // Ensure the color is not transparent
            block.GetComponent<Image>().color = quantityColor.GetValueOrDefault(quantity, Color.white); 
        }
        else
        {
            Debug.LogWarning($"Image not found for: {itemName}");
        }

        // Assign quantity to the Qnty child
        Transform qntyTransform = block.Find("Qnty");
        if (qntyTransform != null)
        {
            TextMeshProUGUI qntyText = qntyTransform.GetComponent<TextMeshProUGUI>();
            qntyText.text = null;
        }
    }

    public void LoadGemItems(Transform block, Gemstone gemstone)
    {
        var itemName = $"Gem_{gemstone.Level:D2}";
        resourcesPath = "UILoading/Gem/Stone";
        Sprite itemSprite = Resources.Load<Sprite>($"{resourcesPath}/{itemName}");
        if (itemSprite != null)
        {
            Image blockImage = block.Find("Image").GetComponent<Image>();
            blockImage.sprite = itemSprite;
            blockImage.color = Color.white; // Ensure the color is not transparent
            Image background = block.GetComponent<Image>();
            background.color = Color.white; 
        }
        else
        {
            Debug.LogWarning($"Image not found for: {itemName}");
        }
        Sprite partSprite = Resources.Load<Sprite>($"UILoading/Gem/Part/{gemstone.Part}");
        if (partSprite != null)
        {
            Image partImage = block.Find("Part").GetComponent<Image>();
            partImage.sprite = partSprite;
            // partImage.color = quantityColor.GetValueOrDefault(quantity, Color.white); 
            partImage.color = Color.white; 
        }
        else
        {
            Debug.LogWarning($"Part image not found for: {gemstone.Part}");
        }
        // Assign quantity to the Qnty child
        Transform qntyTransform = block.Find("Qnty");
        if (qntyTransform != null)
        {
            TextMeshProUGUI qntyText = qntyTransform.GetComponent<TextMeshProUGUI>();
            if (qntyText != null)
            {
                qntyText.text = null;
            }
        }
    }

    /// <summary>
    /// Get the current sidekick ID from AlliesGridSetup
    /// </summary>
    private int GetCurrentSidekickId()
    {
        AlliesGridSetup alliesGridSetup = FindObjectOfType<AlliesGridSetup>();
        if (alliesGridSetup == null)
        {
            Debug.LogWarning("[ItemLoader] AlliesGridSetup not found - cannot determine current sidekick ID");
            return 0;
        }

        // Get current ally name using reflection
        string currentAllyName = GetCurrentAllyNameFromGridSetup(alliesGridSetup);
        if (string.IsNullOrEmpty(currentAllyName))
        {
            Debug.LogWarning("[ItemLoader] Current ally name is empty - using default sidekick ID 0");
            return 0;
        }

        // Convert ally name to sidekick ID
        if (PlayerProfile.Data?.Sidekick != null)
        {
            var sidekick = PlayerProfile.Data.Sidekick.FirstOrDefault(s =>
            {
                string allyBaseId = GetAllyBaseIdFromName(currentAllyName);
                return s.base_id == allyBaseId;
            });

            if (sidekick != null && int.TryParse(sidekick.id, out int sidekickId))
            {
                Debug.Log($"[ItemLoader] Found sidekick ID {sidekickId} for ally {currentAllyName}");
                return sidekickId;
            }
        }

        return 0;
    }

    /// <summary>
    /// Get the current ally name from AlliesGridSetup using reflection
    /// </summary>
    private string GetCurrentAllyNameFromGridSetup(AlliesGridSetup alliesGridSetup)
    {
        if (alliesGridSetup == null) return "";

        try
        {
            var currentAllyNameField = typeof(AlliesGridSetup).GetField("currentAllyName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (currentAllyNameField != null)
            {
                string currentAllyName = (string)currentAllyNameField.GetValue(alliesGridSetup);
                Debug.Log($"[ItemLoader] Retrieved current ally name: {currentAllyName}");
                return currentAllyName ?? "";
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemLoader] Error getting current ally name: {ex.Message}");
        }

        return "";
    }

    /// <summary>
    /// Convert ally name to base_id format used in sidekick data
    /// </summary>
    private string GetAllyBaseIdFromName(string allyName)
    {
        string[] characterNames = {
            "Zorath", "Gideon", "Sylas", "Aurelia", "Lyanna", "Zhara", "Elenya", "Rowan",
            "Liraen", "Cedric", "Selena", "Morgath", "Zyphira", "Kaelith", "Velan", "Ragnar",
            "Lucien", "Ugra", "Eleanor", "Nyx"
        };

        for (int i = 0; i < characterNames.Length; i++)
        {
            if (characterNames[i] == allyName)
            {
                return (i + 1).ToString();
            }
        }

        Debug.LogWarning($"[ItemLoader] Unknown ally name: {allyName}");
        return "0";
    }



    /// <summary>
    /// Refresh the equipment UI after auto equip operations complete
    /// </summary>
    private System.Collections.IEnumerator RefreshEquipmentUIAfterAutoEquip()
    {
        // Wait just one frame since PlayerProfile is now updated immediately from server responses
        yield return null;
        
        Debug.Log("[ItemLoader] Starting equipment UI refresh after auto equip");
        
        // Find and refresh the AlliesEquipments component
        var alliesEquipments = FindObjectOfType<AlliesEquipments>();
        if (alliesEquipments != null)
        {
            Debug.Log("[ItemLoader] Calling InitForCurrentAlly to refresh equipment display");
            alliesEquipments.InitForCurrentAlly();
            Debug.Log("[ItemLoader] Refreshed equipment UI after auto equip");
        }
        else
        {
            Debug.LogWarning("[ItemLoader] AlliesEquipments component not found - cannot refresh UI");
        }
        
        // Also refresh the item loader display to reflect the equipment that was moved from pack
        Debug.Log("[ItemLoader] Refreshing item pack display after auto equip");
        
        if (menuController.IsMenuActive(0)) // Allies Menu
        {
            var alliesBlockSetup = FindObjectOfType<AlliesBlockSetup>();
            if (alliesBlockSetup != null)
            {
                alliesBlockSetup.UpdateTotalBlocks();
            }
            else
            {
                Debug.LogWarning("[ItemLoader] AlliesBlockSetup not found when refreshing after auto equip");
            }
        }
    }
    
}
