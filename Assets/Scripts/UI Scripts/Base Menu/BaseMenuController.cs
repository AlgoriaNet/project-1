using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WebSocket;
using model;
using Newtonsoft.Json.Linq;

public class BaseMenuController : MonoBehaviour
{
    public GameObject commonPage;
    public GameObject baseScrollView;

    public GameObject expeditionPage;
    public GameObject mailHall;
    public GameObject braveTrialPage;
    public GameObject abyssClashPage;
    public GameObject divineTowerPage;
    public GameObject defensePage;
    public GameObject rewardsPage; // Assign RewardsPage in Inspector

    public GameObject leaderPage;
    public GameObject leaderItemPrefab; // Assign LeaderItem prefab in Inspector
    public Transform leaderContent; // Assign Content (inside Scroll View) in Inspector
    private List<GameObject> spawnedLeaders = new List<GameObject>(); // Track spawned items
   
    public GameObject achievePage;
    public GameObject achieveItemPrefab; // Assign LeaderItem prefab in Inspector
    public Transform achieveContent; // Assign Content (inside Scroll View) in Inspector
    private List<GameObject> spawnedAchievers = new List<GameObject>(); // Track spawned items

    public GameObject tavernPage;
    public GameObject travernButton1; // Assign Button_1 in TravenPage
    public GameObject travernButton2; // Assign Button_2 in TravenPage
    private const string lastEnergyClaimTimeKey1 = "LastEnergyClaimTime_12PM";
    private const string lastEnergyClaimTimeKey2 = "LastEnergyClaimTime_19PM";

    public TextMeshProUGUI attemptsText; // Assign AttemptsText in Inspector
    public Button saluteButton; // Assign Salute Button in Inspector
    private const string lastSaluteKey = "LastSaluteDate";
    private const string saluteAttemptsKey = "SaluteAttempts";
    private const int maxAttempts = 3;
    private int remainingAttempts;
    
    // WebSocket API for energy/stamina claims (using PlayerWebSocketApi for now)
    private PlayerWebSocketApi _energyApi;

    private void Start()
    {
        _energyApi = PlayerWebSocketApi.Instance; // Initialize WebSocket API (using PlayerWebSocketApi for energy claims)
        CheckEnergyClaimStatus(); // Check button visibility for travern's energy claim on start
        CheckResetDaily(); // Reset if a new day
        UpdateAttempts();
    }

    public void CloseBackground()
    {
        baseScrollView.SetActive(false);
        commonPage.SetActive(false);
    }

    public void OpenRewardsPage()
    {
        rewardsPage.SetActive(true);
        commonPage.SetActive(false);
        baseScrollView.SetActive(false);
        // ⚠ Reward claim logic (12-hour countup) is handled in OfflineTimer.cs
    }

    public void CloseRewardsPage()
    {
        rewardsPage.SetActive(false);
        commonPage.SetActive(true);
        baseScrollView.SetActive(true);
    }

    // ******************************************************************
    // LeaderPage Codes begin here
    // ******************************************************************
    public void OpenLeaderPage()
    {
        leaderPage.SetActive(true);
        commonPage.SetActive(false);
        baseScrollView.SetActive(false);
        LoadLeaderItems(); // 🔹 Load dynamically when page opens
    }

    public void CloseLeaderPage()
    {
        leaderPage.SetActive(false);
        commonPage.SetActive(true);
        baseScrollView.SetActive(true);
    }

    private void LoadLeaderItems()
    {
        // Clear existing items
        foreach (GameObject item in spawnedLeaders)
        {
            Destroy(item);
        }
        spawnedLeaders.Clear();

        // Load ranking images (4 to 20)
        string rankPath = "UILoading/LeaderRank";
        Sprite[] rankImages = Resources.LoadAll<Sprite>(rankPath);

        // Load user icons (Mock logic for now)
        string iconPath = "UILoading/CharacterImages/UserIcons/WhiteBorderIcons";
        Sprite[] userIcons = Resources.LoadAll<Sprite>(iconPath);

        // Loop from rank 4 to 20
        for (int rank = 4; rank <= 20; rank++)
        {
            GameObject newItem = Instantiate(leaderItemPrefab, leaderContent);
            newItem.name = $"Leader_{rank}";

            // **Set spacing dynamically after first item instantiation**
            if (rank == 4) 
            {
                float itemHeight = newItem.GetComponent<RectTransform>().rect.height;
                leaderContent.GetComponent<VerticalLayoutGroup>().spacing = itemHeight / 6f;            
            }

            // Assign Rank Image
            Image rankImage = newItem.transform.Find("Rank").GetComponent<Image>();
            rankImage.sprite = Array.Find(rankImages, img => img.name == rank.ToString());

            // Assign Icon Image (Mock logic: Load icons randomly)
            Image iconImage = newItem.transform.Find("Icon").GetComponent<Image>();
            if (userIcons.Length > 0)
            {
                iconImage.sprite = userIcons[UnityEngine.Random.Range(0, userIcons.Length)];
            }

            spawnedLeaders.Add(newItem);
        }
    }

    private void CheckResetDaily()
    {
        string lastSaluteDate = PlayerPrefs.GetString(lastSaluteKey, "");
        string today = DateTime.Now.ToString("yyyy-MM-dd");

        if (lastSaluteDate != today)
        {
            PlayerPrefs.SetString(lastSaluteKey, today);
            PlayerPrefs.SetInt(saluteAttemptsKey, maxAttempts);
            PlayerPrefs.Save();
        }

        remainingAttempts = PlayerPrefs.GetInt(saluteAttemptsKey, maxAttempts);
    }

    public void Salute()
    {
        if (remainingAttempts > 0)
        {
            remainingAttempts--;
            PlayerPrefs.SetInt(saluteAttemptsKey, remainingAttempts);
            PlayerPrefs.Save();
            UpdateAttempts();
        }
    }

    private void UpdateAttempts()
    {
        attemptsText.text = $"Attempts: {remainingAttempts}/{maxAttempts}";
        // saluteButton.interactable = remainingAttempts > 0;
        saluteButton.gameObject.SetActive(remainingAttempts > 0);
    }

    // ******************************************************************
    // LeaderPage Codes end here
    // ******************************************************************


    public void OpenDefensePage()
    {
        defensePage.SetActive(true);
        commonPage.SetActive(false);
        baseScrollView.SetActive(false);
    }

    public void CloseDefensePage()
    {
        defensePage.SetActive(false);
        commonPage.SetActive(true);
        baseScrollView.SetActive(true);
    }

    public void OpenAchievePage()
    {
        achievePage.SetActive(true);
        commonPage.SetActive(false);
        baseScrollView.SetActive(false);

        LoadAchieveItems();
    }

    private void LoadAchieveItems()
    {
        // Clear existing items
        foreach (GameObject item in spawnedAchievers)
        {
            Destroy(item);
        }
        spawnedAchievers.Clear();

        // Load user icons (Mock logic: Randomly load 20 icons)
        string iconPath = "UILoading/CharacterImages/UserIcons/WhiteBorderIcons";
        Sprite[] userIcons = Resources.LoadAll<Sprite>(iconPath);

        for (int i = 0; i < 20; i++)
        {
            GameObject newItem = Instantiate(achieveItemPrefab, achieveContent);
            newItem.name = $"Achieve_{i + 1}";

            // Assign Icon Image (Random mock logic)
            Image iconImage = newItem.transform.Find("Icon").GetComponent<Image>();
            if (userIcons.Length > 0)
            {
                iconImage.sprite = userIcons[UnityEngine.Random.Range(0, userIcons.Length)];
            }

            spawnedAchievers.Add(newItem);
        }
    }

    public void CloseAchievePage()
    {
        achievePage.SetActive(false);
        commonPage.SetActive(true);
        baseScrollView.SetActive(true);
    }

    // ******************************************************************
    // ExpeditionPage Codes Begins
    // ******************************************************************
    public void OpenExpeditionPage()
    {
        expeditionPage.SetActive(true);
        commonPage.SetActive(false);
        baseScrollView.SetActive(false);
    }

    public void CloseExpeditionPage()
    {
        expeditionPage.SetActive(false);
        commonPage.SetActive(true);
        baseScrollView.SetActive(true);
    }

    public void OpenBraveTrialPage()
    {
        braveTrialPage.SetActive(true);
        mailHall.SetActive(false);
    }

    public void CloseBraveTrialPage()
    {
        braveTrialPage.SetActive(false);
        mailHall.SetActive(true);
    }

    public void OpenAbyssClashPage()
    {
        abyssClashPage.SetActive(true);
        mailHall.SetActive(false);
    }

    public void CloseAbyssClashPage()
    {
        abyssClashPage.SetActive(false);
        mailHall.SetActive(true);
    }

    public void OpenDivineTowerPage()
    {
        divineTowerPage.SetActive(true);
        mailHall.SetActive(false);
    }

    public void CloseDivineTowerPage()
    {
        divineTowerPage.SetActive(false);
        mailHall.SetActive(true);
    }
    // ******************************************************************
    // ExpeditionPage Codes Ends here
    // ******************************************************************


    // ******************************************************************
    // TraverPage Codes Begins
    // ******************************************************************
    public void OpenTavernPage()
    {
        tavernPage.SetActive(true);
        commonPage.SetActive(false);
        baseScrollView.SetActive(false);
        CheckEnergyClaimStatus(); // Ensure buttons update when page opens
    }

    public void CloseTavernPage()
    {
        tavernPage.SetActive(false);
        commonPage.SetActive(true);
        baseScrollView.SetActive(true);
    }

    private void CheckEnergyClaimStatus()
    {
        DateTime now = DateTime.Now;
        DateTime today12PM = new DateTime(now.Year, now.Month, now.Day, 12, 0, 0);
        DateTime today19PM = new DateTime(now.Year, now.Month, now.Day, 19, 0, 0);

        // Check Button_1 visibility
        if (now >= today12PM && !HasEnergyClaimedToday(lastEnergyClaimTimeKey1))
        {
            travernButton1.SetActive(true);
        }
        else
        {
            travernButton1.SetActive(false);
        }

        // Check Button_2 visibility
        if (now >= today19PM && !HasEnergyClaimedToday(lastEnergyClaimTimeKey2))
        {
            travernButton2.SetActive(true);
        }
        else
        {
            travernButton2.SetActive(false);
        }
    }

    private bool HasEnergyClaimedToday(string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            string lastEnergyClaimString = PlayerPrefs.GetString(key);
            DateTime lastEnergyClaimTime = DateTime.Parse(lastEnergyClaimString);
            return lastEnergyClaimTime.Date == DateTime.Now.Date; // Same day = already claimed
        }
        return false;
    }

    /// <summary>
    /// Claims energy/stamina via WebSocket API for the specified time slot.
    /// Frontend sends claim request and updates UI only upon backend confirmation.
    /// </summary>
    /// <param name="buttonIndex">1 for 12PM slot, 2 for 7PM slot</param>
    public void ClaimEnergy(int buttonIndex)
    {
        Debug.Log($"[ClaimEnergy] Requesting energy claim - ButtonIndex: {buttonIndex}");
        
        // Disable button temporarily to prevent double-clicks
        if (buttonIndex == 1)
            travernButton1.GetComponent<Button>().interactable = false;
        else if (buttonIndex == 2)
            travernButton2.GetComponent<Button>().interactable = false;
        
        // Send claim request to backend via WebSocket using DG's API
        var apiParams = new { type = "daily" };
        
        Debug.Log($"[ClaimEnergy] Sending request with params: {Newtonsoft.Json.JsonConvert.SerializeObject(apiParams)}");
        
        _energyApi.Action("daily_claim", apiParams, OnEnergyClaimResponse, OnEnergyClaimError);
    }
    
    /// <summary>
    /// Handles the response from the backend energy claim API.
    /// Updates player data and UI only if the backend confirms the claim was successful.
    /// </summary>
    /// <param name="response">Backend response containing player data or error info</param>
    private void OnEnergyClaimResponse(JObject response)
    {
        Debug.Log($"[ClaimEnergy] Received response: {response}");
        
        try
        {
            // Check if we have player data (indicates success)
            if (response["player"] != null)
            {
                Debug.Log("[ClaimEnergy] Success response received!");
                
                // Extract stamina from response and update only that field
                var responsePlayer = response["player"].ToObject<Player>();
                var oldStamina = PlayerProfile.Data.Player.Stamina;
                
                // Use proper partial update method instead of overwriting entire player object
                PlayerProfile.Data.UpdateStamina(responsePlayer.Stamina);
                
                var newStamina = PlayerProfile.Data.Player.Stamina;
                Debug.Log($"[ClaimEnergy] Stamina updated! Old: {oldStamina}, New: {newStamina}");
                
                // Calculate stamina gained
                int staminaGained = newStamina - oldStamina;
                
                // Update local claim tracking and hide buttons
                // Since we don't have buttonIndex from response, hide both buttons that might be active
                if (travernButton1.activeInHierarchy)
                {
                    PlayerPrefs.SetString(lastEnergyClaimTimeKey1, DateTime.Now.ToString());
                    travernButton1.SetActive(false);
                }
                if (travernButton2.activeInHierarchy)
                {
                    PlayerPrefs.SetString(lastEnergyClaimTimeKey2, DateTime.Now.ToString());
                    travernButton2.SetActive(false);
                }
                PlayerPrefs.Save();
                
                // Show success feedback UI
                ShowEnergyClaimSuccess(staminaGained);
            }
            else
            {
                // No player data means claim failed
                string errorMessage = response["message"]?.Value<string>() ?? "Energy claim failed - no player data returned";
                Debug.LogError($"[ClaimEnergy] Backend error: {errorMessage}");
                
                // Show error feedback UI
                ShowEnergyClaimError(errorMessage);
                
                // Re-enable buttons since claim failed
                if (travernButton1.GetComponent<Button>() != null && !travernButton1.GetComponent<Button>().interactable)
                    travernButton1.GetComponent<Button>().interactable = true;
                if (travernButton2.GetComponent<Button>() != null && !travernButton2.GetComponent<Button>().interactable)
                    travernButton2.GetComponent<Button>().interactable = true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ClaimEnergy] Exception processing response: {ex.Message}");
            
            // Show generic error feedback
            ShowEnergyClaimError("Failed to process energy claim response");
            
            // Try to restore button state if possible
            CheckEnergyClaimStatus();
        }
    }
    
    /// <summary>
    /// Restores button interactable state when energy claim fails
    /// </summary>
    private void RestoreButtonState(int buttonIndex)
    {
        if (buttonIndex == 1 && travernButton1.activeInHierarchy)
            travernButton1.GetComponent<Button>().interactable = true;
        else if (buttonIndex == 2 && travernButton2.activeInHierarchy)
            travernButton2.GetComponent<Button>().interactable = true;
    }
    
    /// <summary>
    /// Shows success feedback for energy claim (placeholder for UI implementation)
    /// </summary>
    private void ShowEnergyClaimSuccess(int staminaGained)
    {
        Debug.Log($"[UI] Energy claim success! +{staminaGained} Stamina gained");
        // TODO: Implement popup or notification UI showing stamina gained
    }
    
    /// <summary>
    /// Shows error feedback for energy claim (placeholder for UI implementation)  
    /// </summary>
    private void ShowEnergyClaimError(string errorMessage)
    {
        Debug.LogWarning($"[UI] Energy claim error: {errorMessage}");
        // TODO: Implement error popup or notification UI
    }

    /// <summary>
    /// Handles error response from energy claim API
    /// </summary>
    private void OnEnergyClaimError(JObject error)
    {
        Debug.LogError($"[ClaimEnergy] Error response: {error}");
        ShowEnergyClaimError("Network error occurred");
        CheckEnergyClaimStatus(); // Restore button states
    }
    // ******************************************************************
    // TraverPage Codes end here
    // ******************************************************************

    /// <summary>
    /// TEST ONLY: Check if WebSocket communication works without claiming energy
    /// </summary>
    public void TestEnergyConnection()
    {
        Debug.Log("[TEST] Testing WebSocket connection for energy system...");
        
        // Test with a fake action first to see if we get any response
        _energyApi.Action("test_connection", new { test = "energy_system" }, (response) =>
        {
            Debug.Log($"[TEST] Connection test response: {response}");
        }, (error) =>
        {
            Debug.LogError($"[TEST] Connection test error: {error}");
        });
    }
}