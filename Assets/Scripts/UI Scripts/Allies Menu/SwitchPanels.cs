using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SwitchPanels : MonoBehaviour
{
    public Button button;   // Assign the button in Inspector
    public GameObject packPanel;  // Assign the Pack panel in Inspector
    public GameObject boardPanel; // Assign the Board panel in Inspector
    public TextMeshProUGUI buttonText; // Assign the Orange button's text component

    private void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(HandleButtonClick);
        }
        
        // Set initial button text
        UpdateButtonText();
    }

    /// <summary>
    /// Handle Orange button click - either switch to pack or trigger auto equip/embed
    /// </summary>
    private void HandleButtonClick()
    {
        if (buttonText == null) return;
        
        string currentText = buttonText.text;
        
        if (currentText == "Go to Pack")
        {
            // Switch to Pack
            SwitchToPack();
        }
        else if (currentText == "Auto Equip")
        {
            // Trigger Auto Equip (same as red button)
            TriggerAutoEquip();
        }
        else if (currentText == "Auto Embed")
        {
            // Trigger Auto Embed (same as red button)
            TriggerAutoEmbed();
        }
    }
    
    private void SwitchToPack()
    {
        if (packPanel != null) packPanel.SetActive(true);
        if (boardPanel != null) boardPanel.SetActive(false);
        
        // Update button text after switching to pack
        UpdateButtonText();
    }
    
    /// <summary>
    /// Trigger Auto Equip functionality (same as red button)
    /// </summary>
    private void TriggerAutoEquip()
    {
        Debug.Log("[SwitchPanels] Orange Auto Equip button clicked");
        
        // Find ItemLoader and call its auto equip function
        ItemLoader itemLoader = FindObjectOfType<ItemLoader>();
        if (itemLoader != null)
        {
            // Use reflection to call the private OpenAutoEquipPage method
            var autoEquipMethod = typeof(ItemLoader).GetMethod("OpenAutoEquipPage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            if (autoEquipMethod != null)
            {
                autoEquipMethod.Invoke(itemLoader, null);
                Debug.Log("[SwitchPanels] Successfully triggered auto equip from Orange button");
            }
            else
            {
                Debug.LogError("[SwitchPanels] OpenAutoEquipPage method not found in ItemLoader");
            }
        }
        else
        {
            Debug.LogError("[SwitchPanels] ItemLoader not found - cannot trigger auto equip");
        }
    }
    
    /// <summary>
    /// Trigger Auto Embed functionality (same as red button)
    /// </summary>
    private void TriggerAutoEmbed()
    {
        Debug.Log("[SwitchPanels] Orange Auto Embed button clicked");
        
        // Find ItemLoader and call its auto embed function
        ItemLoader itemLoader = FindObjectOfType<ItemLoader>();
        if (itemLoader != null)
        {
            // Use reflection to call the private OpenAutoEmbedPage method
            var autoEmbedMethod = typeof(ItemLoader).GetMethod("OpenAutoEmbedPage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            if (autoEmbedMethod != null)
            {
                autoEmbedMethod.Invoke(itemLoader, null);
                Debug.Log("[SwitchPanels] Successfully triggered auto embed from Orange button");
            }
            else
            {
                Debug.LogError("[SwitchPanels] OpenAutoEmbedPage method not found in ItemLoader");
            }
        }
        else
        {
            Debug.LogError("[SwitchPanels] ItemLoader not found - cannot trigger auto embed");
        }
    }
    
    /// <summary>
    /// Update the Orange button text based on Pack visibility and current tab
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
                    Debug.Log("[SwitchPanels] Pack visible + Equipment tab → Auto Equip");
                    break;
                    
                case ItemLoader.ItemType.Gem:
                    buttonText.text = "Auto Embed";
                    Debug.Log("[SwitchPanels] Pack visible + Gem tab → Auto Embed");
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
            Debug.Log("[SwitchPanels] Board visible → Go to Pack");
        }
    }
    
    /// <summary>
    /// Get the current active tab from LoadButtonController
    /// </summary>
    private ItemLoader.ItemType GetCurrentTab()
    {
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
                return (ItemLoader.ItemType)activeIndex;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SwitchPanels] Error getting current tab: {ex.Message}");
        }
        
        return ItemLoader.ItemType.Equipment; // Default fallback
    }
}