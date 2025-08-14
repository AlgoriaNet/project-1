using System.Collections.Generic;
using battle;
using model;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocket;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Space(10), Tooltip("��ӵ���GameObject")]
    public GameObject AddMoveWap;

    [Tooltip("���߶��������ñ�˳��")] public List<GameObject> prop = new();

    [Header("UI")] public GameObject pauseButton;
    public GameObject speedUpButton;
    public TMP_Text battleTimeText;
    public TMP_Text hpText;
    public RectTransform hpBar;
    public TMP_Text levelText;
    public Image experienceBar;
    
    public GameObject End;
    public GameOverUI gameOverUI; // Direct reference - assign in Inspector
    public PauseUI pauseUI; // Direct reference - assign in Inspector
    private SkillLevelUpController _skillLevelUpController = new();

    [Tooltip("从后端获取数据, 并初始化")] public BattleState State { get; set; }
    public Rampart Rampart;

    [Tooltip("游戏进度管理， 不用设置")] public float BattleTime { get; private set; }
    public bool IsSuspend { get; private set; }
    public float BattleSpeed { get; private set; } = 1;

    public BattleWebSocketApi battleApi { get; private set; }
    
    // Store victory status for ad rewards API call
    private bool lastBattleVictory;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
        battleApi = BattleWebSocketApi.Instance;
    }
    
    public void ResetBattle()
    {
        Debug.Log("[BattleManager] Resetting battle for new game");
        
        // Reset battle time
        BattleTime = 0f;
        
        // Reset suspend state
        IsSuspend = false;
        
        // Reset battle speed to normal (X1)
        BattleSpeed = 1f;
        
        // Reset time scale
        Time.timeScale = 1f;
        
        // Reset UI button states
        if (speedUpButton != null)
        {
            var text = speedUpButton.GetComponentInChildren<TMPro.TMP_Text>();
            if (text != null)
            {
                text.text = "X1";
                Debug.Log("[BattleManager] Speed button reset to X1");
            }
        }
        
        // Ensure pause button shows correct state (unpaused)
        if (pauseButton != null)
        {
            var pauseText = pauseButton.GetComponentInChildren<TMPro.TMP_Text>();
            if (pauseText != null)
            {
                pauseText.text = "Pause"; // or whatever the default pause text should be
            }
        }
        
        // Reset State HP and experience
        if (State != null)
        {
            State.Hp = State.MaxHp;
            State.HpRate = 1.0f;
            State.Experience = 0;
            State.BattleLevel = 1;
            
            // Update UI
            if (hpText != null) hpText.text = State.Hp.ToString();
            if (hpBar != null) hpBar.localScale = new Vector3(State.HpRate, 1, 1);
            if (levelText != null) levelText.text = State.LevelTxt();
            if (experienceBar != null) experienceBar.fillAmount = State.ExperienceRate;
        }
        
        // Reset battle time display
        if (battleTimeText != null)
        {
            battleTimeText.text = "0.00";
            Debug.Log("[BattleManager] Battle time display reset to 0.00");
        }
        
        // Ensure level up popup is hidden
        if (AddMoveWap != null)
        {
            AddMoveWap.SetActive(false);
            Debug.Log("[BattleManager] Level up popup hidden");
        }
        
        // Ensure game over UI is hidden
        if (End != null)
        {
            End.SetActive(false);
        }
        if (gameOverUI != null)
        {
            gameOverUI.gameObject.SetActive(false);
        }
        
        // Ensure pause UI is hidden
        if (pauseUI != null)
        {
            pauseUI.HidePauseMenu();
        }
        
        // Reset fence destruction manager
        if (FenceDestructionManager.Instance != null)
        {
            FenceDestructionManager.Instance.ResetFence();
        }
        
        // Reactivate monster spawning
        if (MonsterInsManager.Instant != null)
        {
            MonsterInsManager.Instant.gameObject.SetActive(true);
            MonsterInsManager.Instant.ResetState();
            Debug.Log("[BattleManager] MonsterInsManager reactivated and state reset");
        }
        else
        {
            Debug.LogError("[BattleManager] MonsterInsManager.Instant is null!");
        }
        
        // Reactivate props
        foreach (var item in prop) 
        {
            if (item != null) item.SetActive(true);
        }
        
        // Clear and reset BattleGridManager monsters list
        if (BattleGridManager.Instance != null && BattleGridManager.Instance.monsters != null)
        {
            BattleGridManager.Instance.monsters.Clear();
            Debug.Log("[BattleManager] BattleGridManager monsters list cleared");
        }
        
        // Find and reactivate hero components
        PlayermaskManager playermaskManager = FindObjectOfType<PlayermaskManager>();
        if (playermaskManager != null)
        {
            playermaskManager.gameObject.SetActive(true);
            Debug.Log("[BattleManager] Hero reactivated");
        }
        else
        {
            Debug.LogError("[BattleManager] PlayermaskManager not found!");
        }
        
        // Reset hero manager state
        HeroManager heroManager = FindObjectOfType<HeroManager>();
        if (heroManager != null)
        {
            heroManager.ResetHeroState();
            Debug.Log("[BattleManager] HeroManager state reset");
        }
        else
        {
            Debug.LogError("[BattleManager] HeroManager not found!");
        }
        
        Debug.Log("[BattleManager] Battle reset complete");
    }

    private void Start()
    {
        Rampart = new Rampart
        {
            Hp = 2000,
        };
        
        // Setup finish line trigger for monster attacks
        // SetupFinishLine(); // Disabled - Rampart now has proper trigger collider
        State = new BattleState
        {
            Hp = 200,
            MaxHp = 200,
            HpRate = 1.0f,  // Initialize HP bar to full
            UpgradeRequiredExperience = new List<int>(),
            MaxBattleLevel = 20,
            BattleLevel = 1,
        };
        for (var i = 0; i < State.MaxBattleLevel; i++)
        {
            State.UpgradeRequiredExperience.Add(100);
        }
        UpdateExperience(0);
        
        // Setup button listeners
        // Pause button should be connected directly to PauseUI.ShowPauseMenu in inspector
        // if (pauseButton != null)
        //     pauseButton.GetComponent<Button>().onClick.AddListener(TogglePause);
        if (speedUpButton != null)
            speedUpButton.GetComponent<Button>().onClick.AddListener(ToggleSpeedUp);
        
        battleApi.Action("battle", new { data = "battle data" }, SetStatFromServer);
    }
    
    private void SetupFinishLine()
    {
        Debug.Log("[BattleManager] Setting up Finish Line trigger...");
        
        // First, try to create/ensure Finish tag exists
        try
        {
            // Check if finish line already exists by name instead of tag (since tag might not work)
            GameObject existingFinishLine = GameObject.Find("FinishLine");
            if (existingFinishLine != null)
            {
                Debug.Log("[BattleManager] FinishLine GameObject already exists, destroying and recreating...");
                DestroyImmediate(existingFinishLine);
            }
            
            // Create the finish line trigger GameObject
            GameObject finishLine = new GameObject("FinishLine");
            finishLine.transform.position = new Vector3(0f, -18f, 0f); // Position at rampart level
            
            // Try to set tag, but handle gracefully if it fails
            try 
            {
                finishLine.tag = "Finish";
                Debug.Log("[BattleManager] Successfully assigned 'Finish' tag");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BattleManager] Failed to assign 'Finish' tag: {e.Message}");
                Debug.LogError("[BattleManager] 'Finish' tag does not exist in Unity Tags! Please add it manually in Project Settings > Tags and Layers");
                
                // Use a different approach - use name-based detection in MonsterManager
                finishLine.name = "FinishLineTrigger";
                Debug.Log("[BattleManager] Using name-based detection instead");
            }
            
            // Add a BoxCollider2D as trigger
            BoxCollider2D trigger = finishLine.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(20f, 2f); // Wide enough to cover the screen width
            
            Debug.Log($"[BattleManager] Finish Line created at {finishLine.transform.position}");
            Debug.Log("[BattleManager] Monsters should now attack the rampart when they reach this line!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[BattleManager] Failed to setup finish line: {e.Message}");
        }
    }

    private void SetStatFromServer(JObject obj)
    {
        // Handle stamina consumption response
        if (obj["player"] != null && obj["player"]["stamina"] != null)
        {
            int newStamina = obj["player"]["stamina"].Value<int>();
            PlayerProfile.Data.UpdateStamina(newStamina);
            
            if (obj["stamina_consumed"] != null)
            {
                int consumed = obj["stamina_consumed"].Value<int>();
                Debug.Log($"[BattleManager] Battle started - Server consumed {consumed} stamina. New stamina: {newStamina}");
            }
        }
        
        SetSidekicks(obj["sidekicks"]);
        SetLevelUpEffects(obj["levelUpEffects"]);
    }

    private void SetSidekicks(JToken sidekicksArray)
    {
        BattleGridManager.Instance.Sidekicks.Clear();
        var sidekickList = sidekicksArray.ToObject<List<Sidekick>>();
        Debug.Log("sidekickList.Count: " + sidekickList.Count);
        foreach (var sidekick in sidekickList)
        {
            if (sidekick.Skill != null)
            {
                Debug.Log("Sidekick Skill name: " + sidekick.Skill.Name);
                Debug.Log("Sidekick Skill target type: " + sidekick.Skill.SkillTargetType);
            }
            else
            {
                Debug.LogWarning($"Sidekick {sidekick?.Name ?? "Unknown"} has null Skill!");
            }
            BattleGridManager.Instance.Sidekicks.Add(sidekick);
        }
    }
    
    private void SetLevelUpEffects(JToken levelUpEffectsArray)
    {
        _skillLevelUpController.Effects.Clear();
        
        // If no level up effects from server, create default ones
        if (levelUpEffectsArray == null || !levelUpEffectsArray.HasValues)
        {
            Debug.Log("No levelUpEffects from server, creating default effects");
            CreateDefaultLevelUpEffects();
        }
        else
        {
            var levelUpEffects = levelUpEffectsArray.ToObject<List<SkillLevelUpEffect>>();
            foreach (var levelUpEffect in levelUpEffects)
            {
                _skillLevelUpController.Effects.Add(levelUpEffect);
            }
            Debug.Log($"Loaded {_skillLevelUpController.Effects.Count} level up effects from server");
        }
    }
    
    private void CreateDefaultLevelUpEffects()
    {
        // Get all available sidekick skills
        foreach (var sidekick in BattleGridManager.Instance.Sidekicks)
        {
            string skillName = sidekick.Skill.Name;
            
            // Add default upgrade options for each skill
            _skillLevelUpController.Effects.Add(new SkillLevelUpEffect
            {
                Id = $"{skillName}_damage",
                SkillName = skillName,
                EffectName = "增加伤害",
                Description = $"增加{skillName}的伤害",
                Weight = 10,
                MaxCount = 5,
                Effects = new Dictionary<string, string> { { "ExtraDamageGain", "0.2" } }
            });
            
            _skillLevelUpController.Effects.Add(new SkillLevelUpEffect
            {
                Id = $"{skillName}_cooldown",
                SkillName = skillName,
                EffectName = "减少冷却",
                Description = $"减少{skillName}的冷却时间",
                Weight = 8,
                MaxCount = 3,
                Effects = new Dictionary<string, string> { { "ReduceCd", "0.5" } }
            });
        }
        Debug.Log($"Created {_skillLevelUpController.Effects.Count} default level up effects");
    }

    void Update()
    {
        if (IsSuspend) return;
        UpdateBattleTime();
        
        // TEST: Press T to test HP reduction manually
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[BattleManager] TESTING: Manual HP reduction triggered!");
            ReduceHp(10);
        }
    }


    public void GameOver(bool isWin)
    {
        Debug.Log($"[BattleManager] Game Over called - Win: {isWin}");
        
        // Stop the game completely
        Time.timeScale = 0; // Pause the game
        IsSuspend = true; // Stop battle updates
        
        // Disable monster spawning
        if (MonsterInsManager.Instant != null) 
            MonsterInsManager.Instant.gameObject.SetActive(false);
        
        // Clean up battle objects (but preserve hero)
        var sidekicks = FindObjectsOfType<SidekickManager>();
        var skills = FindObjectsOfType<SkillWrapperManager>();
        
        // Clear monsters
        if (BattleGridManager.Instance != null && BattleGridManager.Instance.monsters != null)
        {
            foreach (var t in BattleGridManager.Instance.monsters) 
            {
                if (t != null) Destroy(t.gameObject);
            }
            BattleGridManager.Instance.monsters.Clear();
        }
        
        // Disable props (don't destroy them)
        foreach (var item in prop) 
        {
            if (item != null) item.SetActive(false);
        }
        
        // Destroy sidekicks and skills (these can be recreated)
        foreach (var sidekick in sidekicks) 
        {
            if (sidekick != null) Destroy(sidekick.gameObject);
        }
        foreach (var skill in skills) 
        {
            if (skill != null) Destroy(skill.gameObject);
        }
        
        // DON'T destroy hero - it should persist between battles
        
        Debug.Log("[BattleManager] Battle stopped and cleaned up");
        
        // Show game over UI immediately (original behavior)
        ShowGameOverImmediately(isWin);
        
        // Call battle_complete API to get rewards (this will update the UI with rewards)
        CallBattleCompleteAPI(isWin);
    }
    
    private void CallBattleCompleteAPI(bool isWin)
    {
        Debug.Log($"[BattleManager] Calling battle_complete API for Rewards A - Victory: {isWin}");
        
        // Store victory status for potential ad rewards call
        lastBattleVictory = isWin;
        
        var battleData = new 
        {
            victory = isWin
        };
        
        battleApi.Action("battle_complete", battleData, HandleFirstRewards, HandleBattleRewardsError);
    }
    
    /// <summary>
    /// Call battle_complete API again after ad is watched successfully
    /// </summary>
    public void CallAdRewardsAPI()
    {
        Debug.Log($"[BattleManager] Calling battle_complete API for Rewards B (Ad rewards) - Victory: {lastBattleVictory}");
        
        var battleData = new 
        {
            victory = lastBattleVictory
        };
        
        battleApi.Action("battle_complete", battleData, HandleAdRewards, HandleAdRewardsError);
    }
    
    private void HandleFirstRewards(JObject response)
    {
        Debug.Log($"[BattleManager] First rewards (A) received: {response}");
        
        if (response == null)
        {
            Debug.LogError("[BattleManager] Received null response from battle_complete API");
            return;
        }
        
        // The API response format is: { "victory": bool, "rewards": {...}, "updated_player": {...} }
        if (response["rewards"] != null)
        {
            // Store rewards data for display
            var rewards = response["rewards"];
            var updatedPlayer = response["updated_player"];
            
            Debug.Log($"[BattleManager] Processing rewards: {rewards}");
            
            // Update player profile with new data
            if (updatedPlayer != null)
            {
                var player = updatedPlayer.ToObject<Player>();
                PlayerProfile.Data.SetPlayer(player);
                Debug.Log("[BattleManager] Player profile updated with first rewards");
            }
            
            // Display first rewards in UI
            DisplayRewardsInUI(rewards, false); // false = not from ads
        }
        else
        {
            Debug.LogError($"[BattleManager] Response missing 'rewards' field: {response}");
        }
    }
    
    private void HandleAdRewards(JObject response)
    {
        Debug.Log($"[BattleManager] Ad rewards (B) received: {response}");
        
        if (response != null && response["rewards"] != null)
        {
            // Store rewards data for display
            var rewards = response["rewards"];
            var updatedPlayer = response["updated_player"];
            
            // Update player profile with new data
            if (updatedPlayer != null)
            {
                var player = updatedPlayer.ToObject<Player>();
                PlayerProfile.Data.SetPlayer(player);
                Debug.Log("[BattleManager] Player profile updated with ad rewards");
            }
            
            // Display ad rewards in UI (refresh and add B rewards)
            DisplayRewardsInUI(rewards, true); // true = from ads
        }
        else
        {
            Debug.LogError($"[BattleManager] Ad rewards API failed with code: {response["code"]}");
        }
    }
    
    private void HandleBattleRewardsError(JObject error)
    {
        Debug.LogError($"[BattleManager] First rewards API error: {error}");
    }
    
    private void HandleAdRewardsError(JObject error)
    {
        Debug.LogError($"[BattleManager] Ad rewards API error: {error}");
        // Ad rewards failed, but user still has first rewards - no additional action needed
    }
    
    /// <summary>
    /// Display rewards in the game over UI. Called for both A and B rewards.
    /// </summary>
    private void DisplayRewardsInUI(JToken rewards, bool isFromAds)
    {
        string rewardType = isFromAds ? "B (Ad rewards)" : "A (Battle rewards)";
        Debug.Log($"[BattleManager] Displaying rewards {rewardType} in UI");
        
        if (gameOverUI != null)
        {
            var gameOverComponent = gameOverUI.GetComponent<GameOverUI>();
            if (gameOverComponent != null)
            {
                gameOverComponent.DisplayRewards(rewards, isFromAds);
                Debug.Log($"[BattleManager] Rewards {rewardType} displayed in game over UI");
            }
        }
        else
        {
            Debug.LogWarning("[BattleManager] GameOverUI reference not assigned! Cannot display rewards.");
        }
    }
    
    private void ShowGameOverImmediately(bool isWin)
    {
        // Original working behavior - show UI immediately
        Debug.Log("[BattleManager] Showing game over UI immediately");
        
        // Always activate End panel first (the original working behavior)
        if (End != null) End.SetActive(true);
        
        // Then use GameOverUI if available for win/lose page logic
        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver(isWin);
        }
        else
        {
            Debug.LogWarning("[BattleManager] GameOverUI reference not assigned! Using basic End panel.");
        }
    }

    public void TimeReset()
    {
        Time.timeScale = 1;
    }

    public void QuitBattle()
    {
        Debug.Log("[BattleManager] Quit battle called - cleaning up and returning to main menu");
        
        // Stop the game completely
        Time.timeScale = 0; 
        IsSuspend = true; 
        
        // Disable monster spawning
        if (MonsterInsManager.Instant != null) 
            MonsterInsManager.Instant.gameObject.SetActive(false);
        
        // Clean up battle objects
        var sidekicks = FindObjectsOfType<SidekickManager>();
        var skills = FindObjectsOfType<SkillWrapperManager>();
        
        // Clear monsters
        if (BattleGridManager.Instance != null && BattleGridManager.Instance.monsters != null)
        {
            foreach (var t in BattleGridManager.Instance.monsters) 
            {
                if (t != null) Destroy(t.gameObject);
            }
            BattleGridManager.Instance.monsters.Clear();
        }
        
        // Disable props
        foreach (var item in prop) 
        {
            if (item != null) item.SetActive(false);
        }
        
        // Destroy sidekicks and skills
        foreach (var sidekick in sidekicks) 
        {
            if (sidekick != null) Destroy(sidekick.gameObject);
        }
        foreach (var skill in skills) 
        {
            if (skill != null) Destroy(skill.gameObject);
        }
        
        Debug.Log("[BattleManager] Battle cleanup complete - returning to main menu");
        
        // Return to main canvas (same as GameOverUI close logic)
        GameObject mainCanvas = GameObject.Find("Main Canvas");
        if (mainCanvas != null)
        {
            mainCanvas.SetActive(true);
            
            // Enable the Canvas component specifically
            Canvas canvasComponent = mainCanvas.GetComponent<Canvas>();
            if (canvasComponent != null)
            {
                canvasComponent.enabled = true;
                Debug.Log("[BattleManager] Main Canvas component enabled");
            }
            else
            {
                Debug.LogError("[BattleManager] Canvas component not found on Main Canvas!");
            }
        }
        else
        {
            Debug.LogError("[BattleManager] Main Canvas not found!");
        }
        
        // Disable battle object
        GameObject battleObject = GameObject.Find("Battle");
        if (battleObject != null) 
        {
            battleObject.SetActive(false);
            Debug.Log("[BattleManager] Battle disabled");
        }
        
        // Resume time scale for main menu
        Time.timeScale = 1f;
    }

    public void GameRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UpdateBattleTime()
    {
        BattleTime += Time.deltaTime;
        battleTimeText.text = $"{BattleTime:F2}";
    }

    public void UpdateExperience(int experience)
    {
        var isLevelUp = State.AddExperience(experience);
        levelText.text = State.LevelTxt();
        experienceBar.fillAmount = State.ExperienceRate;
        if (isLevelUp) LevelUp();
    }

    public void ReduceHp(int hp)
    {
        Debug.Log($"[BattleManager] ReduceHp called with {hp} damage. Current HP: {State.Hp}");
        var isDead = State.ReduceHp(hp);
        Debug.Log($"[BattleManager] After damage: HP={State.Hp}, HpRate={State.HpRate}, isDead={isDead}");
        if (hpText != null) hpText.text = State.Hp.ToString();
        if (hpBar != null) hpBar.localScale = new Vector3(State.HpRate, 1, 1);
        if (isDead) 
        {
            Debug.Log("[BattleManager] Player is dead! Calling GameOver(false)");
            GameOver(false);
        }
    }

    private void LevelUp()
    {
        Debug.Log("LevelUp");
        var generateOptionCount = _skillLevelUpController.GenerateOptionCount();
        Debug.Log("generateOptionCount: " + generateOptionCount);
        if (generateOptionCount == 0) return;
        var options = _skillLevelUpController.GenerateOption();
        AddMoveWap.gameObject.SetActive(true);
        Time.timeScale = 0;
        var graders = FindObjectsOfType<UIGrader>();
        for (var i = graders.Length - 1; i >= 0; i--)
        {
            if (options.Count <= i) continue;
            graders[i].SetEffect(options[i]);
        }
    }

    public void ChooseSkillEffect(SkillLevelUpEffect skillLevelUpEffect)
    {
        var graders = FindObjectsOfType<UIGrader>();
        for (var i = graders.Length - 1; i >= 0; i--) graders[i].SetEffect(null);
        
        _skillLevelUpController.ApplyEffect(skillLevelUpEffect);
        Time.timeScale = 1;
        AddMoveWap.gameObject.SetActive(false);
    }

    public void TogglePause()
    {
        // Show pause confirmation UI instead of directly toggling
        if (pauseUI != null)
        {
            pauseUI.ShowPauseMenu();
        }
        else
        {
            // Fallback to old behavior if PauseUI not assigned
            Debug.LogWarning("[BattleManager] PauseUI not assigned, using fallback pause");
            IsSuspend = !IsSuspend;
            Time.timeScale = IsSuspend ? 0 : BattleSpeed;
        }
    }

    /// <summary>
    /// Public method to pause the battle (for PauseUI)
    /// </summary>
    public void PauseBattle()
    {
        IsSuspend = true;
        Time.timeScale = 0f;
        Debug.Log("[BattleManager] Battle paused");
    }

    /// <summary>
    /// Public method to resume the battle (for PauseUI)
    /// </summary>
    public void ResumeBattle()
    {
        IsSuspend = false;
        Time.timeScale = BattleSpeed;
        Debug.Log("[BattleManager] Battle resumed");
    }

    public void ToggleSpeedUp()
    {
        BattleSpeed = BattleSpeed == 1 ? 2 : 1;
        if (!IsSuspend) Time.timeScale = BattleSpeed;
        // Update button text
        if (speedUpButton != null)
        {
            var text = speedUpButton.GetComponentInChildren<TMP_Text>();
            if (text != null)
                text.text = BattleSpeed == 1 ? "X1" : "X2";
        }
    }
}