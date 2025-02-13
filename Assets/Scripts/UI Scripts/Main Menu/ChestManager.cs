using UnityEngine;
using TMPro;

public class ChestManager : MonoBehaviour
{
    public TextMeshProUGUI countdownText; // Assign the TMP text component for countdown display
    public TextMeshProUGUI rewardText; // Assign the Reward Popup Text in the Inspector
    public Animator chestAnimator; // Animator to control chest animations
    public GameObject rewardPopup; // Assign the Reward Popup in the Inspector
    public bool hasMonthlyPass = true; // Test variable: true for monthly pass, false otherwise
    public GameObject chestBox; // Assign the Chest Box GameObject in the Inspector

    private int[] countdownDurations = { 300, 600, 900, 1800 }; // Durations in seconds (5, 10, 15, 30 minutes)
    private int currentCountdownIndex = 0;
    private float countdownTime;
    public bool isCountdownActive = false;

    void Start()
    {
        // // Debug print all PlayerPrefs values
        // Debug.Log("PlayerPrefs Values:");
        // Debug.Log($"ChestBoxVisible: {PlayerPrefs.GetInt("ChestBoxVisible", -1)}"); // Default -1 if not set
        // Debug.Log($"CurrentCountdownIndex: {PlayerPrefs.GetInt("CurrentCountdownIndex", -1)}"); // Default -1 if not set
        // Debug.Log($"CountdownTime: {PlayerPrefs.GetFloat("CountdownTime", -1)}"); // Default -1 if not set
        // Debug.Log($"LastSavedTime: {PlayerPrefs.GetString("LastSavedTime", "Not Set")}"); // Default 'Not Set' if not set
        // Debug.Log($"RewardMultipliers: {PlayerPrefs.GetString("RewardMultipliers", "Not Set")}"); // Default 'Not Set' if not set

        // Check if the chest box should be visible
        bool isChestBoxVisible = PlayerPrefs.GetInt("ChestBoxVisible", 1) == 1; // Default to visible
        chestBox.SetActive(isChestBoxVisible);

        if (!isChestBoxVisible)
        {
            chestBox.SetActive(false);
            return; // Exit Start if the chest box is hidden
        }

        // Load saved state
        currentCountdownIndex = PlayerPrefs.GetInt("CurrentCountdownIndex", 0);
        countdownTime = PlayerPrefs.GetFloat("CountdownTime", countdownDurations[currentCountdownIndex]);

        // Calculate elapsed time
        float elapsedTime = (float)(System.DateTime.Now - System.DateTime.Parse(PlayerPrefs.GetString("LastSavedTime", System.DateTime.Now.ToString()))).TotalSeconds;

        if (elapsedTime >= countdownTime)
        {
            // Countdown complete
            countdownTime = 0;
            isCountdownActive = false;
            countdownText.text = "Click Me!";
            chestAnimator?.SetTrigger("ChestReady");
        }
        else
        {
            // Resume countdown with adjusted remaining time
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
        if (hasMonthlyPass)
        {
            Debug.Log("Reward claimed directly with monthly pass!");
            // Placeholder for direct reward claiming logic
        }
        else
        {
            Debug.Log("Show reward ad video to claim reward!");
            // Placeholder for reward ad logic
        }

        // Stop chest animation
        StopAnimation();

        // Refresh countdown for the next round
        if (currentCountdownIndex < countdownDurations.Length - 1)
        {
            currentCountdownIndex++;
            countdownTime = countdownDurations[currentCountdownIndex];
            isCountdownActive = true;

            // Update the countdown display
            int minutes = Mathf.FloorToInt(countdownTime / 60);
            int seconds = Mathf.FloorToInt(countdownTime % 60);
            countdownText.text = $"{minutes:00}:{seconds:00}";

            // Save progress
            SaveState();
        }
        else
        {
            Debug.Log("Final reward claimed. Chest box will disappear.");
            chestBox.SetActive(false); // Hide chest box after the last reward
            PlayerPrefs.SetInt("ChestBoxVisible", 0); // Save state as hidden
        }

        // Close the reward popup
        if (rewardPopup != null)
        {
            rewardPopup.SetActive(false);
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