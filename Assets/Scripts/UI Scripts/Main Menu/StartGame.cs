using System;
using UnityEngine;
using model;

public class StartGame : MonoBehaviour
{
    public static DateTime firstLoginTime; // Store the first login time in lifetime
    public static DateTime currentLoginTime; // Store today's login time
    public static string detectedLanguage; // Store the detected language
    public mailPopupController mailPopupController; // Assign in Inspector

    private const string FirstLoginDateKey = "FirstLoginDate"; // PlayerPrefs key for first login
    private const string DetectedLanguageKey = "DetectedLanguage"; // PlayerPrefs key for language

    private const string PREF_MONTHLY_CARD_EXPIRY = "MonthlyCardExpiry";
    private const string PREF_WEEKLY_CARD_EXPIRY = "WeeklyCardExpiry";

    void Start()
    {
        // Load or set the first login date
        string savedFirstLogin = PlayerPrefs.GetString(FirstLoginDateKey, string.Empty);
        if (string.IsNullOrEmpty(savedFirstLogin))
        {
            firstLoginTime = DateTime.Now;
            PlayerPrefs.SetString(FirstLoginDateKey, firstLoginTime.ToString("o"));
            PlayerPrefs.Save();
        }
        else
        {
            firstLoginTime = DateTime.Parse(savedFirstLogin);
        }

        // Set today's login time
        currentLoginTime = DateTime.Now;
        Debug.Log($"First Login Time: {firstLoginTime}");
        Debug.Log($"Today's Login Time: {currentLoginTime}");

        // Load or set the detected language
        string savedLanguage = PlayerPrefs.GetString(DetectedLanguageKey, string.Empty);
        if (string.IsNullOrEmpty(savedLanguage))
        {
            detectedLanguage = Application.systemLanguage.ToString();
            PlayerPrefs.SetString(DetectedLanguageKey, detectedLanguage);
            PlayerPrefs.Save();
        }
        else
        {
            detectedLanguage = savedLanguage;
        }
        Debug.Log($"Detected Language: {detectedLanguage}");

        // Update mail icon dot state
        if (mailPopupController != null)
        {
            mailPopupController.UpdateMailIconDot();
        }
    }

    public bool IsMonthlyCardActive()
    {
        string expiryString = PlayerProfile.Data?.Player?.MonthlyCardExpiry
                            ?? PlayerPrefs.GetString(PREF_MONTHLY_CARD_EXPIRY, "");
        return DateTime.TryParse(expiryString, out DateTime expiryDate)
            && DateTime.Now < expiryDate;
    }

    public bool IsWeeklyCardActive()
    {
        string expiryString = PlayerProfile.Data?.Player?.MonthlyCardExpiry
                            ?? PlayerPrefs.GetString(PREF_WEEKLY_CARD_EXPIRY, "");
        return DateTime.TryParse(expiryString, out DateTime expiryDate)
            && DateTime.Now < expiryDate;
    }
}