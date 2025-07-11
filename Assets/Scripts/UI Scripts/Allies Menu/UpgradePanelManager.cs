using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.Networking;
using model;
using WebSocket;
using Newtonsoft.Json.Linq;

[System.Serializable]
public class UpgradeLevel
{
    public string level;
    public string description;
    public int cost;
    public bool is_unlocked;
}

[System.Serializable]
public class AllyUpgradeResponse
{
    public string ally_id;
    public string name;
    public string cn_name;
    public int current_level;
    public List<UpgradeLevel> upgrade_levels;
}

[System.Serializable]
public class LevelUpCostData
{
    public string ally_id;
    public int current_level;
    public int next_level;
    public int max_level;
    public bool can_level_up;
    public LevelUpCost cost;
    public PlayerResources player_resources;
    public bool has_enough_resources;
}

[System.Serializable]
public class LevelUpCost
{
    public int skillbook_cost;
    public int gold_cost;
}

[System.Serializable]
public class PlayerResources
{
    public int gold;
    public int skillbooks;
}

public class UpgradePanelManager : MonoBehaviour
{
    [Header("Prefab and Container")]
    public GameObject upgradeLevelPanelPrefab;
    public Transform contentContainer; // The Content transform under ScrollView
    
    [Header("Right Panel UI")]
    public TextMeshProUGUI upgradeText; // The upgrade text on right panel (L01 >>> L02)
    public TextMeshProUGUI itemText; // Item cost/quantity display (40/50) - skillbook for LevelUp, shard for StarUp
    public TextMeshProUGUI goldText; // Gold cost/quantity display (5000/15000)  
    public Button levelUpButton; // Level up button
    
    [Header("Current Ally Info")]
    private string currentAllyId;
    private AllyUpgradeResponse currentUpgradeData;
    private LevelUpCostData currentLevelUpCost;
    
    // List to keep track of created panels
    private List<GameObject> createdPanels = new List<GameObject>();
    
    void Start()
    {
        // Setup level up button
        if (levelUpButton != null)
        {
            levelUpButton.onClick.AddListener(OnLevelUpButtonClicked);
        }
        
        // Subscribe to WebSocket - using PlayerChannel
        PlayerWebSocketApi.Instance.Subscribe();
    }
    
    /// <summary>
    /// Load upgrade panels for a specific ally
    /// </summary>
    /// <param name="allyId">The ally ID (e.g., "02_Gideon")</param>
    public void LoadUpgradePanelsForAlly(string allyId)
    {
        currentAllyId = allyId;
        ClearExistingPanels();
        // Use WebSocket API for upgrade levels (correct payload: ally_id)
        var data = new { ally_id = allyId };
        PlayerWebSocketApi.Instance.Action(
            "get_upgrade_levels",
            data,
            OnUpgradeLevelsReceived,
            OnUpgradeLevelsError
        );
    }

    // WebSocket response handler for upgrade levels
    private void OnUpgradeLevelsReceived(Newtonsoft.Json.Linq.JObject response)
    {
        try
        {
            Debug.Log($"[UpgradePanelManager] Received upgrade levels response: {response}");
            // Parse the response as AllyUpgradeResponse (root object)
            currentUpgradeData = response.ToObject<AllyUpgradeResponse>();
            if (currentUpgradeData != null && currentUpgradeData.upgrade_levels != null)
            {
                CreateUpgradePanels();
            }
            else
            {
                Debug.LogError("[UpgradePanelManager] Failed to parse upgrade levels data from WebSocket response");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[UpgradePanelManager] Error parsing upgrade levels response: {e.Message}");
        }
    }

    // WebSocket error handler for upgrade levels
    private void OnUpgradeLevelsError(Newtonsoft.Json.Linq.JObject error)
    {
        Debug.LogError($"[UpgradePanelManager] Failed to fetch upgrade levels via WebSocket: {error}");
    }
    
    /// <summary>
    /// Clear all existing upgrade panels
    /// </summary>
    private void ClearExistingPanels()
    {
        Debug.Log($"[UpgradePanelManager] Clearing {createdPanels.Count} existing panels");
        foreach (GameObject panel in createdPanels)
        {
            if (panel != null)
            {
                DestroyImmediate(panel);
            }
        }
        createdPanels.Clear();
        Debug.Log($"[UpgradePanelManager] Panels cleared, list now has {createdPanels.Count} items");
    }
    
    /// <summary>
    /// Create upgrade panels based on current upgrade data
    /// </summary>
    private void CreateUpgradePanels()
    {
        if (currentUpgradeData?.upgrade_levels == null) 
        {
            Debug.LogWarning("[UpgradePanelManager] No upgrade_levels data to create panels");
            return;
        }
        
        Debug.Log($"[UpgradePanelManager] Creating {currentUpgradeData.upgrade_levels.Count} panels for {currentUpgradeData.name}");
        
        foreach (UpgradeLevel upgradeLevel in currentUpgradeData.upgrade_levels)
        {
            Debug.Log($"[UpgradePanelManager] Creating panel for level: {upgradeLevel.level} - {upgradeLevel.description}");
            
            // Instantiate the prefab
            GameObject newPanel = Instantiate(upgradeLevelPanelPrefab, contentContainer);
            createdPanels.Add(newPanel);
            
            // Setup the panel content
            SetupPanelContent(newPanel, upgradeLevel);
        }
        
        Debug.Log($"[UpgradePanelManager] Finished creating {createdPanels.Count} upgrade panels for {currentUpgradeData.name}");
        
        // Update the right panel upgrade text (only if upgradeText is assigned)
        if (upgradeText != null)
        {
            UpdateRightPanelUpgradeText();
        }
        
        // Fetch level up cost data
        FetchLevelUpCost();
    }
    
    /// <summary>
    /// Setup the content of a single upgrade panel
    /// </summary>
    /// <param name="panel">The panel GameObject</param>
    /// <param name="upgradeLevel">The upgrade level data</param>
    private void SetupPanelContent(GameObject panel, UpgradeLevel upgradeLevel)
    {
        // Find the components in the panel
        Transform unlockedPanel = panel.transform.Find("UnlockedPanel");
        Transform lockedPanel = panel.transform.Find("LockedPanel");
        TextMeshProUGUI levelText = panel.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descriptionText = panel.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
        
        // Set the level text
        if (levelText != null)
        {
            levelText.text = upgradeLevel.level;
        }
        
        // Set the description text
        if (descriptionText != null)
        {
            descriptionText.text = upgradeLevel.description;
        }
        
        // Determine if this upgrade level should be locked or unlocked
        // Compare current sidekick level vs this upgrade level
        int currentLevel = GetSidekickCurrentLevel(currentUpgradeData.ally_id);
        int upgradeLevelNumber = ExtractLevelNumber(upgradeLevel.level);
        bool isUnlocked = upgradeLevelNumber <= currentLevel;
        
        // Show/hide locked/unlocked panels
        if (unlockedPanel != null)
        {
            unlockedPanel.gameObject.SetActive(isUnlocked);
        }
        
        if (lockedPanel != null)
        {
            lockedPanel.gameObject.SetActive(!isUnlocked);
        }
        
        // Optional: Change text color based on unlock status
        if (levelText != null)
        {
            levelText.color = isUnlocked ? Color.white : Color.gray;
        }
        
        if (descriptionText != null)
        {
            descriptionText.color = isUnlocked ? Color.white : Color.gray;
        }
    }
    
    /// <summary>
    /// Update the upgrade text on the right panel
    /// </summary>
    private void UpdateRightPanelUpgradeText()
    {
        if (upgradeText == null || currentUpgradeData == null)
        {
            return;
        }
        
        // Get the current level and find the next upgrade level
        int currentLevel = GetSidekickCurrentLevel(currentUpgradeData.ally_id);
        
        // For now, just show current level >>> L02 (we can make this smarter later)
        string currentLevelText = $"L{currentLevel:D2}";
        upgradeText.text = $"{currentLevelText} >>> L02";
    }
    
    /// <summary>
    /// Extract level number from level string (e.g., "L06" -> 6)
    /// </summary>
    /// <param name="levelString">Level string like "L02", "L06"</param>
    /// <returns>Level number</returns>
    private int ExtractLevelNumber(string levelString)
    {
        if (string.IsNullOrEmpty(levelString) || levelString.Length < 2)
        {
            return 1; // Default to 1 if invalid
        }
        
        // Remove "L" prefix and parse number
        string numberPart = levelString.Substring(1);
        if (int.TryParse(numberPart, out int levelNumber))
        {
            return levelNumber;
        }
        
        return 1; // Default to 1 if parsing fails
    }
    
    /// <summary>
    /// Get the current skill level for a specific sidekick
    /// </summary>
    /// <param name="allyId">The ally ID (e.g., "02_Gideon")</param>
    /// <returns>Current skill level from PlayerProfile data</returns>
    private int GetSidekickCurrentLevel(string allyId)
    {
        if (PlayerProfile.Data?.Sidekick == null)
        {
            return 1; // Default to level 1 if no data
        }
        // Find the sidekick by matching the ally ID format
        var sidekick = PlayerProfile.Data.Sidekick.FirstOrDefault(s => 
            s.base_id == allyId || 
            s.id == allyId ||
            (s.base_id != null && allyId.Contains("_") && s.base_id.EndsWith(allyId.Split('_')[1]))
        );
        if (sidekick != null)
        {
            return sidekick.skill_level;
        }
        // If sidekick not found, return 1 as default
        return 1;
    }
    
    ///
    
    /// <summary>
    /// Fetch level up cost data via WebSocket
    /// </summary>
    private void FetchLevelUpCost()
    {
        if (string.IsNullOrEmpty(currentAllyId))
        {
            Debug.LogWarning("[UpgradePanelManager] No currentAllyId set for FetchLevelUpCost");
            return;
        }
        
        Debug.Log($"[UpgradePanelManager] Fetching level up cost for ally: {currentAllyId}");
        
        // Debug: Check what sidekick data we actually have
        if (PlayerProfile.Data?.Sidekick != null)
        {
            Debug.Log($"[UpgradePanelManager] Available sidekicks in PlayerProfile:");
            foreach (var sidekick in PlayerProfile.Data.Sidekick)
            {
                Debug.Log($"  - ID: {sidekick.id}, Base_ID: {sidekick.base_id}, Level: {sidekick.skill_level}");
            }
        }
        
        var data = new { ally_name = currentAllyId };
        
        PlayerWebSocketApi.Instance.Action("get_level_up_cost", data, 
            OnLevelUpCostReceived, OnLevelUpCostError);
    }
    
    /// <summary>
    /// Handle level up cost response
    /// </summary>
    /// <param name="response">WebSocket response</param>
    private void OnLevelUpCostReceived(JObject response)
    {
        try
        {
            Debug.Log($"[UpgradePanelManager] Received level up cost response: {response}");
            
            var dataToken = response["data"];
            if (dataToken != null)
            {
                currentLevelUpCost = dataToken.ToObject<LevelUpCostData>();
                Debug.Log($"[UpgradePanelManager] Parsed level up cost data: skillbook={currentLevelUpCost.cost.skillbook_cost}/{currentLevelUpCost.player_resources.skillbooks}, gold={currentLevelUpCost.cost.gold_cost}/{currentLevelUpCost.player_resources.gold}");
                UpdateResourceDisplays();
            }
            else
            {
                Debug.LogWarning("[UpgradePanelManager] No data field in level up cost response");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse level up cost response: {e.Message}");
        }
    }
    
    /// <summary>
    /// Handle level up cost error
    /// </summary>
    /// <param name="error">Error response</param>
    private void OnLevelUpCostError(JObject error)
    {
        Debug.LogError($"Failed to fetch level up cost: {error}");
    }
    
    /// <summary>
    /// Update resource displays on right panel
    /// </summary>
    private void UpdateResourceDisplays()
    {
        Debug.Log("[UpgradePanelManager] UpdateResourceDisplays called");
        
        if (currentLevelUpCost == null)
        {
            Debug.LogWarning("[UpgradePanelManager] currentLevelUpCost is null - no WebSocket data received");
            
            // Show debug info in UI to see what's happening
            if (itemText != null) itemText.text = "NO DATA";
            if (goldText != null) goldText.text = "NO DATA";
            return;
        }
        
        Debug.Log($"[UpgradePanelManager] WebSocket data received:");
        Debug.Log($"  - Ally: {currentLevelUpCost.ally_id}");
        Debug.Log($"  - Current Level: {currentLevelUpCost.current_level}");
        Debug.Log($"  - Next Level: {currentLevelUpCost.next_level}");
        Debug.Log($"  - Next Level: {currentLevelUpCost.next_level}");
        Debug.Log($"  - Skillbook Cost: {currentLevelUpCost.cost.skillbook_cost}");
        Debug.Log($"  - Gold Cost: {currentLevelUpCost.cost.gold_cost}");
        Debug.Log($"  - Player Skillbooks: {currentLevelUpCost.player_resources.skillbooks}");
        Debug.Log($"  - Player Gold: {currentLevelUpCost.player_resources.gold}");
        Debug.Log($"  - Can Level Up: {currentLevelUpCost.can_level_up}");
        Debug.Log($"  - Has Enough Resources: {currentLevelUpCost.has_enough_resources}");
        
        // TODO: Need to detect if we're in LevelUp mode (skillbook) or StarUp mode (shard)
        // For now, assuming LevelUp mode
        string itemType = "skillbook"; // This should be dynamic based on current mode
        Debug.Log($"  - Item Type: {itemType}");
        
        // Update item display (need/have)
        if (itemText != null)
        {
            string itemDisplay = $"{currentLevelUpCost.cost.skillbook_cost}/{currentLevelUpCost.player_resources.skillbooks}";
            itemText.text = itemDisplay;
            Debug.Log($"[UpgradePanelManager] Set itemText to: {itemDisplay}");
        }
        else
        {
            Debug.LogWarning("[UpgradePanelManager] itemText is null - not assigned in Inspector?");
        }
        
        // Update gold display (need/have)
        if (goldText != null)
        {
            string goldDisplay = $"{currentLevelUpCost.cost.gold_cost}/{currentLevelUpCost.player_resources.gold}";
            goldText.text = goldDisplay;
            Debug.Log($"[UpgradePanelManager] Set goldText to: {goldDisplay}");
        }
        else
        {
            Debug.LogWarning("[UpgradePanelManager] goldText is null - not assigned in Inspector?");
        }
        
        // Update level up button state
        if (levelUpButton != null)
        {
            levelUpButton.interactable = currentLevelUpCost.can_level_up && currentLevelUpCost.has_enough_resources;
            Debug.Log($"[UpgradePanelManager] Set levelUpButton.interactable to: {levelUpButton.interactable}");
        }
        else
        {
            Debug.LogWarning("[UpgradePanelManager] levelUpButton is null - not assigned in Inspector?");
        }
    }
    
    /// <summary>
    /// Handle level up button click
    /// </summary>
    private void OnLevelUpButtonClicked()
    {
        if (currentLevelUpCost == null || !currentLevelUpCost.can_level_up || !currentLevelUpCost.has_enough_resources)
        {
            Debug.LogWarning("Cannot level up: insufficient resources or max level reached");
            return;
        }
        
        // Use new WebSocket action 'level_upgrade' and send both ally_name and current_level
        var data = new {
            ally_name = currentAllyId,
            current_level = currentLevelUpCost.current_level
        };
        
        PlayerWebSocketApi.Instance.Action("level_upgrade", data, 
            OnLevelUpSuccess, OnLevelUpError);
    }

    /// <summary>
    /// Handle successful level up response
    /// </summary>
    /// <param name="response">WebSocket response</param>
    private void OnLevelUpSuccess(JObject response)
    {
        try
        {
            Debug.Log($"Level up successful: {response}");
            // Parse the response for new level, gold, and skillbooks
            var dataToken = response["data"];
            if (dataToken != null)
            {
                // Update PlayerProfile and UI with new values if needed
                // Optionally, parse new level, gold, skillbooks from dataToken
                // Example:
                // int newLevel = dataToken["new_level"]?.Value<int>() ?? -1;
                // int gold = dataToken["gold"]?.Value<int>() ?? -1;
                // int skillbooks = dataToken["skillbooks"]?.Value<int>() ?? -1;
                // TODO: Update PlayerProfile.Data.Sidekick and PlayerResources if needed

                // Refresh level up cost data to update displays
                FetchLevelUpCost();
                // Update upgrade text on right panel
                if (upgradeText != null)
                {
                    UpdateRightPanelUpgradeText();
                }
                // Refresh the upgrade panels to update lock/unlock status
                CreateUpgradePanels();
            }
            else
            {
                Debug.LogWarning("[UpgradePanelManager] No data field in level up response");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to process level up response: {e.Message}");
        }
    }

    /// <summary>
    /// Handle level up error
    /// </summary>
    /// <param name="error">Error response</param>
    private void OnLevelUpError(JObject error)
    {
        Debug.LogError($"Level up failed: {error}");
        // Optionally, show error feedback to user here
    }
    
    /// <summary>
    /// Public method to be called from AlliesGridSetup when ally is selected
    /// </summary>
    /// <param name="allyIndex">Ally index like "02"</param>
    /// <param name="allyName">Ally name like "Gideon"</param>
    public void OnAllySelected(string allyIndex, string allyName)
    {
        string allyId = $"{allyIndex}_{allyName}";
        LoadUpgradePanelsForAlly(allyId);
    }
}