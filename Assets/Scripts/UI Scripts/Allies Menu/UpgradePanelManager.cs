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

[System.Serializable]
public class StarUpCostData
{
    public string ally_id;
    public int current_star;
    public int next_star;
    public int max_star;
    public bool can_star_up;
    public StarUpCost cost;
    public StarUpPlayerResources player_resources;
    public bool has_enough_resources;
}

[System.Serializable]
public class StarUpCost
{
    public int shard_cost;
    public int gold_cost;
}

[System.Serializable]
public class StarUpPlayerResources
{
    public int gold;
    public int shards;
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
    public TextMeshProUGUI allyNameText; // Ally name display (e.g., "Aurelia" from "04_Aurelia")
    public Button levelUpButton; // Level up button
    public Transform step2StarGroup; // Drag the StarGroup from UpperGroup here
    
    [Header("Current Ally Info")]
    private string currentAllyId;
    private AllyUpgradeResponse currentUpgradeData;
    private LevelUpCostData currentLevelUpCost;
    private StarUpCostData currentStarUpCost;
    private bool isStarUpMode = false; // Track current mode
    
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
        
        // Listen for player data changes to refresh cost data
        PlayerProfile.Data.AddListener(OnPlayerDataChanged, "Player");
        
        // Also listen for item changes (for shard/skillbook updates)
        PlayerProfile.Data.AddListener(OnPlayerDataChanged, "Items");
        PlayerProfile.Data.AddListener(OnPlayerDataChanged, "OtherItems");
    }
    
    void OnEnable()
    {
        // Refresh when the component becomes active
        if (!string.IsNullOrEmpty(currentAllyId))
        {
            Invoke(nameof(FetchCurrentModeCost), 0.1f); // Small delay to ensure everything is initialized
        }
    }
    
    void OnDestroy()
    {
        // Clean up listeners
        PlayerProfile.Data.RemoveListener(OnPlayerDataChanged, "Player");
        PlayerProfile.Data.RemoveListener(OnPlayerDataChanged, "Items");
        PlayerProfile.Data.RemoveListener(OnPlayerDataChanged, "OtherItems");
    }
    
    /// <summary>
    /// Called when player data changes (e.g., after drawing shards)
    /// </summary>
    /// <param name="model">Updated player model</param>
    private void OnPlayerDataChanged(ApplicationModel model)
    {
        // Refresh cost data if we have a current ally selected
        if (!string.IsNullOrEmpty(currentAllyId))
        {
            // Add a small delay to ensure the backend has updated data
            Invoke(nameof(FetchCurrentModeCost), 0.2f);
        }
    }
    
    /// <summary>
    /// Public method to manually refresh cost data (can be called from other scripts)
    /// </summary>
    public void RefreshCostData()
    {
        if (!string.IsNullOrEmpty(currentAllyId))
        {
            FetchCurrentModeCost();
        }
    }
    
    /// <summary>
    /// Set the current mode (LevelUp or StarUp)
    /// </summary>
    /// <param name="starUpMode">True for StarUp, false for LevelUp</param>
    public void SetMode(bool starUpMode)
    {
        isStarUpMode = starUpMode;
        
        // Update upgrade text immediately for the new mode
        UpdateRightPanelUpgradeText();
        
        // Refresh cost data for the new mode
        if (!string.IsNullOrEmpty(currentAllyId))
        {
            FetchCurrentModeCost();
        }
    }
    
    /// <summary>
    /// Fetch cost data based on current mode
    /// </summary>
    private void FetchCurrentModeCost()
    {
        if (isStarUpMode)
        {
            FetchStarUpCost();
        }
        else
        {
            FetchLevelUpCost();
        }
    }
    
    /// <summary>
    /// Load upgrade panels for a specific ally
    /// </summary>
    /// <param name="allyId">The ally ID (e.g., "02_Gideon")</param>
    public void LoadUpgradePanelsForAlly(string allyId)
    {
        currentAllyId = allyId;
        
        // Update ally name text (extract name from "04_Aurelia" format)
        if (allyNameText != null && !string.IsNullOrEmpty(allyId) && allyId.Contains("_"))
        {
            string allyName = allyId.Split('_')[1]; // Extract "Aurelia" from "04_Aurelia"
            allyNameText.text = allyName;
        }
        
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
            // Parse the response as AllyUpgradeResponse (root object)
            currentUpgradeData = response.ToObject<AllyUpgradeResponse>();
            if (currentUpgradeData != null && currentUpgradeData.upgrade_levels != null)
            {
                CreateUpgradePanels();
            }
            else
            {
            }
        }
        catch (System.Exception e)
        {
        }
    }

    // WebSocket error handler for upgrade levels
    private void OnUpgradeLevelsError(Newtonsoft.Json.Linq.JObject error)
    {
    }
    
    /// <summary>
    /// Clear all existing upgrade panels
    /// </summary>
    private void ClearExistingPanels()
    {
        foreach (GameObject panel in createdPanels)
        {
            if (panel != null)
            {
                DestroyImmediate(panel);
            }
        }
        createdPanels.Clear();
    }
    
    /// <summary>
    /// Create upgrade panels based on current upgrade data
    /// </summary>
    private void CreateUpgradePanels()
    {
        if (currentUpgradeData?.upgrade_levels == null) 
        {
            return;
        }
        
        
        foreach (UpgradeLevel upgradeLevel in currentUpgradeData.upgrade_levels)
        {
            
            // Instantiate the prefab
            GameObject newPanel = Instantiate(upgradeLevelPanelPrefab, contentContainer);
            createdPanels.Add(newPanel);
            
            // Setup the panel content
            SetupPanelContent(newPanel, upgradeLevel);
        }
        
        
        // Update the right panel upgrade text (only if upgradeText is assigned)
        if (upgradeText != null)
        {
            UpdateRightPanelUpgradeText();
        }
        
        // Fetch cost data based on current mode
        FetchCurrentModeCost();
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
    /// Update the upgrade text on the right panel based on current mode
    /// </summary>
    private void UpdateRightPanelUpgradeText()
    {
        if (upgradeText == null)
        {
            return;
        }
        
        if (isStarUpMode)
        {
            // StarUp mode: Show star upgrade (0 ★ >>> 1 ★)
            UpdateStarUpgradeText();
        }
        else
        {
            // LevelUp mode: Show level upgrade (L01 >>> L02)
            UpdateLevelUpgradeText();
        }
    }
    
    /// <summary>
    /// Update text for LevelUp mode (L01 >>> L02)
    /// </summary>
    private void UpdateLevelUpgradeText()
    {
        
        if (currentLevelUpCost != null)
        {
            // Use backend data if available
            string newText = $"L{currentLevelUpCost.current_level:D2} >>> L{currentLevelUpCost.next_level:D2}";
            upgradeText.text = newText;
        }
        else
        {
            // Fallback to frontend calculation
            
            if (currentUpgradeData == null)
            {
                upgradeText.text = "L?? >>> L??";
                return;
            }
            
            int currentLevel = GetSidekickCurrentLevel(currentUpgradeData.ally_id);
            int nextLevel = currentLevel + 1;
            
            string currentLevelText = $"L{currentLevel:D2}";
            string nextLevelText = $"L{nextLevel:D2}";
            string newText = $"{currentLevelText} >>> {nextLevelText}";
            upgradeText.text = newText;
        }
        
    }
    
    /// <summary>
    /// Update text for StarUp mode (S0 >>> S1)
    /// </summary>
    private void UpdateStarUpgradeText()
    {
        if (currentStarUpCost != null)
        {
            // Use backend data if available
            string newText = $"S{currentStarUpCost.current_star} >>> S{currentStarUpCost.next_star}";
            upgradeText.text = newText;
            
            // Update yellow star to match the current star level
            UpdateStep2StarYellow(currentStarUpCost.current_star);
        }
        else
        {
            // Fallback to frontend calculation
            int currentStar = GetSidekickCurrentStarLevel(currentAllyId);
            int nextStar = currentStar + 1;
            string newText = $"S{currentStar} >>> S{nextStar}";
            upgradeText.text = newText;
            
            // Update yellow star to match the current star level
            UpdateStep2StarYellow(currentStar);
        }
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
    
    /// <summary>
    /// Get the current star level for a specific sidekick
    /// </summary>
    /// <param name="allyId">The ally ID (e.g., "02_Gideon")</param>
    /// <returns>Current star level from PlayerProfile data</returns>
    private int GetSidekickCurrentStarLevel(string allyId)
    {
        if (PlayerProfile.Data?.Sidekick == null)
        {
            return 0; // Default to 0 stars if no data
        }
        // Find the sidekick by matching the ally ID format
        var sidekick = PlayerProfile.Data.Sidekick.FirstOrDefault(s => 
            s.base_id == allyId || 
            s.id == allyId ||
            (s.base_id != null && allyId.Contains("_") && s.base_id.EndsWith(allyId.Split('_')[1]))
        );
        if (sidekick != null)
        {
            return sidekick.star;
        }
        // If sidekick not found, return 0 as default
        return 0;
    }
    
    ///
    
    /// <summary>
    /// Fetch level up cost data via WebSocket
    /// </summary>
    private void FetchLevelUpCost()
    {
        if (string.IsNullOrEmpty(currentAllyId))
        {
            return;
        }
        
        
        // Backend expects ally_id (not ally_name) with fragment_name format ("04_Aurelia")
        
        var data = new { ally_id = currentAllyId };
        
        if (PlayerWebSocketApi.Instance != null)
        {
            PlayerWebSocketApi.Instance.Action("get_level_up_cost", data, 
                OnLevelUpCostReceived, OnLevelUpCostError);
        }
        else
        {
        }
    }
    
    /// <summary>
    /// Fetch star up cost data via WebSocket
    /// </summary>
    private void FetchStarUpCost()
    {
        if (string.IsNullOrEmpty(currentAllyId))
        {
            return;
        }
        
        
        // Backend expects ally_id with fragment_name format ("04_Aurelia")
        var data = new { ally_id = currentAllyId };
        
        if (PlayerWebSocketApi.Instance != null)
        {
            PlayerWebSocketApi.Instance.Action("get_star_upgrade_cost", data, 
                OnStarUpCostReceived, OnStarUpCostError);
        }
        else
        {
        }
    }
    
    /// <summary>
    /// Handle level up cost response
    /// </summary>
    /// <param name="response">WebSocket response</param>
    private void OnLevelUpCostReceived(JObject response)
    {
        try
        {
            
            // Backend sends data directly in response, not nested in "data" field
            currentLevelUpCost = response.ToObject<LevelUpCostData>();
            
            // Update upgrade text with backend data (same as star upgrade)
            UpdateRightPanelUpgradeText();
            
            UpdateResourceDisplays();
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
    /// Handle star up cost response
    /// </summary>
    /// <param name="response">WebSocket response</param>
    private void OnStarUpCostReceived(JObject response)
    {
        try
        {
            
            // Backend sends data directly in response, not nested in "data" field
            currentStarUpCost = response.ToObject<StarUpCostData>();
            
            // Update upgrade text with backend data
            UpdateRightPanelUpgradeText();
            
            UpdateResourceDisplays();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse star up cost response: {e.Message}");
        }
    }
    
    /// <summary>
    /// Handle star up cost error
    /// </summary>
    /// <param name="error">Error response</param>
    private void OnStarUpCostError(JObject error)
    {
        Debug.LogError($"Failed to fetch star up cost: {error}");
    }
    
    
    /// <summary>
    /// Update resource displays on right panel
    /// </summary>
    private void UpdateResourceDisplays()
    {
        
        if (isStarUpMode)
        {
            if (currentStarUpCost == null)
            {
                if (itemText != null) itemText.text = "NO DATA";
                if (goldText != null) goldText.text = "NO DATA";
                return;
            }
            
            
            // Update item display (shard for star up)
            if (itemText != null)
            {
                string itemDisplay = $"{currentStarUpCost.cost.shard_cost}/{currentStarUpCost.player_resources.shards}";
                itemText.text = itemDisplay;
            }
            
            // Update gold display with K formatting
            if (goldText != null)
            {
                string goldDisplay = $"{NumberFormatter.FormatNumber(currentStarUpCost.cost.gold_cost)}/{NumberFormatter.FormatNumber(currentStarUpCost.player_resources.gold)}";
                goldText.text = goldDisplay;
            }
            
            // Update button state
            if (levelUpButton != null)
            {
                levelUpButton.interactable = currentStarUpCost.can_star_up && currentStarUpCost.has_enough_resources;
            }
        }
        else
        {
            if (currentLevelUpCost == null)
            {
                if (itemText != null) itemText.text = "NO DATA";
                if (goldText != null) goldText.text = "NO DATA";
                return;
            }
            
            
            // Update item display (skillbook for level up)
            if (itemText != null)
            {
                string itemDisplay = $"{currentLevelUpCost.cost.skillbook_cost}/{currentLevelUpCost.player_resources.skillbooks}";
                itemText.text = itemDisplay;
            }
            
            // Update gold display with K formatting
            if (goldText != null)
            {
                string goldDisplay = $"{NumberFormatter.FormatNumber(currentLevelUpCost.cost.gold_cost)}/{NumberFormatter.FormatNumber(currentLevelUpCost.player_resources.gold)}";
                goldText.text = goldDisplay;
            }
            
            // Update button state
            if (levelUpButton != null)
            {
                levelUpButton.interactable = currentLevelUpCost.can_level_up && currentLevelUpCost.has_enough_resources;
            }
        }
    }
    
    /// <summary>
    /// Handle level up button click
    /// </summary>
    private void OnLevelUpButtonClicked()
    {
        if (isStarUpMode)
        {
            HandleStarUpClick();
        }
        else
        {
            HandleLevelUpClick();
        }
    }
    
    /// <summary>
    /// Handle level up action
    /// </summary>
    private void HandleLevelUpClick()
    {
        if (currentLevelUpCost == null || !currentLevelUpCost.can_level_up || !currentLevelUpCost.has_enough_resources)
        {
            Debug.LogWarning("Cannot level up: insufficient resources or max level reached");
            return;
        }
        
        // Backend expects ally_name with fragment_name format ("04_Aurelia")
        var data = new {
            ally_name = currentAllyId,
            current_level = currentLevelUpCost.current_level
        };
        
        PlayerWebSocketApi.Instance.Action("level_upgrade", data, 
            OnLevelUpSuccess, OnLevelUpError);
    }
    
    /// <summary>
    /// Handle star up action
    /// </summary>
    private void HandleStarUpClick()
    {
        if (currentStarUpCost == null || !currentStarUpCost.can_star_up || !currentStarUpCost.has_enough_resources)
        {
            Debug.LogWarning("Cannot star up: insufficient resources or max star reached");
            return;
        }
        
        // Backend expects ally_name with fragment_name format ("04_Aurelia")
        var data = new {
            ally_name = currentAllyId,
            current_star = currentStarUpCost.current_star
        };
        
        PlayerWebSocketApi.Instance.Action("star_upgrade", data, 
            OnStarUpSuccess, OnStarUpError);
    }

    /// <summary>
    /// Handle successful level up response
    /// </summary>
    /// <param name="response">WebSocket response</param>
    private void OnLevelUpSuccess(JObject response)
    {
        try
        {
            
            // Log current state before any changes
            
            // Parse the response for new level, gold, and skillbooks
            var dataToken = response["data"];
            if (dataToken != null)
            {
                
                // Update upgrade text on right panel FIRST (same pattern as star upgrade)
                if (upgradeText != null)
                {
                    UpdateRightPanelUpgradeText();
                }
                else
                {
                }

                // Update PlayerProfile and UI with new values if needed
                // Optionally, parse new level, gold, skillbooks from dataToken
                // Example:
                // int newLevel = dataToken["new_level"]?.Value<int>() ?? -1;
                // int gold = dataToken["gold"]?.Value<int>() ?? -1;
                // int skillbooks = dataToken["skillbooks"]?.Value<int>() ?? -1;
                // TODO: Update PlayerProfile.Data.Sidekick and PlayerResources if needed
                
                // Refresh the upgrade panels to update lock/unlock status
                CreateUpgradePanels();
                
                // Update Step1 and Step2 displays for consistency (same as star upgrade)
                AlliesGridSetup alliesGridSetup = FindObjectOfType<AlliesGridSetup>();
                if (alliesGridSetup != null)
                {
                    alliesGridSetup.RefreshAllAllyStarDisplays();
                }
                else
                {
                }
                
                // Refresh cost data to update displays (LAST step like star upgrade)
                FetchCurrentModeCost();
                
                // Log final state
            }
            else
            {
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
    /// Update Step2 StarGroup yellow image based on S value
    /// </summary>
    /// <param name="sValue">The S value (1-5)</param>
    private void UpdateStep2StarYellow(int sValue)
    {
        if (step2StarGroup != null)
        {
            
            // Set complete star state - enable stars 1 through sValue, disable the rest
            for (int i = 1; i <= 5; i++)
            {
                Transform starYellow = step2StarGroup.Find($"Star{i}/yellow");
                if (starYellow != null)
                {
                    bool shouldActivate = i <= sValue;
                    starYellow.gameObject.SetActive(shouldActivate);
                }
                else
                {
                }
            }
        }
        else
        {
        }
    }
    
    /// <summary>
    /// Handle successful star up response
    /// </summary>
    /// <param name="response">WebSocket response</param>
    private void OnStarUpSuccess(JObject response)
    {
        try
        {
            Debug.Log($"Star up successful: {response}");
            var dataToken = response["data"];
            if (dataToken != null)
            {
                // Extract updated_sidekick data from response (Backend Option 1) - it's at root level, not in data
                var updatedSidekickToken = response["updated_sidekick"];
                if (updatedSidekickToken != null)
                {
                    // Parse updated sidekick data
                    var updatedSidekick = updatedSidekickToken.ToObject<model.Sidekick>();
                    if (updatedSidekick != null && PlayerProfile.Data?.Sidekick != null)
                    {
                        // Find and update the specific sidekick in PlayerProfile.Data.Sidekick
                        var existingSidekick = PlayerProfile.Data.Sidekick.FirstOrDefault(s => s.id == updatedSidekick.id);
                        if (existingSidekick != null)
                        {
                            existingSidekick.star = updatedSidekick.star;
                            existingSidekick.skill_level = updatedSidekick.skill_level;
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                    }
                }
                else
                {
                }
                
                // Update upgrade text on right panel
                if (upgradeText != null)
                {
                    UpdateRightPanelUpgradeText();
                    
                    // Extract S value and update Step2 star yellow immediately
                    string sText = upgradeText.text; // e.g., "S3"
                    if (sText.StartsWith("S") && int.TryParse(sText.Substring(1), out int sValue))
                    {
                        UpdateStep2StarYellow(sValue);
                    }
                }
                
                // Refresh the upgrade panels to update lock/unlock status
                CreateUpgradePanels();
                
                // Refresh star displays immediately - both Step1 and Step2
                AlliesGridSetup alliesGridSetup = FindObjectOfType<AlliesGridSetup>();
                if (alliesGridSetup != null)
                {
                    // Update Step2 star group immediately for instant visual feedback
                    alliesGridSetup.UpdateStep2StarDisplay(currentAllyId);
                    
                    // Force Step1 grid recreation with fresh data now that PlayerProfile.Data.Sidekick is updated
                    alliesGridSetup.ForceReloadWithFreshData();
                }
                else
                {
                }
                
                // Refresh cost data to update displays
                FetchCurrentModeCost();
            }
            else
            {
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to process star up response: {e.Message}");
        }
    }

    /// <summary>
    /// Handle star up error
    /// </summary>
    /// <param name="error">Error response</param>
    private void OnStarUpError(JObject error)
    {
        Debug.LogError($"Star up failed: {error}");
        // Optionally, show error feedback to user here
    }
    
    /// <summary>
    /// Delay Step1 grid recreation to ensure PlayerProfile.Data has fresh sidekick data
    /// </summary>
    private System.Collections.IEnumerator DelayedStep1GridRecreation(AlliesGridSetup alliesGridSetup, string allyId)
    {
        
        // Wait a short time for backend data to update PlayerProfile.Data.Sidekick
        yield return new WaitForSeconds(0.5f);
        
        alliesGridSetup.ForceReloadWithFreshData();
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