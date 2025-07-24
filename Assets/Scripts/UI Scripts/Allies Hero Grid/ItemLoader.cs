using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq; // For LINQ operations
using model; // For IEnumerator and coroutines
using TMPro;
using PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.PopUpBox; // For EquipmentDismantleManager
using PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.Allies_Menu; // For AlliesEquipments
using WebSocket; // For EquipmentWebSocketApi
using Newtonsoft.Json.Linq; // For JObject handling

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
            FindObjectOfType<AlliesBlockSetup>()?.UpdateTotalBlocks();
        }
        else if (menuController.IsMenuActive(1)) // Hero Menu
        {
            FindObjectOfType<HeroBlockSetup>()?.UpdateTotalBlocks();
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
                    actionButtonText.text = "Auto Equip";
                    actionButton.onClick.RemoveAllListeners(); // Remove any previous listeners
                    actionButton.onClick.AddListener(OpenAutoEquipPage); // Placeholder for Auto Equip action
                    break;

                case ItemType.Gem:
                    actionButton.gameObject.SetActive(true); // Ensure the button is active
                    actionButtonText.text = "Auto Embed";
                    actionButton.onClick.RemoveAllListeners(); // Remove any previous listeners
                    actionButton.onClick.AddListener(OpenAutoEmbedPage); // Placeholder for Auto Embed action
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
        Debug.Log("[ItemLoader] ✅ Auto Equip button clicked - starting auto equip process");
        
        // Get current sidekick ID from AlliesGridSetup
        int currentSidekickId = GetCurrentSidekickId();
        if (currentSidekickId == 0)
        {
            Debug.LogWarning("[ItemLoader] No current sidekick selected - cannot auto equip");
            return;
        }
        
        Debug.Log($"[ItemLoader] Auto equipping for sidekick ID: {currentSidekickId}");
        
        // Check all equipment slots for upgrades or empty slots
        string[] equipmentSlots = { "Helm", "Shoulder", "Chest", "Pants", "Gloves", "Boots" };
        List<(string slotType, Equipment currentEquipment, Equipment bestEquipment)> upgradeActions = new List<(string, Equipment, Equipment)>();
        
        foreach (string slot in equipmentSlots)
        {
            Equipment currentEquipment = GetCurrentlyEquippedForSidekick(slot, currentSidekickId);
            Equipment bestEquipment = FindBestEquipmentInPack(slot, currentEquipment);
            
            if (bestEquipment != null)
            {
                if (currentEquipment == null)
                {
                    Debug.Log($"[ItemLoader] Found empty slot: {slot} - will equip {bestEquipment.Name}");
                    upgradeActions.Add((slot, null, bestEquipment));
                }
                else if (IsEquipmentBetter(bestEquipment, currentEquipment))
                {
                    Debug.Log($"[ItemLoader] Found upgrade for {slot}: {currentEquipment.Name} -> {bestEquipment.Name}");
                    upgradeActions.Add((slot, currentEquipment, bestEquipment));
                }
                else
                {
                    Debug.Log($"[ItemLoader] No upgrade available for {slot}: {currentEquipment.Name} is already the best");
                }
            }
            else
            {
                if (currentEquipment == null)
                {
                    Debug.Log($"[ItemLoader] No equipment available for empty slot: {slot}");
                }
                else
                {
                    Debug.Log($"[ItemLoader] No better equipment available for {slot}: {currentEquipment.Name}");
                }
            }
        }
        
        if (upgradeActions.Count == 0)
        {
            Debug.Log("[ItemLoader] No equipment upgrades or empty slots to fill");
            return;
        }
        
        Debug.Log($"[ItemLoader] Found {upgradeActions.Count} equipment actions to perform");
        
        // Track pending operations for UI refresh
        int pendingOperations = upgradeActions.Count;
        
        // Perform all upgrade actions
        foreach (var action in upgradeActions)
        {
            if (action.currentEquipment != null)
            {
                Debug.Log($"[ItemLoader] Upgrading {action.slotType}: {action.currentEquipment.Name} -> {action.bestEquipment.Name}");
            }
            else
            {
                Debug.Log($"[ItemLoader] Equipping to empty {action.slotType}: {action.bestEquipment.Name}");
            }
            
            AutoEquipSingleItem(action.bestEquipment, currentSidekickId, () => {
                pendingOperations--;
                if (pendingOperations <= 0)
                {
                    // All operations completed - refresh UI
                    Debug.Log("[ItemLoader] All auto equip operations completed - refreshing UI");
                    StartCoroutine(RefreshEquipmentUIAfterAutoEquip());
                }
            });
        }
    }

    private void OpenAutoEmbedPage()
    {
        // Logic for Auto Embed (to be implemented)
        Debug.Log("Opening Auto Embed Page...");
    }

    private void OpenDismantlePage()
    {
        // Use the new EquipmentDismantleManager instead of HeroBlockSetup
        if (EquipmentDismantleManager.Instance != null)
        {
            EquipmentDismantleManager.Instance.OpenDismantlePage(EquipmentDismantleManager.DismantleContext.Hero);
            Debug.Log("Opening Dismantle Page via EquipmentDismantleManager...");
        }
        else
        {
            Debug.LogError("❌ EquipmentDismantleManager.Instance is null!");
        }
    }

    private void OpenGemMergePage()
    {
        // Call OpenGemMergePage in HeroMenuPopup
        if (heroMenuPopup != null)
        {
            heroMenuPopup.OpenGemMergePage();
        }
        else
        {
            Debug.LogError("HeroMenuPopup is not assigned!");
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
            contentPanel = FindObjectOfType<AlliesBlockSetup>()?.contentPanel;
        }
        else if (menuController.IsMenuActive(1)) // Hero Menu
        {
            contentPanel = FindObjectOfType<HeroBlockSetup>()?.contentPanel;
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
    /// Check if there's currently equipped equipment for a specific part and sidekick
    /// </summary>
    private Equipment GetCurrentlyEquippedForSidekick(string equipmentPart, int sidekickId)
    {
        if (PlayerProfile.Data?.Player?.Equipments == null)
        {
            return null;
        }

        Equipment currentEquipment = PlayerProfile.Data.Player.Equipments.Find(equipment =>
            equipment.Part == equipmentPart && equipment.EquipWithSidekickId == sidekickId);

        return currentEquipment;
    }

    /// <summary>
    /// Find the best available equipment in pack for a specific slot type
    /// Priority: Higher type number (Helm_05 > Helm_03 > Helm_01)
    /// Secondary: Equipment ID for same type (placeholder for power values)
    /// </summary>
    private Equipment FindBestEquipmentInPack(string slotType, Equipment currentEquipment = null)
    {
        List<Equipment> packEquipments = PlayerProfile.Data.GetEquipmentsInPack();
        if (packEquipments == null || packEquipments.Count == 0)
        {
            return null;
        }

        // Filter equipment by slot type
        List<Equipment> suitableEquipments = packEquipments.Where(eq => eq.Part == slotType).ToList();
        
        if (suitableEquipments.Count == 0)
        {
            return null;
        }

        Debug.Log($"[ItemLoader] Found {suitableEquipments.Count} {slotType} equipment(s) in pack");

        // Sort by equipment ranking: Primary = type number, Secondary = equipment ID
        Equipment bestEquipment = suitableEquipments.OrderByDescending(eq => GetEquipmentTypeNumber(eq.Name))
                                                   .ThenByDescending(eq => eq.Id) // Placeholder for power value ranking
                                                   .First();

        // If there's no current equipment, return the best from pack
        if (currentEquipment == null)
        {
            Debug.Log($"[ItemLoader] Selected best {slotType} for empty slot: {bestEquipment.Name} (ID: {bestEquipment.Id}, Type: {GetEquipmentTypeNumber(bestEquipment.Name)})");
            return bestEquipment;
        }

        // If there's current equipment, only return if pack equipment is better
        if (IsEquipmentBetter(bestEquipment, currentEquipment))
        {
            Debug.Log($"[ItemLoader] Found better {slotType}: {bestEquipment.Name} (Type: {GetEquipmentTypeNumber(bestEquipment.Name)}) > {currentEquipment.Name} (Type: {GetEquipmentTypeNumber(currentEquipment.Name)})");
            return bestEquipment;
        }

        Debug.Log($"[ItemLoader] No better {slotType} found in pack than currently equipped {currentEquipment.Name}");
        return null;
    }

    /// <summary>
    /// Extract the type number from equipment name (e.g., "Helm_05" returns 5)
    /// </summary>
    private int GetEquipmentTypeNumber(string equipmentName)
    {
        if (string.IsNullOrEmpty(equipmentName))
            return 0;

        string[] parts = equipmentName.Split('_');
        if (parts.Length >= 2 && int.TryParse(parts[1], out int typeNumber))
        {
            return typeNumber;
        }

        return 0;
    }

    /// <summary>
    /// Compare two equipment items to determine if one is better than the other
    /// Priority: Higher type number (Helm_05 > Helm_03 > Helm_01)
    /// For same type equipment, they are considered equal (not better)
    /// </summary>
    private bool IsEquipmentBetter(Equipment candidate, Equipment current)
    {
        if (candidate == null || current == null)
            return false;

        int candidateType = GetEquipmentTypeNumber(candidate.Name);
        int currentType = GetEquipmentTypeNumber(current.Name);

        // Primary comparison: type number - only higher type is considered better
        if (candidateType > currentType)
            return true;

        // For same or lower type, candidate is not better
        return false;
    }

    /// <summary>
    /// Auto equip a single equipment item to the current sidekick
    /// </summary>
    private void AutoEquipSingleItem(Equipment equipment, int sidekickId, System.Action onComplete = null)
    {
        if (equipment == null)
        {
            Debug.LogError("[ItemLoader] Cannot auto equip - equipment is null");
            onComplete?.Invoke();
            return;
        }

        EquipmentWebSocketApi equipmentApi = EquipmentWebSocketApi.Instance;
        if (equipmentApi == null)
        {
            Debug.LogError("[ItemLoader] EquipmentWebSocketApi.Instance is null - cannot auto equip");
            onComplete?.Invoke();
            return;
        }

        var apiParams = new
        {
            type = "sidekick",
            sidekickId = sidekickId,
            equipmentId = equipment.Id
        };

        Debug.Log($"[ItemLoader] Auto equip API call - type: sidekick, sidekickId: {sidekickId}, equipmentId: {equipment.Id}");
        
        // Use the dedicated Equip API for auto equipping
        equipmentApi.Action("equip", apiParams, (response) => {
            Debug.Log($"[ItemLoader] Auto equip response received for {equipment.Name}");
            
            // Update PlayerProfile from server response (same as EquipmentDetailManager)
            UpdatePlayerProfileFromResponse(response, equipment.Name);
            
            // Call completion callback
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// Update PlayerProfile from server response (copied from EquipmentDetailManager)
    /// </summary>
    private void UpdatePlayerProfileFromResponse(Newtonsoft.Json.Linq.JObject response, string equipmentName)
    {
        if (response == null)
        {
            Debug.LogError($"[ItemLoader] ❌ Server response is null for {equipmentName}!");
            return;
        }
        
        // Check for the correct response structure: response["player_profile"]["Player"]
        if (response["player_profile"] == null)
        {
            Debug.LogError($"[ItemLoader] ❌ Server response missing 'player_profile' field for {equipmentName}. Response keys: {string.Join(", ", response.Properties().Select(p => p.Name))}");
            return;
        }
        
        if (response["player_profile"]["Player"] == null)
        {
            Debug.LogError($"[ItemLoader] ❌ Server response missing 'Player' field in player_profile for {equipmentName}. player_profile keys: {string.Join(", ", response["player_profile"].Cast<Newtonsoft.Json.Linq.JProperty>().Select(p => p.Name))}");
            return;
        }
        
        try
        {
            PlayerProfile.Data.SetPlayer(response["player_profile"]["Player"].ToObject<model.Player>());
            Debug.Log($"[ItemLoader] ✅ Player profile updated successfully from server response for {equipmentName}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemLoader] ❌ Error updating player profile for {equipmentName}: {ex.Message}");
        }
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
            FindObjectOfType<AlliesBlockSetup>()?.UpdateTotalBlocks();
        }
    }
    
}
