using UnityEngine;
using System;
using TMPro;

public class ChestManager : MonoBehaviour
{
    public TextMeshProUGUI countdownText; // Assign the TMP text component for countdown display
    public TextMeshProUGUI rewardText; // Assign the Reward Popup Text in the Inspector
    public Animator chestAnimator; // Animator to control chest animations
    public GameObject rewardPopup; // Assign the Reward Popup in the Inspector
    public GameObject adTV;

    public GameObject chestBox; // Assign the Chest Box GameObject in the Inspector

    private int[] countdownDurations = { 300, 600, 900, 1800 }; // Durations in seconds (5, 10, 15, 30 minutes)
    private int currentCountdownIndex = 0;
    private float countdownTime;
    public bool isCountdownActive = false;

    void Start()
    {
        Debug.Log("ChestManager Start() has been called.");

        // ----- NEW DAILY RESET LOGIC (moved from StartGame) -----
        string storedLoginDate = PlayerPrefs.GetString("LastLoginDate", "");
        string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

        if (storedLoginDate != currentDate)
        {
            Debug.Log("New day detected! Resetting chest progress.");
            // Reset daily progress
            currentCountdownIndex = 0;
            countdownTime = countdownDurations[0];
            isCountdownActive = true; // Optionally auto-start the countdown
            chestBox.SetActive(true); // Ensure the chest box is visible
            PlayerPrefs.SetInt("ChestBoxVisible", 1);
            // Update the date keys in PlayerPrefs
            PlayerPrefs.SetString("LastLoginDate", currentDate);
            PlayerPrefs.SetString("LastSavedTime", DateTime.Now.ToString());
            SaveState();
        }
        // ----- END DAILY RESET LOGIC -----

        // Continue with the existing logic:
        bool isChestBoxVisible = PlayerPrefs.GetInt("ChestBoxVisible", 1) == 1;
        chestBox.SetActive(isChestBoxVisible);

        if (!isChestBoxVisible)
        {
            chestBox.SetActive(false);
            return;
        }

        // Load saved state
        currentCountdownIndex = PlayerPrefs.GetInt("CurrentCountdownIndex", currentCountdownIndex);
        countdownTime = PlayerPrefs.GetFloat("CountdownTime", countdownTime);

        // Calculate elapsed time based on the saved LastSavedTime
        float elapsedTime = (float)(DateTime.Now - DateTime.Parse(PlayerPrefs.GetString("LastSavedTime", DateTime.Now.ToString()))).TotalSeconds;

        if (elapsedTime >= countdownTime)
        {
            countdownTime = 0;
            isCountdownActive = false;
            countdownText.text = "Click Me!";
            chestAnimator?.SetTrigger("ChestReady");
        }
        else
        {
            countdownTime -= elapsedTime;
            isCountdownActive = true;
            chestAnimator?.ResetTrigger("ChestReady");
        }

        // Check monthly card status from expiry date
        string expiryDateStr = PlayerPrefs.GetString("MonthlyCardExpiry", "");
        if (DateTime.TryParse(expiryDateStr, out DateTime expiryDate) && DateTime.Now < expiryDate)
        {
            adTV?.SetActive(false); // Monthly card active
        }
        else
        {
            adTV?.SetActive(true);  // Monthly card expired or not bought
        }
    
        Debug.Log("MonthlyCardExpiry raw string: " + expiryDateStr);
        Debug.Log("Current time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        Debug.Log("Parsed expiry date: " + expiryDate.ToString("yyyy-MM-dd HH:mm:ss"));
        Debug.Log("Monthly card is active: " + (DateTime.Now < expiryDate));
    }

    void Update()
    {
        if (isCountdownActive)
        {
            countdownTime -= Time.deltaTime;

            if (countdownTime > 0)
            {
                // Update countdown display
                int minutes = Mathf.FloorToInt(countdownTime / 60);
                int seconds = Mathf.FloorToInt(countdownTime % 60);
                countdownText.text = $"{minutes:00}:{seconds:00}";
            }
            else
            {
                // Countdown complete
                countdownTime = 0;
                isCountdownActive = false;
                countdownText.text = "Click Me!";
                chestAnimator?.SetTrigger("ChestReady");
            }
        }
    }

    public void StartFirstCountdown()
    {
        if (currentCountdownIndex < countdownDurations.Length)
        {
            countdownTime = countdownDurations[currentCountdownIndex];
            isCountdownActive = true;

            // Save progress
            SaveState();
        }
    }

    public void OnChestClicked()
    {
        if (!isCountdownActive)
        {
            // Show the reward popup
            if (rewardPopup != null)
            {
                rewardPopup.SetActive(true);
                SetRewardText(); // Update the reward multiplier text
            }
        }
    }

    public void SetRewardText()
    {
        // Retrieve multipliers from PlayerPrefs
        string multipliersString = PlayerPrefs.GetString("RewardMultipliers", "2,3,4,5"); // Default fallback
        int[] rewardMultipliers = System.Array.ConvertAll(multipliersString.Split(','), int.Parse);

        if (rewardText != null && currentCountdownIndex >= 0 && currentCountdownIndex < rewardMultipliers.Length)
        {
            rewardText.text = $"x{rewardMultipliers[currentCountdownIndex]}";
        }
    }

    // Method triggered by the Claim Button
    public void ClaimReward()
    {
        string expiryString = PlayerPrefs.GetString("MonthlyCardExpiry", "");
        bool isMonthlyCardActive = false;
        if (DateTime.TryParse(expiryString, out DateTime expiryDate))
        {
            isMonthlyCardActive = DateTime.Now < expiryDate;
        }

        Debug.Log("Monthly card active: " + isMonthlyCardActive);

        if (isMonthlyCardActive)
        {
            Debug.Log("Reward claimed directly with monthly card!");
        }
        else
        {
            Debug.Log("Show reward ad video to claim reward!");
            if (GoogleMobileAdsScript.This.CheckRewardedAd())
            {
                GoogleMobileAdsScript.This.ShowRewardedAd("gold_reward");
            }
            else
            {
                Debug.Log("Rewarded ad not available, showing interstitial ad instead.");
                GoogleMobileAdsScript.This.ShowInterstitialAd();
            }
        }

        // Get multiplier
        string multipliersString = PlayerPrefs.GetString("RewardMultipliers", "2,3,4,5");
        int[] rewardMultipliers = Array.ConvertAll(multipliersString.Split(','), int.Parse);
        int rewardAmount = (currentCountdownIndex >= 0 && currentCountdownIndex < rewardMultipliers.Length)
            ? rewardMultipliers[currentCountdownIndex]
            : 1;

        // Update local PlayerPrefs
        int newGold = PlayerPrefs.GetInt("GoldCoin", 0) + rewardAmount;
        PlayerPrefs.SetInt("GoldCoin", newGold);
        PlayerPrefs.Save();

        // Update Player model
        var player = model.PlayerProfile.Data.Player;
        player.GoldCoin += rewardAmount;
        model.PlayerProfile.Data.SetPlayer(model.PlayerProfile.Data.Player);

        // Sync to server (you can replace this with actual implementation)
        Debug.Log($"[Sync] Gold updated: {player.GoldCoin}");

        Debug.Log($"Added {rewardAmount} gold. New total: {newGold}");

        StopAnimation();

        if (currentCountdownIndex < countdownDurations.Length - 1)
        {
            currentCountdownIndex++;
            countdownTime = countdownDurations[currentCountdownIndex];
            isCountdownActive = true;

            int minutes = Mathf.FloorToInt(countdownTime / 60);
            int seconds = Mathf.FloorToInt(countdownTime % 60);
            countdownText.text = $"{minutes:00}:{seconds:00}";

            SaveState();
        }
        else
        {
            Debug.Log("Final reward claimed. Chest box will disappear.");
            chestBox.SetActive(false);
            PlayerPrefs.SetInt("ChestBoxVisible", 0);
        }

        rewardPopup?.SetActive(false);
    }

    // Add a method to close the popup
    public void CloseRewardPopup()
    {
        if (rewardPopup != null)
        {
            rewardPopup.SetActive(false);
        }
    }

    // In ChestManager
    public void StopAnimation()
    {
        if (chestAnimator)
        {
            chestAnimator.ResetTrigger("ChestReady");
            chestAnimator.Play("Idle", 0, 0f);
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // Save state on game exit or pause
            SaveState();
        }
    }

    public void SaveState()
    {
        PlayerPrefs.SetInt("CurrentCountdownIndex", currentCountdownIndex);
        PlayerPrefs.SetFloat("CountdownTime", countdownTime);
        PlayerPrefs.SetString("LastSavedTime", System.DateTime.Now.ToString());
        PlayerPrefs.Save(); // Ensure data is written immediately
    }


    // Temporary for reset chestbox, remove it after test 
    public void ResetChestBox()
    {
        // Reset PlayerPrefs for the chest box
        PlayerPrefs.SetInt("CurrentCountdownIndex", 0);
        PlayerPrefs.SetFloat("CountdownTime", countdownDurations[0]);
        PlayerPrefs.SetString("LastSavedTime", System.DateTime.Now.ToString());
        PlayerPrefs.SetInt("ChestBoxVisible", 1); // Ensure the chest box is visible
        PlayerPrefs.Save(); // Write changes to disk

        // Reactivate chest box
        if (chestBox != null)
        {
            chestBox.SetActive(true);
        }

        // Reset state variables
        currentCountdownIndex = 0;
        countdownTime = countdownDurations[0];
        isCountdownActive = true;

        // 🔹 Reset the animation to its idle state
        chestAnimator?.ResetTrigger("ChestReady");

        Debug.Log("✅ ChestBox has been reset for testing.");
    }
}