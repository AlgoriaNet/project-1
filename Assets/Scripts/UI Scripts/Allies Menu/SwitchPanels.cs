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
    [Header("Separate Buttons Approach")]
    public Button goToPackButton;     // "Go to Pack" button
    public Button autoEquipButton;    // "Auto Equip" button  
    public Button autoEmbedButton;    // "Auto Embed" button
    
    [Header("Panel References")]
    public GameObject packPanel;  // Assign the Pack panel in Inspector
    public GameObject boardPanel; // Assign the Board panel in Inspector

    private void Start()
    {
        if (goToPackButton != null)
            goToPackButton.onClick.AddListener(SwitchToPack);
        
        if (autoEquipButton != null)
            autoEquipButton.onClick.AddListener(TriggerAutoEquip);
        
        if (autoEmbedButton != null)
            autoEmbedButton.onClick.AddListener(TriggerAutoEmbed);
        
        UpdateButtonVisibility();
    }

    
    private void SwitchToPack()
    {
        if (packPanel != null) packPanel.SetActive(true);
        if (boardPanel != null) boardPanel.SetActive(false);
        UpdateButtonVisibility();
    }
    
    private void TriggerAutoEquip()
    {
        int currentSidekickId = GetCurrentSidekickId();
        if (currentSidekickId == 0) return;
        
        AutoEquipUtility.AutoEquipAll(AutoEquipUtility.EquipContext.Ally, currentSidekickId, () => {
            StartCoroutine(RefreshEquipmentUIAfterAutoEquip());
        });
    }
    
    private void TriggerAutoEmbed()
    {
        ItemLoader itemLoader = FindObjectOfType<ItemLoader>();
        if (itemLoader == null) return;
        
        var autoEmbedMethod = typeof(ItemLoader).GetMethod("OpenAutoEmbedPage", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (autoEmbedMethod != null)
            autoEmbedMethod.Invoke(itemLoader, null);
    }
    
    public void UpdateButtonVisibility()
    {
        bool isPackVisible = packPanel != null && packPanel.activeInHierarchy;
        
        if (isPackVisible)
        {
            ItemLoader.ItemType currentTab = GetCurrentTab();
            
            switch (currentTab)
            {
                case ItemLoader.ItemType.Equipment:
                    if (goToPackButton != null) goToPackButton.gameObject.SetActive(false);
                    if (autoEquipButton != null) autoEquipButton.gameObject.SetActive(true);
                    if (autoEmbedButton != null) autoEmbedButton.gameObject.SetActive(false);
                    break;
                    
                case ItemLoader.ItemType.Gem:
                    if (goToPackButton != null) goToPackButton.gameObject.SetActive(false);
                    if (autoEquipButton != null) autoEquipButton.gameObject.SetActive(false);
                    if (autoEmbedButton != null) autoEmbedButton.gameObject.SetActive(true);
                    break;
                    
                default:
                    if (goToPackButton != null) goToPackButton.gameObject.SetActive(true);
                    if (autoEquipButton != null) autoEquipButton.gameObject.SetActive(false);
                    if (autoEmbedButton != null) autoEmbedButton.gameObject.SetActive(false);
                    break;
            }
        }
        else
        {
            if (goToPackButton != null) goToPackButton.gameObject.SetActive(true);
            if (autoEquipButton != null) autoEquipButton.gameObject.SetActive(false);
            if (autoEmbedButton != null) autoEmbedButton.gameObject.SetActive(false);
        }
    }
    
    
    private ItemLoader.ItemType GetCurrentTab()
    {
        ItemLoader itemLoader = FindObjectOfType<ItemLoader>();
        if (itemLoader != null)
            return itemLoader.currentItemType;
        
        LoadButtonController loadButtonController = FindObjectOfType<LoadButtonController>();
        if (loadButtonController == null)
            return ItemLoader.ItemType.Equipment;
        
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
        catch
        {
        }
        
        return ItemLoader.ItemType.Equipment;
    }
    
    
    private int GetCurrentSidekickId()
    {
        AlliesGridSetup alliesGridSetup = FindObjectOfType<AlliesGridSetup>();
        if (alliesGridSetup == null) return 0;

        string currentAllyName = GetCurrentAllyNameFromGridSetup(alliesGridSetup);
        if (string.IsNullOrEmpty(currentAllyName)) return 0;

        if (PlayerProfile.Data?.Sidekick != null)
        {
            var sidekick = PlayerProfile.Data.Sidekick.FirstOrDefault(s =>
            {
                string allyBaseId = GetAllyBaseIdFromName(currentAllyName);
                return s.base_id == allyBaseId;
            });

            if (sidekick != null && int.TryParse(sidekick.id, out int sidekickId))
                return sidekickId;
        }

        return 0;
    }
    
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
                return currentAllyName ?? "";
            }
        }
        catch
        {
        }

        return "";
    }
    
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
                return (i + 1).ToString();
        }

        return "0";
    }
    
    private System.Collections.IEnumerator RefreshEquipmentUIAfterAutoEquip()
    {
        yield return null;
        
        var alliesEquipments = FindObjectOfType<AlliesEquipments>();
        if (alliesEquipments != null)
        {
            alliesEquipments.InitForCurrentAlly();
        }
        
        var alliesBlockSetup = FindObjectOfType<AlliesBlockSetup>();
        if (alliesBlockSetup != null)
        {
            alliesBlockSetup.UpdateTotalBlocks();
        }
    }
    
    private System.Collections.IEnumerator RefreshAlliesUIAfterAutoEmbed(AutoEmbedResult result)
    {
        yield return null;
        
        var alliesEquipments = FindObjectOfType<AlliesEquipments>();
        if (alliesEquipments != null)
        {
            alliesEquipments.InitForCurrentAlly();
        }
        
        var alliesBlockSetup = FindObjectOfType<AlliesBlockSetup>();
        if (alliesBlockSetup != null)
        {
            alliesBlockSetup.UpdateTotalBlocks();
        }
        
        ShowAutoEmbedResult(result, "Allies");
    }
    
    private void ShowAutoEmbedResult(AutoEmbedResult result, string context)
    {
        // Auto embed result feedback - can be extended with UI notifications
    }
}