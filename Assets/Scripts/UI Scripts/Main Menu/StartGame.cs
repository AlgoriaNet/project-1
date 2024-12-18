using System;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    public static DateTime loginTime; // Store the login time
    public static string detectedLanguage; // Store the detected language
    private bool isNewDay; // Check if it's a new day

    public ChestManager chestManager; // Assign the ChestManager object in the Inspector

    void Start()
    {
        // Store the login time
        loginTime = DateTime.Now;
        Debug.Log($"Login Time: {loginTime}");

        // Detect and store the system language
        detectedLanguage = Application.systemLanguage.ToString();
        Debug.Log($"Detected Language: {detectedLanguage}");
    
        // Handle daily reset
        HandleDailyReset();

    }

    private void HandleDailyReset()
    {
        // Get the last login date from PlayerPrefs
        string lastLoginDate = PlayerPrefs.GetString("LastLoginDate", string.Empty);
        string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

        // Check if the date has changed
        isNewDay = lastLoginDate != currentDate;

        // isNewDay = true;  // for test only, to trigger the chest box. 

        if (isNewDay)
        {
            Debug.Log("New day detected! Resetting daily features...");
            PlayerPrefs.SetString("LastLoginDate", currentDate);

            // Reset any daily-dependent features
            ResetDailyFeatures();

            if (chestManager != null)
            {
                // Re-enable chest box if it was hidden
                chestManager.chestBox.SetActive(true);
                PlayerPrefs.SetInt("ChestBoxVisible", 1); // Ensure visibility is stored in PlayerPrefs

                // Stop any active animations
                chestManager.StopAnimation();

                // Start a new 5-minute countdown
                chestManager.StartFirstCountdown();

                // Clear old countdown data
                PlayerPrefs.DeleteKey("CurrentCountdownIndex");
                PlayerPrefs.DeleteKey("CountdownTime");
            }
        }
        else
        {
            Debug.Log("Same day login. No reset needed.");
        }
    }

    private void ResetDailyFeatures()
    {
        Debug.Log("Performing daily reset tasks...");

        // Placeholder logic: Set dummy multipliers for now
        int[] multipliers = new int[] { 2, 3, 4, 5 }; // Dummy multipliers

        // TODO: Replace this logic with actual server integration to fetch multipliers
        Debug.Log("Dummy multipliers set. Will replace with server values in future implementation.");

        // Store multipliers locally for ChestManager to access later
        PlayerPrefs.SetString("RewardMultipliers", string.Join(",", multipliers));
    }
}