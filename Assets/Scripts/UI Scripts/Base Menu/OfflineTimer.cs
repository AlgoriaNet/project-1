using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OfflineTimer : MonoBehaviour
{
    public RectTransform greenBar; // Assign GreenBar (RectTransform) in Inspector
    public RectTransform greyBar; // Assign GreyBar (for width reference)
    public TextMeshProUGUI timingText; // Assign TimingText in Inspector

    private const float maxOfflineHours = 12f; // Maximum offline hours limit
    private const float maxOfflineSeconds = maxOfflineHours * 3600f; // 12 hours in seconds
    private float elapsedOfflineTime = 0f;

    private void Start()
    {
        CalculateAccumulatedOfflineTime(); // Get accumulated offline duration
        UpdateUI(); // Apply values to UI
    }

    private void CalculateAccumulatedOfflineTime()
    {
        if (PlayerPrefs.HasKey("LastLogOffTime"))
        {
            string lastLogOffString = PlayerPrefs.GetString("LastLogOffTime");
            DateTime lastLogOffTime = DateTime.Parse(lastLogOffString);
            TimeSpan offlineDuration = DateTime.Now - lastLogOffTime;
            float offlineSeconds = (float)offlineDuration.TotalSeconds;

            // Retrieve previous accumulated time
            float previousOfflineTime = PlayerPrefs.GetFloat("AccumulatedOfflineTime", 0f);

            // Add new offline time to accumulated time
            elapsedOfflineTime = Mathf.Min(previousOfflineTime + offlineSeconds, maxOfflineSeconds);
        }
        else
        {
            elapsedOfflineTime = PlayerPrefs.GetFloat("AccumulatedOfflineTime", 0f);
        }
    }

    private void UpdateUI()
    {
        // Convert elapsedOfflineTime to HH:MM:SS format
        TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedOfflineTime);
        timingText.text = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";

        // ✅ Corrected GreenBar width calculation
        float fillPercentage = elapsedOfflineTime / maxOfflineSeconds;
        float maxWidth = greyBar.rect.width;  
        float newWidth = Mathf.Clamp(fillPercentage * maxWidth, 0, maxWidth);
        greenBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
    }

    private void OnApplicationQuit()
    {
        // Save log-off time when the player exits
        PlayerPrefs.SetString("LastLogOffTime", DateTime.Now.ToString());
        PlayerPrefs.SetFloat("AccumulatedOfflineTime", elapsedOfflineTime);
        PlayerPrefs.Save();
    }

    public void ClaimRewards()
    {
        // Reset timer after claiming rewards
        elapsedOfflineTime = 0f;
        PlayerPrefs.SetFloat("AccumulatedOfflineTime", 0f); // Reset stored time
        PlayerPrefs.DeleteKey("LastLogOffTime"); // Remove last log-off time
        UpdateUI();
    }
}
