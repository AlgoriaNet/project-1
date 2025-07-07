using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;
using WebSocket;
using System;
using model;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Simple celebration popup for ally summoning - handles display and API call
/// </summary>
public class AllyStandPage : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject standPagePanel;
    [SerializeField] private Image allyIllustrationImage;
    [SerializeField] private TextMeshProUGUI allyNameText;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        // Setup close button
        closeButton?.onClick.AddListener(ClosePanel);
    }
    
    public void ClosePanel()
    {
        standPagePanel.SetActive(false);
    }

    public void ShowAllyStandPage(string allyDisplayName, string allyIndex, string allyName)
    {
        // Load ally illustration
        LoadAllyIllustration(allyIndex, allyDisplayName);
        
        // Set ally name
        allyNameText.text = allyDisplayName;
        
        // Show the panel immediately
        standPagePanel.SetActive(true);
        
        // Start the summon process
        StartSummon(allyName);
    }
    
    private void StartSummon(string allyName)
    {
        Debug.Log($"[AllyStandPage] Starting summon for {allyName}");
        
        // Send merge request to backend
        var apiParams = new { ally_name = allyName, shards_used = 10 };
        PlayerWebSocketApi.Instance.Action("summon_ally", apiParams, OnSummonSuccess, OnSummonError);
    }
    
    private void OnSummonSuccess(JObject response)
    {
        Debug.Log($"[AllyStandPage] Summon success: {response}");
        
        try
        {
            // Handle player data update more carefully to prevent data loss
            if (response["player"] != null)
            {
                var newPlayerData = response["player"].ToObject<Player>();
                Debug.Log($"[AllyStandPage] Backend returned player data - Gold: {newPlayerData.GoldCoin}, Diamond: {newPlayerData.Diamond}");
                Debug.Log($"[AllyStandPage] Backend returned items count: {newPlayerData.ItemsJson?.Count ?? 0}");
                
                // Get current player data to preserve existing items
                var currentPlayer = PlayerProfile.Data.Player;
                if (currentPlayer != null && newPlayerData.ItemsJson != null)
                {
                    // Log what the backend is sending us
                    foreach (var item in newPlayerData.ItemsJson)
                    {
                        Debug.Log($"[AllyStandPage] Backend item: {item.Key} = {item.Value}");
                    }
                    
                    // Update specific items from backend response instead of replacing everything
                    foreach (var backendItem in newPlayerData.ItemsJson)
                    {
                        currentPlayer.ItemsJson[backendItem.Key] = backendItem.Value;
                        Debug.Log($"[AllyStandPage] Updated {backendItem.Key} to {backendItem.Value}");
                    }
                    
                    // Update currency values
                    currentPlayer.GoldCoin = newPlayerData.GoldCoin;
                    currentPlayer.Diamond = newPlayerData.Diamond;
                    
                    Debug.Log($"[AllyStandPage] After update - Gold: {currentPlayer.GoldCoin}, Diamond: {currentPlayer.Diamond}");
                    Debug.Log($"[AllyStandPage] After update - Total items: {currentPlayer.ItemsJson?.Count ?? 0}");
                }
                else
                {
                    // Fallback: use SetPlayer if we don't have current data
                    Debug.LogWarning("[AllyStandPage] No current player data, using SetPlayer");
                    PlayerProfile.Data.SetPlayer(newPlayerData);
                }
            }
            
            // Handle sidekick data
            if (response["sidekick"] != null)
            {
                var newSidekick = response["sidekick"].ToObject<Sidekick>();
                PlayerProfile.Data.AddSidekick(newSidekick);
            }
            
            if (response["sidekicks"] != null)
            {
                var sidekicks = response["sidekicks"].ToObject<List<Sidekick>>();
                PlayerProfile.Data.SetSidekicks(sidekicks);
            }
            
            string allyName = response["ally_summoned"]?.ToString() ?? "Unknown";
            Debug.Log($"✅ Successfully summoned {allyName}!");
            
            // **CRITICAL**: Ensure ally is permanently unlocked
            if (!string.IsNullOrEmpty(allyName) && allyName != "Unknown")
            {
                var currentPlayer = PlayerProfile.Data.Player;
                if (currentPlayer != null)
                {
                    // Initialize SummonedAllies list if null
                    if (currentPlayer.SummonedAllies == null)
                    {
                        currentPlayer.SummonedAllies = new List<string>();
                    }
                    
                    // Add ally to summoned allies list for permanent unlock
                    // Try multiple formats to ensure compatibility
                    string allyForList = allyName.ToLower(); // "02_gideon"
                    string allyNameOnly = allyName.Split('_').Length > 1 ? allyName.Split('_')[1].ToLower() : allyName.ToLower(); // "gideon"
                    
                    Debug.Log($"[AllyStandPage] Trying to add ally - Full: '{allyForList}', Name only: '{allyNameOnly}'");
                    
                    // Add both formats to be safe
                    if (!currentPlayer.SummonedAllies.Contains(allyForList))
                    {
                        currentPlayer.SummonedAllies.Add(allyForList);
                        Debug.Log($"[AllyStandPage] ✅ Added '{allyForList}' to permanent SummonedAllies list");
                    }
                    
                    if (!currentPlayer.SummonedAllies.Contains(allyNameOnly))
                    {
                        currentPlayer.SummonedAllies.Add(allyNameOnly);
                        Debug.Log($"[AllyStandPage] ✅ Added '{allyNameOnly}' to permanent SummonedAllies list");
                    }
                    
                    // Log current summoned allies for debugging
                    Debug.Log($"[AllyStandPage] Current SummonedAllies: [{string.Join(", ", currentPlayer.SummonedAllies)}]");
                }
            }
            Debug.Log($"✅ Successfully summoned {allyName}!");
            
            // Force refresh OtherDetailBox to update shard count immediately
            if (OtherDetailBox.Instance != null)
            {
                Debug.Log("[AllyStandPage] Forcing OtherDetailBox refresh to update shard count");
                OtherDetailBox.Instance.RefreshDetailBox();
            }
            
            // Also refresh the Others tab grid to show updated shard quantities
            var heroBlockSetup = FindObjectOfType<HeroBlockSetup>();
            if (heroBlockSetup != null)
            {
                Debug.Log("[AllyStandPage] Forcing Others tab refresh to update grid shard count");
                heroBlockSetup.UpdateOthersTab();
            }
            else
            {
                Debug.LogWarning("[AllyStandPage] HeroBlockSetup not found, cannot refresh Others tab grid");
            }
            
            // Keep the celebration panel open as success indicator
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AllyStandPage] Error processing summon response: {ex.Message}");
        }
    }
    
    private void OnSummonError(JObject error)
    {
        Debug.LogError($"[AllyStandPage] Summon error: {error}");
        
        string errorMessage = error["error"]?.ToString() ?? "Unknown error occurred";
        Debug.LogError($"[AllyStandPage] Backend error: {errorMessage}");
        
        // Could show error message in UI or close panel
        // For now, keep panel open but log error
    }
    
    private void LoadAllyIllustration(string index, string name)
    {
        string illustrationPath = $"UILoading/CharacterImages/Stand_Illustration/P_{index}_{name}";
        Sprite illustrationSprite = Resources.Load<Sprite>(illustrationPath);
        
        if (illustrationSprite != null)
        {
            allyIllustrationImage.sprite = illustrationSprite;
        }
        else
        {
            Debug.LogWarning($"[AllyStandPage] Illustration not found: {illustrationPath}");
        }
    }
}
