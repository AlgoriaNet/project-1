using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Networking;

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

public class UpgradePanelManager : MonoBehaviour
{
    [Header("Prefab and Container")]
    public GameObject upgradeLevelPanelPrefab;
    public Transform contentContainer; // The Content transform under ScrollView
    
    [Header("Current Ally Info")]
    private string currentAllyId;
    private AllyUpgradeResponse currentUpgradeData;
    
    // List to keep track of created panels
    private List<GameObject> createdPanels = new List<GameObject>();
    
    void Start()
    {
        // Remove auto-loading - will be called from AlliesGridSetup
    }
    
    /// <summary>
    /// Load upgrade panels for a specific ally
    /// </summary>
    /// <param name="allyId">The ally ID (e.g., "02_Gideon")</param>
    public void LoadUpgradePanelsForAlly(string allyId)
    {
        Debug.Log($"[UpgradePanelManager] LoadUpgradePanelsForAlly called for: {allyId}");
        
        // Prevent duplicate loading for the same ally
        if (currentAllyId == allyId && createdPanels.Count > 0)
        {
            Debug.Log($"[UpgradePanelManager] Already loaded panels for {allyId}, skipping");
            return;
        }
        
        currentAllyId = allyId;
        
        // Clear existing panels
        ClearExistingPanels();
        Debug.Log($"[UpgradePanelManager] Cleared {createdPanels.Count} existing panels");
        
        // Start API call coroutine
        StartCoroutine(FetchUpgradeLevels(allyId));
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
        
        // Show/hide locked/unlocked panels
        if (unlockedPanel != null)
        {
            unlockedPanel.gameObject.SetActive(upgradeLevel.is_unlocked);
        }
        
        if (lockedPanel != null)
        {
            lockedPanel.gameObject.SetActive(!upgradeLevel.is_unlocked);
        }
        
        // Optional: Change text color based on unlock status
        if (levelText != null)
        {
            levelText.color = upgradeLevel.is_unlocked ? Color.white : Color.gray;
        }
        
        if (descriptionText != null)
        {
            descriptionText.color = upgradeLevel.is_unlocked ? Color.white : Color.gray;
        }
    }
    
    /// <summary>
    /// Fetch upgrade levels from API
    /// </summary>
    /// <param name="allyId">The ally ID (e.g., "02_Gideon")</param>
    private IEnumerator FetchUpgradeLevels(string allyId)
    {
        string apiUrl = $"{Config.BaseUrl}/api/allies/{allyId}/upgrade_levels";
        Debug.Log($"Fetching upgrade levels from: {apiUrl}");
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    Debug.Log($"API Response: {jsonResponse}");
                    
                    currentUpgradeData = JsonUtility.FromJson<AllyUpgradeResponse>(jsonResponse);
                    
                    if (currentUpgradeData != null)
                    {
                        CreateUpgradePanels();
                    }
                    else
                    {
                        Debug.LogError($"Failed to parse upgrade data for ally: {allyId}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error parsing upgrade data for {allyId}: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"Failed to fetch upgrade levels for {allyId}: {webRequest.error}");
                Debug.LogError($"Response Code: {webRequest.responseCode}");
            }
        }
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