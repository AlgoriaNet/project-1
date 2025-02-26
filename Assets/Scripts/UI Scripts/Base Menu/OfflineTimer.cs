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
        CalculateOfflineTime(); // Get the last offline duration
        UpdateUI(); // Apply values to UI
    }

    private void CalculateOfflineTime()
    {
        // Retrieve last log-off time from PlayerPrefs
        if (PlayerPrefs.HasKey("LastLogOffTime"))
        {
            string lastLogOffString = PlayerPrefs.GetString("LastLogOffTime");
            DateTime lastLogOffTime = DateTime.Parse(lastLogOffString);
            TimeSpan offlineDuration = DateTime.Now - lastLogOffTime;

            // Convert to seconds and apply the max limit (12 hours)
            elapsedOfflineTime = Mathf.Min((float)offlineDuration.TotalSeconds, maxOfflineSeconds);
        }
        else
        {
            elapsedOfflineTime = 0f; // No previous log-off, start from 0
        }
    }

    private void UpdateUI()
    {
        // Convert elapsedOfflineTime to HH:MM:SS format
        TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedOfflineTime);
        timingText.text = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";

        // ✅ Corrected GreenBar width calculation
        float fillPercentage = elapsedOfflineTime / maxOfflineSeconds;
        float maxWidth = greyBar.rect.width;  // Get the full width of GreyBar
        float newWidth = Mathf.Clamp(fillPercentage * maxWidth, 0, maxWidth); // Ensure valid range
        greenBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
    }

    private void OnApplicationQuit()
    {
        // Save log-off time when the player exits
        PlayerPrefs.SetString("LastLogOffTime", DateTime.Now.ToString());
        PlayerPrefs.Save();
    }

    public void ClaimRewards()
    {
        // Reset timer after claiming
        elapsedOfflineTime = 0f;
        PlayerPrefs.DeleteKey("LastLogOffTime"); // Remove log-off time
        UpdateUI();
    }
}