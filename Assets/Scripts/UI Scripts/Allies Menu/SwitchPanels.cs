using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;
using model;
using EquipmentUtils;
using GemUtils;
using PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.Allies_Menu;

public class SwitchPanels : MonoBehaviour
{
    [Header("SEPARATE BUTTONS APPROACH - MUCH CLEANER")]
    public Button goToPackButton;     // "Go to Pack" button
    public Button autoEquipButton;    // "Auto Equip" button  
    public Button autoEmbedButton;    // "Auto Embed" button
    
    [Header("Legacy - Keep for backward compatibility")]
    public Button button;   // Assign the button in Inspector (LEGACY)
    public GameObject packPanel;  // Assign the Pack panel in Inspector
    public GameObject boardPanel; // Assign the Board panel in Inspector
    public TextMeshProUGUI buttonText; // Assign the Orange button's text component (LEGACY)

    private void Start()
    {
        // SEPARATE BUTTONS APPROACH - Much cleaner and more reliable
        if (goToPackButton != null)
        {
            goToPackButton.onClick.AddListener(SwitchToPack);
            Debug.Log("[SwitchPanels] ✅ Go to Pack button registered");
        }
        
        if (autoEquipButton != null)
        {
            autoEquipButton.onClick.AddListener(TriggerAutoEquip);
            Debug.Log("[SwitchPanels] ✅ Auto Equip button registered");
        }
        
        if (autoEmbedButton != null)
        {
            autoEmbedButton.onClick.AddListener(TriggerAutoEmbed);
            Debug.Log("[SwitchPanels] ✅ Auto Embed button registered");
        }
        
        // LEGACY: Keep old dynamic button approach for backward compatibility
        if (button != null)
        {
            button.onClick.AddListener(HandleButtonClick);
            Debug.Log("[SwitchPanels] ⚠️ Legacy dynamic button registered");
            
            // HIDE THE LEGACY BUTTON since we're using separate buttons now
            button.gameObject.SetActive(false);
            Debug.Log("[SwitchPanels] 🚫 HIDDEN legacy dynamic button - using separate buttons instead");
        }
        
        // Update button visibility based on current state
        UpdateButtonVisibility();
    }

    /// <summary>
    /// Handle Orange button click - either switch to pack or trigger auto equip/embed
    /// </summary>
    private void HandleButtonClick()
    {
        Debug.Log("[SwitchPanels] 🚨🚨🚨 HandleButtonClick called! 🚨🚨🚨");
        
        if (buttonText == null) 
        {
            Debug.LogError("[SwitchPanels] ❌ buttonText is NULL!");
            return;
        }
        
        // CRITICAL FIX: Store current tab BEFORE any event handlers can change it
        ItemLoader.ItemType preservedTab = GetCurrentTab();
        string currentText = buttonText.text;
        Debug.Log($"[SwitchPanels] Current button text: '{currentText}' (Length: {currentText.Length}), Preserved tab: {preservedTab}");
        
        // DEBUGGING: Check for exact text matches with trimming
        string trimmedText = currentText.Trim();
        Debug.Log($"[SwitchPanels] Trimmed button text: '{trimmedText}' (Length: {trimmedText.Length})");
        
        if (trimmedText == "Go to Pack")
        {
            Debug.Log("[SwitchPanels] ✅ Matched 'Go to Pack' - switching to pack");
            SwitchToPack();
        }
        else if (trimmedText == "Auto Equip")
        {
            Debug.Log("[SwitchPanels] ✅ Matched 'Auto Equip' - triggering auto equip");
            TriggerAutoEquip();
        }
        else if (trimmedText == "Auto Embed")
        {
            Debug.Log("[SwitchPanels] ✅ Matched 'Auto Embed' - forcing gem tab and triggering auto embed");
            
            // CRITICAL FIX: Ensure we stay on Gem tab for Auto Embed
            Debug.Log("[SwitchPanels] 🔥 FORCING GEM TAB BEFORE AUTO EMBED");
            ForceTabSwitch(ItemLoader.ItemType.Gem);
            
            // Trigger Auto Embed (same as red button)
            TriggerAutoEmbed();
        }
        else
        {
            Debug.LogWarning($"[SwitchPanels] ❌ UNRECOGNIZED button text: '{trimmedText}' - no action taken!");
        }
    }
    
    private void SwitchToPack()
    {
        Debug.Log("[SwitchPanels] 🎯 SwitchToPack called");
        
        if (packPanel != null) packPanel.SetActive(true);
        if (boardPanel != null) boardPanel.SetActive(false);
        
        // Update button visibility after switching to pack
        UpdateButtonVisibility();
    }
    
    /// <summary>
    /// Trigger Auto Equip functionality (same as red button)
    /// </summary>
    private void TriggerAutoEquip()
    {
        Debug.Log("[SwitchPanels] Orange Auto Equip button clicked - using direct method like Hero");
        
        // FIXED: Use direct method call instead of reflection (like Hero approach)
        // Get current sidekick ID from AlliesGridSetup
        int currentSidekickId = GetCurrentSidekickId();
        if (currentSidekickId == 0)
        {
            Debug.LogWarning("[SwitchPanels] No current sidekick selected - cannot auto equip");
            return;
        }
        
        Debug.Log($"[SwitchPanels] Auto equipping for sidekick ID: {currentSidekickId}");
        
        // Use AutoEquipUtility directly (same as ItemLoader.OpenAutoEquipPage does)
        AutoEquipUtility.AutoEquipAll(AutoEquipUtility.EquipContext.Ally, currentSidekickId, () => {
            Debug.Log("[SwitchPanels] Auto equip completed - refreshing Ally UI");
            StartCoroutine(RefreshEquipmentUIAfterAutoEquip());
        });
    }
    
    /// <summary>
    /// Trigger Auto Embed functionality - CLEAN VERSION WITHOUT TAB SWITCHING
    /// </summary>
    private void TriggerAutoEmbed()
    {
        Debug.Log("[SwitchPanels] 🔥🔥🔥 SEPARATE BUTTON AUTO EMBED CLICKED - NO TAB SWITCHING!");
        
        try
        {
            // Step 1: Find ItemLoader and use its method directly
            ItemLoader itemLoader = FindObjectOfType<ItemLoader>();
            if (itemLoader == null)
            {
                Debug.LogError("[SwitchPanels] ❌ ItemLoader not found!");
                return;
            }
            
            Debug.Log("[SwitchPanels] ✅ Found ItemLoader, calling its OpenAutoEmbedPage method via reflection");
            
            // Use reflection to call the private OpenAutoEmbedPage method
            var autoEmbedMethod = typeof(ItemLoader).GetMethod("OpenAutoEmbedPage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            if (autoEmbedMethod != null)
            {
                Debug.Log("[SwitchPanels] ✅ Found OpenAutoEmbedPage method, invoking...");
                autoEmbedMethod.Invoke(itemLoader, null);
                Debug.Log("[SwitchPanels] ✅ Successfully invoked OpenAutoEmbedPage");
            }
            else
            {
                Debug.LogError("[SwitchPanels] ❌ OpenAutoEmbedPage method not found in ItemLoader");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SwitchPanels] 💥 EXCEPTION in TriggerAutoEmbed: {ex.Message}");
            Debug.LogError($"[SwitchPanels] 💥 Stack trace: {ex.StackTrace}");
        }
    }
    
    /// <summary>
    /// Update button visibility - DEBUG VERSION TO SEE WHAT'S HAPPENING
    /// </summary>
    public void UpdateButtonVisibility()
    {
        bool isPackVisible = packPanel != null && packPanel.activeInHierarchy;
        
        // CRITICAL DEBUG: Check if separate buttons exist
        Debug.Log($"[SwitchPanels] 🔍 DEBUG: goToPackButton={goToPackButton != null}, autoEquipButton={autoEquipButton != null}, autoEmbedButton={autoEmbedButton != null}");
        Debug.Log($"[SwitchPanels] 🔍 DEBUG: packPanel={packPanel != null}, packVisible={isPackVisible}");
        
        if (isPackVisible)
        {
            // Pack is visible - check current tab (EXACTLY like legacy UpdateButtonText)
            ItemLoader.ItemType currentTab = GetCurrentTab();
            Debug.Log($"[SwitchPanels] 🔍 DEBUG: Pack visible, currentTab={currentTab}");
            
            switch (currentTab)
            {
                case ItemLoader.ItemType.Equipment:
                    // Show "Auto Equip" button, hide others
                    if (goToPackButton != null) { goToPackButton.gameObject.SetActive(false); Debug.Log("[SwitchPanels] ✅ Hid Go to Pack button"); }
                    if (autoEquipButton != null) { autoEquipButton.gameObject.SetActive(true); Debug.Log("[SwitchPanels] ✅ Showed Auto Equip button"); }
                    if (autoEmbedButton != null) { autoEmbedButton.gameObject.SetActive(false); Debug.Log("[SwitchPanels] ✅ Hid Auto Embed button"); }
                    Debug.Log("[SwitchPanels] Pack visible + Equipment tab → Show Auto Equip");
                    break;
                    
                case ItemLoader.ItemType.Gem:
                    // Show "Auto Embed" button, hide others
                    if (goToPackButton != null) { goToPackButton.gameObject.SetActive(false); Debug.Log("[SwitchPanels] ✅ Hid Go to Pack button"); }
                    if (autoEquipButton != null) { autoEquipButton.gameObject.SetActive(false); Debug.Log("[SwitchPanels] ✅ Hid Auto Equip button"); }
                    if (autoEmbedButton != null) { autoEmbedButton.gameObject.SetActive(true); Debug.Log("[SwitchPanels] ✅ Showed Auto Embed button"); }
                    Debug.Log("[SwitchPanels] Pack visible + Gem tab → Show Auto Embed");
                    break;
                    
                default:
                    // Allies menu only has Equipment and Gem - this shouldn't happen
                    Debug.LogWarning($"[SwitchPanels] Unexpected tab in Allies menu: {currentTab}");
                    if (goToPackButton != null) { goToPackButton.gameObject.SetActive(true); Debug.Log("[SwitchPanels] ✅ Showed Go to Pack button (default)"); }
                    if (autoEquipButton != null) { autoEquipButton.gameObject.SetActive(false); Debug.Log("[SwitchPanels] ✅ Hid Auto Equip button (default)"); }
                    if (autoEmbedButton != null) { autoEmbedButton.gameObject.SetActive(false); Debug.Log("[SwitchPanels] ✅ Hid Auto Embed button (default)"); }
                    break;
            }
        }
        else
        {
            // Board is visible - show "Go to Pack" button only (EXACTLY like legacy)
            Debug.Log($"[SwitchPanels] 🔍 DEBUG: Board visible");
            if (goToPackButton != null) { goToPackButton.gameObject.SetActive(true); Debug.Log("[SwitchPanels] ✅ Showed Go to Pack button (board)"); }
            if (autoEquipButton != null) { autoEquipButton.gameObject.SetActive(false); Debug.Log("[SwitchPanels] ✅ Hid Auto Equip button (board)"); }
            if (autoEmbedButton != null) { autoEmbedButton.gameObject.SetActive(false); Debug.Log("[SwitchPanels] ✅ Hid Auto Embed button (board)"); }
            Debug.Log("[SwitchPanels] Board visible → Show 'Go to Pack' button");
        }
        
        // LEGACY: Also update old dynamic button for backward compatibility
        UpdateButtonText();
    }
    
    /// <summary>
    /// Update the Orange button text based on Pack visibility and current tab - LEGACY SUPPORT
    /// </summary>
    public void UpdateButtonText()
    {
        if (buttonText == null) return;
        
        bool isPackVisible = packPanel != null && packPanel.activeInHierarchy;
        
        if (isPackVisible)
        {
            // Pack is visible - check current tab
            ItemLoader.ItemType currentTab = GetCurrentTab();
            
            switch (currentTab)
            {
                case ItemLoader.ItemType.Equipment:
                    buttonText.text = "Auto Equip";
                    Debug.Log("[SwitchPanels] LEGACY: Pack visible + Equipment tab → Auto Equip");
                    break;
                    
                case ItemLoader.ItemType.Gem:
                    buttonText.text = "Auto Embed";
                    Debug.Log("[SwitchPanels] LEGACY: Pack visible + Gem tab → Auto Embed");
                    break;
                    
                default:
                    buttonText.text = "Go to Pack";
                    break;
            }
        }
        else
        {
            // Board is visible - default text
            buttonText.text = "Go to Pack";
            Debug.Log("[SwitchPanels] LEGACY: Board visible → Go to Pack");
        }
    }
    
    /// <summary>
    /// Get the current active tab from LoadButtonController
    /// </summary>
    private ItemLoader.ItemType GetCurrentTab()
    {
        // CRITICAL FIX: Check ItemLoader directly instead of LoadButtonController
        // This ensures we get the ACTUAL current tab, not what LoadButtonController thinks
        ItemLoader itemLoader = FindObjectOfType<ItemLoader>();
        if (itemLoader != null)
        {
            Debug.Log($"[SwitchPanels] GetCurrentTab from ItemLoader: {itemLoader.currentItemType}");
            return itemLoader.currentItemType;
        }
        
        LoadButtonController loadButtonController = FindObjectOfType<LoadButtonController>();
        if (loadButtonController == null)
        {
            return ItemLoader.ItemType.Equipment; // Default to Equipment
        }
        
        // Use reflection to get the current active button index
        try
        {
            var activeButtonIndexField = typeof(LoadButtonController).GetField("activeButtonIndex", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            if (activeButtonIndexField != null)
            {
                int activeIndex = (int)activeButtonIndexField.GetValue(loadButtonController);
                Debug.Log($"[SwitchPanels] GetCurrentTab from LoadButtonController: {(ItemLoader.ItemType)activeIndex}");
                return (ItemLoader.ItemType)activeIndex;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SwitchPanels] Error getting current tab: {ex.Message}");
        }
        
        return ItemLoader.ItemType.Equipment; // Default fallback
    }
    
    /// <summary>
    /// Force switch to a specific tab - used to prevent unwanted tab switches
    /// </summary>
    private void ForceTabSwitch(ItemLoader.ItemType targetTab)
    {
        Debug.Log($"[SwitchPanels] 🔧 ForceTabSwitch to {targetTab}");
        
        LoadButtonController loadButtonController = FindObjectOfType<LoadButtonController>();
        if (loadButtonController == null)
        {
            Debug.LogError("[SwitchPanels] ❌ LoadButtonController not found for ForceTabSwitch");
            return;
        }
        
        // Force the tab switch by calling ToggleButtonVisibility directly
        int targetIndex = (int)targetTab;
        loadButtonController.ToggleButtonVisibility(targetIndex);
        
        Debug.Log($"[SwitchPanels] ✅ Forced tab switch to {targetTab} (index {targetIndex})");
    }
    
    /// <summary>
    /// Get the current sidekick ID from AlliesGridSetup (copied from ItemLoader)
    /// </summary>
    private int GetCurrentSidekickId()
    {
        AlliesGridSetup alliesGridSetup = FindObjectOfType<AlliesGridSetup>();
        if (alliesGridSetup == null)
        {
            Debug.LogWarning("[SwitchPanels] AlliesGridSetup not found - cannot determine current sidekick ID");
            return 0;
        }

        // Get current ally name using reflection
        string currentAllyName = GetCurrentAllyNameFromGridSetup(alliesGridSetup);
        if (string.IsNullOrEmpty(currentAllyName))
        {
            Debug.LogWarning("[SwitchPanels] Current ally name is empty - using default sidekick ID 0");
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
                Debug.Log($"[SwitchPanels] Found player sidekick ID {sidekickId} (base_id: {sidekick.base_id}) for ally {currentAllyName}");
                return sidekickId;
            }
        }

        return 0;
    }
    
    /// <summary>
    /// Get the current ally name from AlliesGridSetup using reflection (copied from ItemLoader)
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
                Debug.Log($"[SwitchPanels] Retrieved current ally name: {currentAllyName}");
                return currentAllyName ?? "";
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SwitchPanels] Error getting current ally name: {ex.Message}");
        }

        return "";
    }
    
    /// <summary>
    /// Convert ally name to base_id format used in sidekick data (copied from ItemLoader)
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

        Debug.LogWarning($"[SwitchPanels] Unknown ally name: {allyName}");
        return "0";
    }
    
    /// <summary>
    /// Refresh the equipment UI after auto equip operations complete (copied from ItemLoader)
    /// </summary>
    private System.Collections.IEnumerator RefreshEquipmentUIAfterAutoEquip()
    {
        yield return null;
        
        Debug.Log("[SwitchPanels] Starting equipment UI refresh after auto equip");
        
        var alliesEquipments = FindObjectOfType<AlliesEquipments>();
        if (alliesEquipments != null)
        {
            Debug.Log("[SwitchPanels] Calling InitForCurrentAlly to refresh equipment display");
            alliesEquipments.InitForCurrentAlly();
        }
        
        var alliesBlockSetup = FindObjectOfType<AlliesBlockSetup>();
        if (alliesBlockSetup != null)
        {
            alliesBlockSetup.UpdateTotalBlocks();
        }
    }
    
    /// <summary>
    /// Refresh Allies UI after auto embed operation (copied from ItemLoader)
    /// </summary>
    private System.Collections.IEnumerator RefreshAlliesUIAfterAutoEmbed(AutoEmbedResult result)
    {
        yield return null;
        
        Debug.Log("[SwitchPanels] Starting Allies UI refresh after auto embed");
        
        var alliesEquipments = FindObjectOfType<AlliesEquipments>();
        if (alliesEquipments != null)
        {
            Debug.Log("[SwitchPanels] Calling InitForCurrentAlly to refresh equipment display with embedded gems");
            alliesEquipments.InitForCurrentAlly();
        }
        
        var alliesBlockSetup = FindObjectOfType<AlliesBlockSetup>();
        if (alliesBlockSetup != null)
        {
            alliesBlockSetup.UpdateTotalBlocks();
        }
        
        ShowAutoEmbedResult(result, "Allies");
    }
    
    /// <summary>
    /// Show auto embed result feedback to user (copied from ItemLoader)
    /// </summary>
    private void ShowAutoEmbedResult(AutoEmbedResult result, string context)
    {
        if (result.TotalEmbedded > 0)
        {
            Debug.Log($"[SwitchPanels] ✅ {context} Auto Embed Success: {result.TotalEmbedded} gems embedded");
        }
        
        if (result.FailedEmbeds > 0)
        {
            Debug.LogWarning($"[SwitchPanels] ⚠️ {context} Auto Embed Partial: {result.FailedEmbeds} gems failed to embed");
        }
        
        if (result.TotalAttempted == 0)
        {
            Debug.Log($"[SwitchPanels] ℹ️ {context} Auto Embed: No gems to embed (all equipment slots full or no suitable gems)");
        }
    }
}