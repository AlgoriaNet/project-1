// using System;
// using UnityEngine;

// public class StartGame : MonoBehaviour
// {
//     public static DateTime loginTime; // Store the login time
//     public static string detectedLanguage; // Store the detected language
//     // private bool isNewDay; // Check if it's a new day

//     // public ChestManager chestManager; // Assign the ChestManager object in the Inspector

//     void Start()
//     {
//         // Store the login time
//         loginTime = DateTime.Now;
//         Debug.Log($"Login Time: {loginTime}");

//         // Detect and store the system language
//         detectedLanguage = Application.systemLanguage.ToString();
//         Debug.Log($"Detected Language: {detectedLanguage}");
//     }
// }


using System;
using UnityEngine;
using TMPro;
using Newtonsoft.Json;

public class StartGame : MonoBehaviour
{
    public static DateTime loginTime; // Store the login time
    public static string detectedLanguage; // Store the detected language

    // UI references for displaying all the fields
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private TextMeshProUGUI goldCoinText;
    [SerializeField] private TextMeshProUGUI diamondText;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private TextMeshProUGUI unpackCountsText;
    [SerializeField] private TextMeshProUGUI playerIdText;

    void Start()
    {
        // Store the login time
        loginTime = DateTime.Now;
        Debug.Log($"Login Time: {loginTime}");

        // Detect and store the system language
        detectedLanguage = Application.systemLanguage.ToString();
        Debug.Log($"Detected Language: {detectedLanguage}");

        // Attempt WebSocket login
        TryWebSocketLogin();

        // Retrieve and update user data
        RetrieveAndUpdateUserInfo();
    }

    private void TryWebSocketLogin()
    {
        string playerId = PlayerPrefs.GetString("player_id", null);

        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogWarning("No player_id found, skipping WebSocket login.");
            return;
        }

        var loginPayload = new
        {
            action = "login",
            player_id = playerId
        };

        string json = JsonConvert.SerializeObject(loginPayload);
        GamingSocketApi.Instance.Action("login", loginPayload);
        Debug.Log("WebSocket login sent: " + json);
    }

    // private void RetrieveAndUpdateUserInfo()
    // {
    //     // Retrieve and update username
    //     string username = PlayerPrefs.GetString("Username", "Unknown");
    //     Debug.Log($"Username: {username}");
    //     if (usernameText != null) usernameText.text = username;

    //     // Retrieve and update level
    //     int level = PlayerPrefs.GetInt("Level", 1);
    //     Debug.Log($"Level: {level}");
    //     if (levelText != null) levelText.text = level.ToString();

    //     // Retrieve and update experience (exp)
    //     int exp = PlayerPrefs.GetInt("Exp", 0);
    //     Debug.Log($"Experience: {exp}");
    //     if (expText != null) expText.text = exp.ToString();

    //     // Retrieve and update gold coin count
    //     int goldCoin = PlayerPrefs.GetInt("GoldCoin", 0);
    //     Debug.Log($"Gold Coin: {goldCoin}");
    //     if (goldCoinText != null) goldCoinText.text = goldCoin.ToString();

    //     // Retrieve and update diamond count
    //     int diamond = PlayerPrefs.GetInt("Diamond", 0);
    //     Debug.Log($"Diamond: {diamond}");
    //     if (diamondText != null) diamondText.text = diamond.ToString();

    //     // Retrieve and update stamina
    //     int stamina = PlayerPrefs.GetInt("Stamina", 0);
    //     Debug.Log($"Stamina: {stamina}");
    //     if (staminaText != null) staminaText.text = $"{stamina} / 100"; // Assuming max stamina is 100

    //     // Retrieve and update unpack count
    //     int unpackCounts = PlayerPrefs.GetInt("UnpackCounts", 0);
    //     Debug.Log($"Unpack Counts: {unpackCounts}");
    //     if (unpackCountsText != null) unpackCountsText.text = unpackCounts.ToString();

    //     // Retrieve and update player_id (UID)
    //     string playerId = PlayerPrefs.GetString("player_id", "Unknown");
    //     Debug.Log($"Player ID: {playerId}");
    //     if (playerIdText != null) playerIdText.text = playerId;
    // }

    private void RetrieveAndUpdateUserInfo()
    {
        // Retrieve and update username from PlayerPrefs
        string username = PlayerPrefs.GetString("Username", "Unknown");
        Debug.Log($"Username: {username}");
        if (usernameText != null) usernameText.text = username;

        // Retrieve and updael
        int level = model.PlayerProfile.Data?.Player?.Level ?? PlayerPrefs.GetInt("Level", 1);
        Debug.Log($"Level: {level}");
        if (levelText != null) levelText.text = level.ToString();

        // Retrieve and update experience (exp)
        int exp = model.PlayerProfile.Data?.Player?.Exp ?? PlayerPrefs.GetInt("Exp", 0);
        Debug.Log($"Experience: {exp}");
        if (expText != null) expText.text = exp.ToString();

        // Retrieve and update gold coin count
        int goldCoin = model.PlayerProfile.Data?.Player?.GoldCoin ?? PlayerPrefs.GetInt("GoldCoin", 0);
        Debug.Log($"Gold Coin: {goldCoin}");
        if (goldCoinText != null) goldCoinText.text = goldCoin.ToString();

        // Retrieve and update diamond count
        int diamond = model.PlayerProfile.Data?.Player?.Diamond ?? PlayerPrefs.GetInt("Diamond", 0);
        Debug.Log($"Diamond: {diamond}");
        if (diamondText != null) diamondText.text = diamond.ToString();

        // Retrieve and update stamina
        int stamina = model.PlayerProfile.Data?.Player?.Stamina ?? PlayerPrefs.GetInt("Stamina", 0);
        Debug.Log($"Stamina: {stamina}");
        if (staminaText != null) staminaText.text = $"{stamina} / 100";

        // Retrieve and update player_id from PlayerPrefs
        string playerId = PlayerPrefs.GetString("player_id", "Unknown");
        Debug.Log($"Player ID: {playerId}");
        if (playerIdText != null) playerIdText.text = playerId;
    }
}