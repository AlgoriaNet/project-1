using System;
using System.Collections;
using UnityEngine;
using Newtonsoft.Json.Linq;
using model;
using WebSocket;

/// <summary>
/// Manages hourly energy/stamina claims via WebSocket API.
/// Follows DG's specification: hourly_claim with {type: "hourly"}
/// </summary>
public class HourlyEnergyManager : MonoBehaviour
{
    // Constants
    private const string LAST_HOURLY_CLAIM_KEY = "LastHourlyEnergyClaimTime";
    private const int HOURLY_CLAIM_INTERVAL_SECONDS = 3600; // 1 hour
    private const float CHECK_INTERVAL_SECONDS = 300f; // Check every 5 minutes
    private const int MAX_CLAIMS_PER_SESSION = 24; // Cap offline catch-up to 24 hours max
    
    // WebSocket API reference
    private PlayerWebSocketApi _energyApi;
    
    // Internal state
    private float _timeSinceLastCheck = 0f;
    private DateTime _lastClaimTime;
    private bool _hasPerformedInitialCheck = false;

    void Start()
    {  
        // Load the last claim time from PlayerPrefs
        LoadLastClaimTime();
        
        // Perform initial check after a delay to allow WebSocket and player data to be ready
        StartCoroutine(DelayedInitialCheck());
    }
    
    private IEnumerator DelayedInitialCheck()
    {
        // Wait for WebSocket and player data to be ready
        yield return new WaitForSeconds(5f);        
       
        // Initialize WebSocket API with safety checks
        while (_energyApi == null)
        {
            try
            {
                _energyApi = PlayerWebSocketApi.Instance;
                if (_energyApi != null)
                {
                    break;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[HourlyEnergyManager] WebSocket not ready yet: {ex.Message}");
            }
            
            yield return new WaitForSeconds(2f);
        }
        
        // Wait for player data to be available
        while (PlayerProfile.Data?.Player == null)
        {
            yield return new WaitForSeconds(1f);
        }
        
        CheckAndClaimHourlyEnergy();
        _hasPerformedInitialCheck = true;
    }
    
    void Update()
    {
        _timeSinceLastCheck += Time.deltaTime;
        
        // If we haven't performed the initial check yet, skip regular checks
        if (!_hasPerformedInitialCheck)
            return;
            
        // Safety check: ensure we have player data and WebSocket API
        if (PlayerProfile.Data?.Player == null || _energyApi == null)
            return;
        
        if (_timeSinceLastCheck >= CHECK_INTERVAL_SECONDS)
        {
            _timeSinceLastCheck = 0f;
            CheckAndClaimHourlyEnergy();
        }
    }
    
    private void LoadLastClaimTime()
    {
        if (PlayerPrefs.HasKey(LAST_HOURLY_CLAIM_KEY))
        {
            string lastClaimString = PlayerPrefs.GetString(LAST_HOURLY_CLAIM_KEY);
            if (DateTime.TryParse(lastClaimString, out DateTime parsedTime))
            {
                _lastClaimTime = parsedTime;
            }
            else
            {
                _lastClaimTime = DateTime.Now;
                SaveLastClaimTime();
            }
        }
        else
        {
            // First time running - set last claim to now (no retroactive claims)
            _lastClaimTime = DateTime.Now;
            SaveLastClaimTime();
        }
    }
    
    private void SaveLastClaimTime()
    {
        PlayerPrefs.SetString(LAST_HOURLY_CLAIM_KEY, _lastClaimTime.ToString("o"));
        PlayerPrefs.Save();
    }
    
    private void CheckAndClaimHourlyEnergy()
    {
        DateTime now = DateTime.Now;
        TimeSpan timeSinceLastClaim = now - _lastClaimTime;
        
        // Calculate how many full hours have passed since last claim
        int hoursToReclaim = (int)(timeSinceLastClaim.TotalSeconds / HOURLY_CLAIM_INTERVAL_SECONDS);        
 
        // Check if at least 1 hour has passed since last claim
        if (hoursToReclaim >= 1)
        {
            // Cap the claims to prevent excessive API calls (e.g., if offline for days)
            int claimsToMake = Mathf.Min(hoursToReclaim, MAX_CLAIMS_PER_SESSION);
            
            if (hoursToReclaim > MAX_CLAIMS_PER_SESSION)
            {
                Debug.LogWarning($"[HourlyEnergyManager] {hoursToReclaim} hours missed, but capping to {MAX_CLAIMS_PER_SESSION} claims per session");
            }            

            ClaimHourlyEnergy(claimsToMake);
        }
    }
    
    private void ClaimHourlyEnergy(int hoursToClaim = 1)
    {
        if (_energyApi == null)
        {
            Debug.LogWarning("[HourlyEnergyManager] WebSocket API not initialized yet, skipping claim");
            return;
        }
        
        // Check if WebSocketManager is ready (additional safety check)
        try
        {
            if (WebSocketManager.Instance == null)
            {
                Debug.LogWarning("[HourlyEnergyManager] WebSocketManager not ready, skipping claim");
                return;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[HourlyEnergyManager] WebSocket not ready: {ex.Message}");
            return;
        }       
        
        // Use coroutine to space out multiple claims slightly
        StartCoroutine(SendHourlyClaimsCoroutine(hoursToClaim));
    }
    
    private IEnumerator SendHourlyClaimsCoroutine(int hoursToClaim)
    {
        for (int i = 0; i < hoursToClaim; i++)
        {
            // Use DG's exact API specification - only {type: "hourly"}
            var apiParams = new { type = "hourly" };
            
            _energyApi.Action("hourly_claim", apiParams, OnHourlyClaimResponse, OnHourlyClaimError);
            
            // Small delay between claims to avoid overwhelming the server
            if (i < hoursToClaim - 1)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    
    private void OnHourlyClaimResponse(JObject response)
    {       
        try
        {
            // Check if we have player data (indicates success)
            if (response["player"] != null)
            {
                // Extract stamina from response and update only that field
                var responsePlayer = response["player"].ToObject<Player>();
                var oldStamina = PlayerProfile.Data.Player.Stamina;
                
                // Use proper partial update method instead of overwriting entire player object
                PlayerProfile.Data.UpdateStamina(responsePlayer.Stamina);
                
                var newStamina = PlayerProfile.Data.Player.Stamina;
                var staminaGained = newStamina - oldStamina;

                // Update last claim time to now (for each successful claim)
                _lastClaimTime = DateTime.Now;
                SaveLastClaimTime();
            }
            else
            {
                Debug.LogError("[HourlyEnergyManager] No player data in response");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[HourlyEnergyManager] Error processing response: {ex.Message}");
        }
    }
    
    private void OnHourlyClaimError(JObject error)
    {
        Debug.LogError($"[HourlyEnergyManager] Error: {error}");
    }
    
    /// <summary>
    /// Public method to manually trigger a single hourly energy claim.
    /// Can be called from UI or other scripts.
    /// </summary>
    public void ManualHourlyClaim()
    {
        if (_energyApi == null)
        {
            Debug.LogWarning("[HourlyEnergyManager] Cannot make manual claim - WebSocket API not ready");
            return;
        }
        
        ClaimHourlyEnergy(1);
    }
    
    /// <summary>
    /// Debug method to show current status and force a check
    /// </summary>
    public void DebugStatus()
    {
        DateTime now = DateTime.Now;
        TimeSpan timeSinceLastClaim = now - _lastClaimTime;
        int hoursToReclaim = (int)(timeSinceLastClaim.TotalSeconds / HOURLY_CLAIM_INTERVAL_SECONDS);
        
        try
        {
            bool wsManagerReady = WebSocketManager.Instance != null;
        }
        catch (System.Exception)
        {
        }
        
        // Force a check
        CheckAndClaimHourlyEnergy();
    }
    
    /// <summary>
    /// Reset the last claim time (for testing purposes)
    /// </summary>
    public void ResetLastClaimTime()
    {
        _lastClaimTime = DateTime.Now;
        SaveLastClaimTime();
    }
}
