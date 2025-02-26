using System;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    public static DateTime loginTime; // Store the login time
    public static string detectedLanguage; // Store the detected language
    // private bool isNewDay; // Check if it's a new day

    // public ChestManager chestManager; // Assign the ChestManager object in the Inspector

    void Start()
    {
        // Store the login time
        loginTime = DateTime.Now;
        Debug.Log($"Login Time: {loginTime}");

        // Detect and store the system language
        detectedLanguage = Application.systemLanguage.ToString();
        Debug.Log($"Detected Language: {detectedLanguage}");
    }
}