using System;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    public static DateTime firstLoginTime; // Store the first login time in lifetime
    public static DateTime currentLoginTime; // Store today's login time
    public static string detectedLanguage; // Store the detected language
    public mailPopupController mailPopupController; // Assign in Inspector

    private const string FirstLoginDateKey = "FirstLoginDate"; // PlayerPrefs key for first login
    private const string DetectedLanguageKey = "DetectedLanguage"; // PlayerPrefs key for language

    void Start()
    {
        // --- Start of Temporary Testing Code (Remove After Testing) ---
        // Force firstLoginTime to 2025/2/1 for testing Day 2 to Day 7 unlocking
        firstLoginTime = new DateTime(2025, 3, 5, 0, 0, 0); // 2025/2/1 00:00:00
        PlayerPrefs.SetString(FirstLoginDateKey, firstLoginTime.ToString("o"));
        PlayerPrefs.Save();
        Debug.Log($"[TEST] Forced First Login Time: {firstLoginTime}");
        // --- End of Temporary Testing Code (Remove After Testing) ---

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
}