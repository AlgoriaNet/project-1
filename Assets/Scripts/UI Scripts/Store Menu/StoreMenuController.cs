using UnityEngine;
using UnityEngine.UI;
using TMPro;
using model;
using System;

public class StoreMenuController : MonoBehaviour
{
    [Header("HeroChest UI References")]
    public GameObject x1heroKeyPanel;
    public GameObject x1heroDiamondPanel;
    public GameObject x10heroKeyPanel;
    public GameObject x10heroDiamondPanel;
    public GameObject heroFreeButton;
    public GameObject heroAdTV;
    public TextMeshProUGUI heroFreeButtonText;

    [Header("RareChest UI References")]
    public GameObject x1rareKeyPanel;
    public GameObject x1rareDiamondPanel;
    public GameObject x10rareKeyPanel;
    public GameObject x10rareDiamondPanel;
    public GameObject rareFreeButton;
    public GameObject rareAdTV;
    public TextMeshProUGUI rareFreeButtonText;

    [Header("EpicChest UI References")]
    public GameObject x1epicKeyPanel;
    public GameObject x1epicDiamondPanel;
    public GameObject x10epicKeyPanel;
    public GameObject x10epicDiamondPanel;     
    public GameObject epicFreeButton;    
    public GameObject epicAdTV;
    public TextMeshProUGUI epicFreeButtonText;

    [Header("Gacha Panels")]
    public GameObject x1gachaKeyPanel;
    public GameObject x1gachaDiamondPanel;
    public GameObject x10gachaKeyPanel;
    public GameObject x10gachaDiamondPanel;
    public TextMeshProUGUI x1gachaDiamondText;
    public TextMeshProUGUI x10gachaDiamondText;
    public Image x1gachaKeyImage;
    public Image x10gachaKeyImage;

    // Constants for pricing and limits - no more magic numbers!
    private const int HERO_X1_DIAMOND_COST = 300;
    private const int HERO_X10_DIAMOND_COST = 3000;
    private const int RARE_X1_DIAMOND_COST = 180;
    private const int RARE_X10_DIAMOND_COST = 1800;
    private const int EPIC_X1_DIAMOND_COST = 200;
    private const int EPIC_X10_DIAMOND_COST = 2000;

    private const int HERO_FREE_CLAIM_LIMIT = 1;
    private const int RARE_FREE_CLAIM_LIMIT = 3;
    private const int EPIC_FREE_CLAIM_LIMIT = 3;

    private const int KEY_REQUIRED_FOR_X1 = 1;
    private const int KEY_REQUIRED_FOR_X10 = 10;

    // PlayerPrefs keys - centralized for easier maintenance
    private const string PREF_LAST_FREE_CLAIM_DATE = "LastFreeClaimDate";
    private const string PREF_HERO_FREE_CLAIM_COUNT = "HeroFreeClaimCount";
    private const string PREF_RARE_FREE_CLAIM_COUNT = "RareFreeClaimCount";
    private const string PREF_EPIC_FREE_CLAIM_COUNT = "EpicFreeClaimCount";
    private const string PREF_MONTHLY_CARD_EXPIRY = "MonthlyCardExpiry";

    // Sprite paths - centralized
    private const string HERO_KEY_SPRITE_PATH = "UILoading/Items/heroKey";
    private const string RARE_KEY_SPRITE_PATH = "UILoading/Items/rareKey";
    private const string EPIC_KEY_SPRITE_PATH = "UILoading/Items/epicKey";

    void Start()
    {
        // Subscribe to player data changes for automatic updates
        PlayerProfile.Data.AddListener(UpdatePlayerValues, "Player");

        // Initial panel update
        var player = PlayerProfile.Data.Player;
        if (player != null)
        {
            UpdatePanels(player);
        }

        // // TODO: Remove this test code before production deployment
        // PlayerPrefs.SetInt(PREF_HERO_FREE_CLAIM_COUNT, 0);
        // PlayerPrefs.SetInt(PREF_RARE_FREE_CLAIM_COUNT, 2);
        // PlayerPrefs.SetInt(PREF_EPIC_FREE_CLAIM_COUNT, 2);
        // PlayerPrefs.Save();

        // Update UI for claim status
        UpdateFreeClaimStatus();
    }

    private void UpdatePlayerValues(ApplicationModel model)
    {
        var player = PlayerProfile.Data.Player;
        if (player == null) return;
        UpdatePanels(player);
    }

    private void UpdatePanels(Player player)
    {
        int heroKeyCount = player.ItemsJson.TryGetValue("heroKey", out int heroKey) ? heroKey : 0;
        int rareKeyCount = player.ItemsJson.TryGetValue("rareKey", out int rareKey) ? rareKey : 0;
        int epicKeyCount = player.ItemsJson.TryGetValue("epicKey", out int epicKey) ? epicKey : 0;

        UpdateChestPanel(heroKeyCount, x1heroKeyPanel, x1heroDiamondPanel, x10heroKeyPanel, x10heroDiamondPanel, KEY_REQUIRED_FOR_X1, KEY_REQUIRED_FOR_X10);
        UpdateChestPanel(rareKeyCount, x1rareKeyPanel, x1rareDiamondPanel, x10rareKeyPanel, x10rareDiamondPanel, KEY_REQUIRED_FOR_X1, KEY_REQUIRED_FOR_X10);
        UpdateChestPanel(epicKeyCount, x1epicKeyPanel, x1epicDiamondPanel, x10epicKeyPanel, x10epicDiamondPanel, KEY_REQUIRED_FOR_X1, KEY_REQUIRED_FOR_X10);
    }

    private void UpdateChestPanel(int keyCount, GameObject x1KeyPanel, GameObject x1DiamondPanel, GameObject x10KeyPanel, GameObject x10DiamondPanel, int requiredFor1, int requiredFor10)
    {
        x1KeyPanel.SetActive(keyCount >= requiredFor1);
        x1DiamondPanel.SetActive(keyCount < requiredFor1);
        x10KeyPanel.SetActive(keyCount >= requiredFor10);
        x10DiamondPanel.SetActive(keyCount < requiredFor10);
    }

    public bool IsMonthlyCardActive()
    {
        string expiryString = PlayerProfile.Data?.Player?.MonthlyCardExpiry
                            ?? PlayerPrefs.GetString(PREF_MONTHLY_CARD_EXPIRY, "");
        return DateTime.TryParse(expiryString, out DateTime expiryDate)
            && DateTime.Now < expiryDate;
    }

    public void UpdateFreeClaimStatus()
    {
        string lastClaimDate = PlayerPrefs.GetString(PREF_LAST_FREE_CLAIM_DATE, "");
        string today = DateTime.Now.ToString("yyyy-MM-dd");

        // Daily reset: if a new day, reset all counts and re-enable buttons
        if (lastClaimDate != today)
        {
            PlayerPrefs.SetString(PREF_LAST_FREE_CLAIM_DATE, today);
            PlayerPrefs.SetInt(PREF_HERO_FREE_CLAIM_COUNT, 0);
            PlayerPrefs.SetInt(PREF_RARE_FREE_CLAIM_COUNT, 0);
            PlayerPrefs.SetInt(PREF_EPIC_FREE_CLAIM_COUNT, 0);
        }

        bool isMonthlyCardActive = IsMonthlyCardActive(); 
        Debug.Log("Monthly card active: " + isMonthlyCardActive);

        // Toggle ad icons depending on monthly card
        heroAdTV?.SetActive(!isMonthlyCardActive);
        rareAdTV?.SetActive(!isMonthlyCardActive);
        epicAdTV?.SetActive(!isMonthlyCardActive);

        // Read current claim counts
        int heroCount = PlayerPrefs.GetInt(PREF_HERO_FREE_CLAIM_COUNT, 0);
        int rareCount = PlayerPrefs.GetInt(PREF_RARE_FREE_CLAIM_COUNT, 0);
        int epicCount = PlayerPrefs.GetInt(PREF_EPIC_FREE_CLAIM_COUNT, 0);

        // Update text
        heroFreeButtonText.text = $"{Mathf.Min(heroCount, HERO_FREE_CLAIM_LIMIT)}/{HERO_FREE_CLAIM_LIMIT}";
        rareFreeButtonText.text = $"{Mathf.Min(rareCount, RARE_FREE_CLAIM_LIMIT)}/{RARE_FREE_CLAIM_LIMIT}";
        epicFreeButtonText.text = $"{Mathf.Min(epicCount, EPIC_FREE_CLAIM_LIMIT)}/{EPIC_FREE_CLAIM_LIMIT}";

        // Update button states (enable/disable + alpha) immediately
        SetClaimButtonState(heroFreeButton, heroCount < HERO_FREE_CLAIM_LIMIT);
        SetClaimButtonState(rareFreeButton, rareCount < RARE_FREE_CLAIM_LIMIT);
        SetClaimButtonState(epicFreeButton, epicCount < EPIC_FREE_CLAIM_LIMIT);
    }

    private void SetClaimButtonState(GameObject buttonObj, bool isEnabled)
    {
        var btn = buttonObj.GetComponent<Button>();
        if (btn != null)
        {
            btn.interactable = isEnabled;
        }

        var canvasGroup = buttonObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = buttonObj.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = isEnabled ? 1f : 0.7f;
    }

    // Consolidated gacha panel update method - no more code duplication!
    public void UpdateGachaPanels(string keyType, int keyCount, int x1Cost, int x10Cost, string spritePath)
    {
        // Always set diamond costs and key images
        x1gachaDiamondText.text = x1Cost.ToString();
        x10gachaDiamondText.text = x10Cost.ToString();
        
        var keySprite = Resources.Load<Sprite>(spritePath);
        x1gachaKeyImage.sprite = keySprite;
        x10gachaKeyImage.sprite = keySprite;

        // Panel visibility logic based on key count
        if (keyCount < KEY_REQUIRED_FOR_X1)
        {
            // Not enough for x1: show diamond panels only
            x1gachaKeyPanel.SetActive(false);
            x1gachaDiamondPanel.SetActive(true);
            x10gachaKeyPanel.SetActive(false);
            x10gachaDiamondPanel.SetActive(true);
        }
        else if (keyCount >= KEY_REQUIRED_FOR_X10)
        {
            // Enough for both: show key panels only
            x1gachaKeyPanel.SetActive(true);
            x1gachaDiamondPanel.SetActive(false);
            x10gachaKeyPanel.SetActive(true);
            x10gachaDiamondPanel.SetActive(false);
        }
        else
        {
            // Between 1-9 keys: x1 key available, x10 requires diamonds
            x1gachaKeyPanel.SetActive(true);
            x1gachaDiamondPanel.SetActive(false);
            x10gachaKeyPanel.SetActive(false);
            x10gachaDiamondPanel.SetActive(true);
        }

        Debug.Log($"{keyType} Count: {keyCount}");
    }

    public void gachaHeroPanelUpdate()
    {
        var player = PlayerProfile.Data.Player;
        int heroKeyCount = player.ItemsJson.TryGetValue("heroKey", out int heroKey) ? heroKey : 0;
        
        UpdateGachaPanels("heroKey", heroKeyCount, HERO_X1_DIAMOND_COST, HERO_X10_DIAMOND_COST, HERO_KEY_SPRITE_PATH);
    }

    public void gachaRarePanelUpdate()
    {
        var player = PlayerProfile.Data.Player;
        int rareKeyCount = player.ItemsJson.TryGetValue("rareKey", out int rareKey) ? rareKey : 0;
        
        UpdateGachaPanels("rareKey", rareKeyCount, RARE_X1_DIAMOND_COST, RARE_X10_DIAMOND_COST, RARE_KEY_SPRITE_PATH);
    }

    public void gachaEpicPanelUpdate()
    {
        var player = PlayerProfile.Data.Player;
        int epicKeyCount = player.ItemsJson.TryGetValue("epicKey", out int epicKey) ? epicKey : 0;
        
        UpdateGachaPanels("epicKey", epicKeyCount, EPIC_X1_DIAMOND_COST, EPIC_X10_DIAMOND_COST, EPIC_KEY_SPRITE_PATH);
    }

    private void OnDestroy()
    {
        if (PlayerProfile.Data != null)
        {
            PlayerProfile.Data.RemoveListener(UpdatePlayerValues, "Player");
        }
    }
}