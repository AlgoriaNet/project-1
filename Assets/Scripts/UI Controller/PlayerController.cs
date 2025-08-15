using model;
using TMPro;
using UnityEngine;
using Newtonsoft.Json.Linq;
using WebSocket;

namespace UI_Controller
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerName;
        [SerializeField] private TMP_Text playerId;
        [SerializeField] private TMP_Text playerLevel;
        [SerializeField] private TMP_Text playerExp;
        [SerializeField] private TMP_Text playerGoldCoin;
        [SerializeField] private TMP_Text playerDiamond;
        [SerializeField] private TMP_Text playerStamina;
        
        private void Start()
        {
            PlayerProfile.Data.AddListener(UpdatePlayerInfo, "Player");
            
            // Subscribe to PlayerLevelChannel for level updates
            PlayerLevelWebSocketApi.Instance.Subscribe();
        }
      
        private bool _isRefreshingLevelInfo = false;

        private void UpdatePlayerInfo(ApplicationModel model)
        {
            Debug.Log("update top info: " + PlayerProfile.Data.Player);
            var player = PlayerProfile.Data.Player;
            
            // Don't try to refresh level info if player data is not loaded yet
            if (player == null)
            {
                Debug.Log("[PlayerController] Player data not loaded yet, skipping level refresh");
                return;
            }
            
            if (playerName != null) playerName.text = player?.Name;
            if (playerId != null) playerId.text = player?.Id.ToString();
            if (playerLevel != null) playerLevel.text = player?.Level.ToString();
            if (playerExp != null) playerExp.text = player?.Exp.ToString();
            if (playerGoldCoin != null) playerGoldCoin.text = NumberFormatter.FormatNumber(player?.GoldCoin ?? 0);
            if (playerDiamond != null) playerDiamond.text = NumberFormatter.FormatNumber(player?.Diamond ?? 0);
            if (playerStamina != null) playerStamina.text = NumberFormatter.FormatNumber(player?.Stamina ?? 0);
            
            // Only refresh level info if we're not already in the middle of a level refresh
            // This prevents infinite loops when the level API response updates the player data
            if (!_isRefreshingLevelInfo)
            {
                RefreshLevelInfo();
            }
        }
        
        /// <summary>
        /// Call get_level_info API to refresh level display with server calculations
        /// </summary>
        public void RefreshLevelInfo()
        {
            if (_isRefreshingLevelInfo)
            {
                Debug.Log("[PlayerController] Already refreshing level info, skipping duplicate request");
                return;
            }
            
            Debug.Log("[PlayerController] Calling get_level_info API to refresh level data");
            _isRefreshingLevelInfo = true;
            
            PlayerLevelWebSocketApi.Instance.Action("get_level_info", new { }, HandleLevelInfo, HandleLevelError);
        }
        
        private void HandleLevelInfo(JObject response)
        {
            _isRefreshingLevelInfo = false;
            Debug.Log($"[PlayerController] Level info received: {response}");
            
            // Success callback receives the data portion directly for successful requests (code 200)
            var levelInfo = response["level_info"];
            var updatedPlayer = response["updated_player"];
            
            if (levelInfo != null)
            {
                UpdateLevelDisplay(levelInfo);
                Debug.Log("[PlayerController] Level display updated successfully");
            }
            
            // Don't update the full player profile from level API response
            // The level display is already updated above, and updating the full profile 
            // can trigger unwanted side effects in other systems
            Debug.Log("[PlayerController] Level API response processed successfully");
        }
        
        private void HandleLevelError(JObject error)
        {
            _isRefreshingLevelInfo = false;
            Debug.LogError($"[PlayerController] Level info API error: {error}");
        }
        
        private void UpdateLevelDisplay(JToken levelInfo)
        {
            var currentLevel = levelInfo["current_level"]?.Value<int>() ?? 1;
            var currentExp = levelInfo["current_exp"]?.Value<int>() ?? 0;
            var expToNextLevel = levelInfo["exp_to_next_level"]?.Value<int>() ?? 0;
            var isMaxLevel = levelInfo["is_max_level"]?.Value<bool>() ?? false;
            var totalExpForNextLevel = levelInfo["total_exp_for_next_level"]?.Value<int>() ?? 0;
            
            // Calculate simple progress percentage: currentExp / totalExpForNextLevel
            float progressPercentage = 0f;
            if (!isMaxLevel && totalExpForNextLevel > 0)
            {
                progressPercentage = ((float)currentExp / totalExpForNextLevel) * 100f;
            }
            
            // Update basic level and EXP display
            if (playerLevel != null) playerLevel.text = isMaxLevel ? "MAX" : currentLevel.ToString();
            if (playerExp != null)
            {
                if (isMaxLevel)
                {
                    playerExp.text = "MAX LEVEL";
                }
                else
                {
                    playerExp.text = $"{currentExp}/{totalExpForNextLevel}";
                }
            }
            
            // Check for equipment unlocks
            var equipmentUnlocked = levelInfo["equipment_unlocked"];
            if (equipmentUnlocked != null && equipmentUnlocked.HasValues)
            {
                Debug.Log($"[PlayerController] Equipment unlocked: {equipmentUnlocked}");
                // TODO: Add equipment unlock notification UI
            }
            
            // Check for max level achievement
            if (isMaxLevel)
            {
                Debug.Log("[PlayerController] Player reached max level!");
                // TODO: Add max level celebration effects
            }
            
            Debug.Log($"[PlayerController] Level display updated: Level {currentLevel}, EXP {currentExp}/{totalExpForNextLevel} ({progressPercentage:F1}%)");
        }
        

        private void OnDestroy()
        {
            PlayerProfile.Data.RemoveListener(UpdatePlayerInfo, "Player");
        }
    }
}