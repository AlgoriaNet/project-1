using UnityEngine;
using System;
using TMPro;
using model;
using Newtonsoft.Json.Linq;
using WebSocket;

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
    private static PurchaseWebSocketApi _wsSocketApi;

    void Start()
    {
        Debug.Log("ChestManager Start() has been called.");

        // _wsSocketApi = PurchaseWebSocketApi.Instance;

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

            // Update countdown display immediately
            int minutes = Mathf.FloorToInt(countdownTime / 60);
            int seconds = Mathf.FloorToInt(countdownTime % 60);
            countdownText.text = $"{minutes:00}:{seconds:00}";

            // ← ADD THIS: Reset animation state properly
            chestAnimator?.ResetTrigger("ChestReady");
            chestAnimator?.Play("Idle", 0, 0f);

            PlayerPrefs.SetInt("ChestBoxVisible", 1);
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
            return;
        }

        // Load saved state
        currentCountdownIndex = PlayerPrefs.GetInt("CurrentCountdownIndex", currentCountdownIndex);
        countdownTime = PlayerPrefs.GetFloat("CountdownTime", countdownTime);

        // // Calculate elapsed time based on the saved LastSavedTime
        // float elapsedTime = (float)(DateTime.Now - DateTime.Parse(PlayerPrefs.GetString("LastSavedTime", DateTime.Now.ToString()))).TotalSeconds;

        // Calculate elapsed time based on the saved LastSavedTime
        float elapsedTime = 0f;
        if (DateTime.TryParse(PlayerPrefs.GetString("LastSavedTime", DateTime.Now.ToString()), out DateTime lastSavedTime))
        {
            elapsedTime = (float)(DateTime.Now - lastSavedTime).TotalSeconds;
        }

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
            // Stop the chest ready animation immediately when clicked
            StopAnimation();

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
        // int[] rewardMultipliers = System.Array.ConvertAll(multipliersString.Split(','), int.Parse);

        int[] rewardMultipliers;
        try
        {
            rewardMultipliers = System.Array.ConvertAll(multipliersString.Split(','), int.Parse);
        }
        catch
        {
            rewardMultipliers = new int[] { 2, 3, 4, 5 }; // Default fallback
        }

        if (rewardText != null && currentCountdownIndex >= 0 && currentCountdownIndex < rewardMultipliers.Length)
        {
            rewardText.text = $"x{rewardMultipliers[currentCountdownIndex]}";
        }
    }

    // Method triggered by the Claim Button
    public void ClaimReward()
    {
        // string expiryString = PlayerPrefs.GetString("MonthlyCardExpiry", "");
        // bool isMonthlyCardActive = false;
        // if (DateTime.TryParse(expiryString, out DateTime expiryDate))
        // {
        //     isMonthlyCardActive = DateTime.Now < expiryDate;
        // }

        string expiryString = PlayerProfile.Data?.Player?.MonthlyCardExpiry 
                            ?? PlayerPrefs.GetString("MonthlyCardExpiry", "");

        bool isMonthlyCardActive = DateTime.TryParse(expiryString, out DateTime expiryDate) 
                                && DateTime.Now < expiryDate;

        Debug.Log("Monthly card active: " + isMonthlyCardActive);

        if (isMonthlyCardActive)
        {
            adTV?.SetActive(false);
            Debug.Log("Reward claimed directly with monthly card!");

            // Give reward immediately for monthly card users
            GiveRewardAndProceed();
        }
        else
        {
            adTV?.SetActive(true);
            Debug.Log("Show reward ad video to claim reward!");
            if (GoogleMobileAdsScript.This.CheckRewardedAd())
            {
                // Pass the reward method as callback - will execute AFTER ad completion
                GoogleMobileAdsScript.This.ShowRewardedAd("gold_reward", GiveRewardAndProceed);
            }
            else
            {
                Debug.Log("Rewarded ad not available, showing interstitial ad instead.");
                // For interstitial ads, give reward immediately since there's no reward mechanism
                GoogleMobileAdsScript.This.ShowInterstitialAd(GiveRewardAndProceed);
            }
        }
    }

    // Moved the reward logic to a separate method so it can be called as a callback
    private void GiveRewardAndProceed()
    {
        // Hide the reward popup immediately
        rewardPopup?.SetActive(false);

        // Get multiplier
        string multipliersString = PlayerPrefs.GetString("RewardMultipliers", "2,3,4,5");
        int[] rewardMultipliers;
        try
        {
            rewardMultipliers = Array.ConvertAll(multipliersString.Split(','), int.Parse);
        }
        catch
        {
            rewardMultipliers = new int[] { 2, 3, 4, 5 }; // Default fallback
        }
        int rewardAmount = (currentCountdownIndex >= 0 && currentCountdownIndex < rewardMultipliers.Length)
            ? rewardMultipliers[currentCountdownIndex]
            : 1;

        // Use API to update gold (similar to GoldPurchase.cs)
        _wsSocketApi.Action("add_gold", new { type = rewardAmount.ToString() }, AfterChestReward);
        Debug.Log($"Chest reward of {rewardAmount} gold claimed via API.");
    }

    // Add callback method for API response (similar to GoldPurchase.cs)
    public void AfterChestReward(JObject _object)
    {
        // Handle the API response
        int gold = _object.GetValue("gold").Value<int>();

        // Update Player model only - with protection
        try
        {
            PlayerProfile.Data.Player.GoldCoin = gold;
            PlayerProfile.Data.NotifyListeners("Player");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to update player profile: {e.Message}");
            // Continue execution anyway
        }

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