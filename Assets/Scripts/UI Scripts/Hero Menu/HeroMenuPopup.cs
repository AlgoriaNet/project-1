using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using model;
using Newtonsoft.Json.Linq;
using WebSocket;


public class HeroMenuPopup : MonoBehaviour
{
    public GameObject upDownMenu; 
    public GameObject heroStep2; 
    public GameObject heroPopup; // Parent panel (semi-transparent background)
    public GameObject skinPage;
    public GameObject gunPage;
    public GameObject gemPage;
    public Button skinButton;
    public Button gunButton;
    public Button gemButton;
    
    public Button closeGunButton;
    public Button closeGemButton;
    public Button closeSkinButton; // Added close button for the skin page

    public Transform heroStep2ContentPanel;  // The original content panel that holds the BlockItems

    public GameObject skillItemPrefab; // Assign SkillItem prefab in Inspector
    public Transform skillContentPanel;  // Assign "Content" inside ScrollView
    private GameObject activeDescription = null;
    
    // Level and EXP display components
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private RectTransform expBar;
    private bool _isRefreshingLevelInfo = false; 


    // Mock List for test only, Need to delete later. 
    private List<(string skillId, string skillName, string skillImagePath, bool isActive)> skillImagePaths = new List<(string, string, string, bool)>
    {
        ("Skill 1", "Bullet Mutation", "01_Bullet Mutation", false),
        ("Skill 2", "Bullet +1", "02_Bullet +1", false),
        ("Skill 3", "Bullet +1", "02_Bullet +1", false),
        ("Skill 4", "Crit Damage", "03_Crit Damage", false),
        ("Skill 5", "Crit Damage", "03_Crit Damage", false),
        ("Skill 6", "Crit Damage", "03_Crit Damage", true),
        ("Skill 7", "Crit Rate", "04_Crit Rate", true),
        ("Skill 8", "Gun Damage", "05_Gun Damage", true),
        ("Skill 9", "Gun Damage", "05_Gun Damage", true),
        ("Skill 10", "Crit Rate", "04_Crit Rate", true)
    };


    void Start()
    {
        // Ensure all popups are initially hidden
        heroPopup.SetActive(false);
        skinPage.SetActive(false);
        gunPage.SetActive(false);
        gemPage.SetActive(false);

        // Assign button listeners
        skinButton.onClick.AddListener(OpenSkinPage);
        gunButton.onClick.AddListener(OpenGunPage);
        gemButton.onClick.AddListener(OpenGemPage);

        closeGunButton.onClick.AddListener(CloseGunPage);
        closeGemButton.onClick.AddListener(CloseGemPage);
        closeSkinButton.onClick.AddListener(CloseSkinPage); // Assign listener for skin page close button
        
        // Subscribe to player data updates for level/EXP display
        PlayerProfile.Data.AddListener(UpdateHeroMenuInfo, "Player");
    }

    private void OpenSkinPage()
    {
        heroPopup.SetActive(true);
        skinPage.SetActive(true);
        heroStep2.SetActive(false);
        upDownMenu.SetActive(false);
        Debug.Log("Skin panel opened.");
    }

    // 🔹 Call LoadSkillItems when GunPage opens
    private void OpenGunPage()
    {
        heroPopup.SetActive(true);
        gunPage.SetActive(true);
        heroStep2.SetActive(false);
        upDownMenu.SetActive(false);
        LoadSkillItems(); // ✅ Load skills here
        Debug.Log("Gun panel opened.");
    }

    private void OpenGemPage()
    {
        heroPopup.SetActive(true);
        gemPage.SetActive(true);
        heroStep2.SetActive(false);
        Debug.Log("Gem panel opened.");
    }


    public void CloseSkinPage()
    {
        skinPage.SetActive(false);
        heroStep2.SetActive(true);
        Debug.Log("Skin panel closed.");
        CheckAndCloseHeroPopup();
    }

    public void CloseGunPage()
    {
        gunPage.SetActive(false);
        heroStep2.SetActive(true);
        Debug.Log("Gun panel closed.");
        CheckAndCloseHeroPopup();
    }

    public void CloseGemPage()
    {
        gemPage.SetActive(false);
        heroStep2.SetActive(true);
        Debug.Log("Gem panel closed.");
        CheckAndCloseHeroPopup();
    }


    private void CheckAndCloseHeroPopup()
    {
        // Close the parent panel if no pages are active
        if (!skinPage.activeSelf && !gunPage.activeSelf && !gemPage.activeSelf)
        {
            heroPopup.SetActive(false);
            heroStep2.SetActive(true);
            upDownMenu.SetActive(true);
            Debug.Log("HeroPopup closed.");
        }
    }
    
    private void LoadSkillItems()
    {
        // Clear existing skill items only
        foreach (Transform child in skillContentPanel)
        {
            Destroy(child.gameObject);
        }

        // Instantiate skill items dynamically
        foreach (var skill in skillImagePaths)
        {
            string skillId = skill.skillId;
            string skillName = skill.skillName;
            string skillImagePath = skill.skillImagePath;
            bool isActive = skill.isActive;

            // Instantiate new skill item
            GameObject newSkillItem = Instantiate(skillItemPrefab, skillContentPanel);
            newSkillItem.name = skillId;

            // ✅ Assign Skill Name
            var skillNameText = newSkillItem.transform.Find("Description/Block/SkillName").GetComponent<TextMeshProUGUI>();
            if (skillNameText != null) skillNameText.text = skillName;

            // ✅ Load & Assign Skill Image (For both SkillImage positions)
            string path = $"UILoading/SkillItem/{skillImagePath}";
            Sprite skillSprite = Resources.Load<Sprite>(path);

            if (skillSprite != null)
            {
                // Set SkillImage (Main)
                var skillImageMain = newSkillItem.transform.Find("SkillImage").GetComponent<Image>();
                if (skillImageMain != null) skillImageMain.sprite = skillSprite;

                // Set SkillImage (Inside Description Block)
                var skillImageBlock = newSkillItem.transform.Find("Description/Block/SkillImage").GetComponent<Image>();
                if (skillImageBlock != null) skillImageBlock.sprite = skillSprite;
            }
            else
            {
                Debug.LogWarning($"❌ Image not found at path: {path}");
            }

            // ✅ Add Button Click Event to Show Description
            Button skillButton = newSkillItem.GetComponent<Button>();
            if (skillButton != null)
            {
                skillButton.onClick.AddListener(() => ToggleDescription(newSkillItem));
            }

            // ✅ Ensure description starts hidden
            Transform description = newSkillItem.transform.Find("Description");
            if (description != null) description.gameObject.SetActive(false);

            // ✅ Hide button inside the block if the skill is not active
            var blockButton = newSkillItem.transform.Find("Description/Block/Button").GetComponent<Button>();
            var shade = newSkillItem.transform.Find("Shade").GetComponent<Image>();

            if (!isActive)
            {
                if (blockButton != null)
                {
                    blockButton.gameObject.SetActive(false); // Disable Button if the skill is inactive
                }

                if (shade != null)
                {
                    shade.gameObject.SetActive(true); // Enable Shade if the skill is inactive
                }
            }
        }
    }

    // ✅ Function to toggle descriptions
    private void ToggleDescription(GameObject selectedSkill)
    {
        Transform selectedDesc = selectedSkill.transform.Find("Description");

        if (selectedDesc == null) return; // If no description, return

        // Disable the previous active description
        if (activeDescription != null && activeDescription != selectedDesc.gameObject)
        {
            activeDescription.SetActive(false);
        }

        // Toggle the current description
        bool isActive = selectedDesc.gameObject.activeSelf;
        selectedDesc.gameObject.SetActive(!isActive);

        // Update activeDescription reference
        activeDescription = selectedDesc.gameObject.activeSelf ? selectedDesc.gameObject : null;
    }
    
    /// <summary>
    /// Update Hero Menu level and EXP display when player data changes
    /// </summary>
    private void UpdateHeroMenuInfo(ApplicationModel model)
    {
        var player = PlayerProfile.Data.Player;
        
        // Don't try to refresh level info if player data is not loaded yet
        if (player == null)
        {
            Debug.Log("[HeroMenuPopup] Player data not loaded yet, skipping level refresh");
            return;
        }
        
        // Update basic level display first
        if (levelText != null) levelText.text = $"Level {player.Level}";
        
        // Only refresh detailed level info if we're not already in the middle of a level refresh
        if (!_isRefreshingLevelInfo)
        {
            RefreshHeroMenuLevelInfo();
        }
    }
    
    /// <summary>
    /// Call get_level_info API to refresh Hero Menu level and EXP display with server calculations
    /// </summary>
    public void RefreshHeroMenuLevelInfo()
    {
        if (_isRefreshingLevelInfo)
        {
            Debug.Log("[HeroMenuPopup] Already refreshing level info, skipping duplicate request");
            return;
        }
        
        Debug.Log("[HeroMenuPopup] Calling get_level_info API to refresh Hero Menu level data");
        _isRefreshingLevelInfo = true;
        
        PlayerLevelWebSocketApi.Instance.Action("get_level_info", new { }, HandleHeroMenuLevelInfo, HandleHeroMenuLevelError);
    }
    
    /// <summary>
    /// Handle successful level info response for Hero Menu
    /// </summary>
    private void HandleHeroMenuLevelInfo(JObject response)
    {
        _isRefreshingLevelInfo = false;
        Debug.Log($"[HeroMenuPopup] Level info received: {response}");
        
        var levelInfo = response["level_info"];
        
        if (levelInfo != null)
        {
            UpdateHeroMenuLevelDisplay(levelInfo);
            Debug.Log("[HeroMenuPopup] Hero Menu level display updated successfully");
        }
        
        Debug.Log("[HeroMenuPopup] Hero Menu level API response processed successfully");
    }
    
    /// <summary>
    /// Handle level info API error for Hero Menu
    /// </summary>
    private void HandleHeroMenuLevelError(JObject error)
    {
        _isRefreshingLevelInfo = false;
        Debug.LogError($"[HeroMenuPopup] Level info API error: {error}");
    }
    
    /// <summary>
    /// Update Hero Menu level and EXP bar display with server data
    /// </summary>
    private void UpdateHeroMenuLevelDisplay(JToken levelInfo)
    {
        var currentLevel = levelInfo["current_level"]?.Value<int>() ?? 1;
        var currentExp = levelInfo["current_exp"]?.Value<int>() ?? 0;
        var isMaxLevel = levelInfo["is_max_level"]?.Value<bool>() ?? false;
        var totalExpForNextLevel = levelInfo["total_exp_for_next_level"]?.Value<int>() ?? 0;
        
        // Calculate progress within current level range
        var totalExpForCurrentLevel = levelInfo["total_exp_for_current_level"]?.Value<int>() ?? 0;
        float progressPercentage = 0f;
        if (!isMaxLevel && totalExpForNextLevel > totalExpForCurrentLevel)
        {
            float expInCurrentLevel = currentExp - totalExpForCurrentLevel;
            float expNeededForNextLevel = totalExpForNextLevel - totalExpForCurrentLevel;
            progressPercentage = (expInCurrentLevel / expNeededForNextLevel) * 100f;
        }
        
        // Update level text
        if (levelText != null) 
        {
            levelText.text = isMaxLevel ? "Level MAX" : $"Level {currentLevel}";
        }
        
        // Update EXP bar (exactly like BattleManager HP bar)
        if (expBar != null)
        {
            if (isMaxLevel)
            {
                expBar.localScale = new Vector3(1.0f, 1, 1); // Full bar for max level
            }
            else
            {
                float expRate = progressPercentage / 100f; // Simple calculation
                expBar.localScale = new Vector3(expRate, 1, 1); // Scale X-axis like HP bar
            }
        }
        
        Debug.Log($"[HeroMenuPopup] Hero Menu level display updated: Level {currentLevel}, EXP {currentExp}/{totalExpForNextLevel} ({progressPercentage:F1}%)");
    }
    
    /// <summary>
    /// Clean up listeners when destroyed
    /// </summary>
    private void OnDestroy()
    {
        PlayerProfile.Data.RemoveListener(UpdateHeroMenuInfo, "Player");
    }
}
