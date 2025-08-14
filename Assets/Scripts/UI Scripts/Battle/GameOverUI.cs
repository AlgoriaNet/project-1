using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using TMPro;
using System.Collections.Generic;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Pages")]
    public GameObject winPage;
    public GameObject losePage;
    public GameObject battleObject;

    [Header("Shared Buttons")]
    public Button watchAdsButton;
    public Button closeButton;
    
    [Header("UI Elements")]
    public GameObject buttonGroup;
    public GameObject clickTextPanel;
    
    [Header("Rewards Display")]
    public GameObject grid; // The Grid object from End (Battle → BattleCanvas → PopUpBox → End → Grid)
    public GameObject blockPrefab; // BlockItem prefab
    public GridLayoutGroup gridLayoutGroup; // For layout control
    
    // Constants for layout (following gacha pattern)
    private const int BLOCKS_PER_ROW = 5;
    
    // Store rewards for potential refresh
    private List<JToken> allRewards = new List<JToken>();
    
    // Track last display time to prevent duplicate calls
    private float lastDisplayTime = 0f;

    
    void Start()
    {
        // Setup button listeners
        SetupButtonListeners();
        
        // Initially hide both pages but keep grid ready for rewards
        if (winPage) winPage.SetActive(false);
        if (losePage) losePage.SetActive(false);
        // Don't disable grid - it should be ready to show rewards when battle ends
    }
    
    void SetupButtonListeners()
    {
        // Shared buttons
        if (closeButton) closeButton.onClick.AddListener(() => CloseGameOver());
        if (watchAdsButton) watchAdsButton.onClick.AddListener(() => WatchAds());
    }
    
    /// <summary>
    /// Display battle rewards in the UI. Called for both A and B rewards.
    /// </summary>
    public void DisplayRewards(JToken rewards, bool isFromAds)
    {
        string rewardType = isFromAds ? "B (from ads)" : "A (battle)";
        Debug.Log($"[GameOverUI] DisplayRewards called with {rewardType}");
        
        // SIMPLE: Just show the rewards we received, no complex logic
        allRewards.Clear(); // Always clear first
        allRewards.Add(rewards); // Add only the current rewards
        
        Debug.Log($"[GameOverUI] Showing {allRewards.Count} reward set(s)");
        RefreshRewardsDisplay();
    }
    
    /// <summary>
    /// Clear and refresh the entire rewards display with all collected rewards
    /// </summary>
    private void RefreshRewardsDisplay()
    {
        Debug.Log($"[GameOverUI] RefreshRewardsDisplay called. Processing {allRewards.Count} reward sets");
        
        if (grid == null || blockPrefab == null)
        {
            Debug.LogWarning("[GameOverUI] Grid or BlockPrefab not assigned!");
            return;
        }
        
        // Show grid
        grid.SetActive(true);
        
        // Clear existing blocks
        ClearGrid();
        
        // Display all rewards in order
        int blockIndex = 0;
        for (int i = 0; i < allRewards.Count; i++)
        {
            Debug.Log($"[GameOverUI] Processing reward set {i + 1}/{allRewards.Count}");
            blockIndex = DisplayRewardSet(allRewards[i], blockIndex);
        }
        
        // Adjust grid layout
        AdjustGridLayout(blockIndex);
    }
    
    /// <summary>
    /// Clear all existing reward blocks
    /// </summary>
    private void ClearGrid()
    {
        foreach (Transform child in grid.transform)
        {
            Destroy(child.gameObject);
        }
    }
    
    /// <summary>
    /// Display one set of rewards (either A or B)
    /// </summary>
    private int DisplayRewardSet(JToken rewards, int startIndex)
    {
        int currentIndex = startIndex;
        
        // Display fixed rewards first
        var fixedRewards = rewards["fixed"];
        if (fixedRewards != null)
        {
            // EXP
            if (fixedRewards["exp"] != null && fixedRewards["exp"].Value<int>() > 0)
            {
                CreateRewardBlock("EXP", "UILoading/Items/EXP", fixedRewards["exp"].Value<int>(), currentIndex++);
            }
            
            // Gold
            if (fixedRewards["gold_coin"] != null && fixedRewards["gold_coin"].Value<int>() > 0)
            {
                CreateRewardBlock("Gold", "UILoading/Items/Gold", fixedRewards["gold_coin"].Value<int>(), currentIndex++);
            }
            
            // Gun Scroll
            if (fixedRewards["gunScroll"] != null && fixedRewards["gunScroll"].Value<int>() > 0)
            {
                CreateRewardBlock("Gun Scroll", "UILoading/Items/gunScroll", fixedRewards["gunScroll"].Value<int>(), currentIndex++);
            }
            
            // Equipment Scroll
            if (fixedRewards["equipScroll"] != null && fixedRewards["equipScroll"].Value<int>() > 0)
            {
                CreateRewardBlock("Equipment Scroll", "UILoading/Items/equipScroll", fixedRewards["equipScroll"].Value<int>(), currentIndex++);
            }
        }
        
        // Display skillbooks
        var skillbooks = rewards["skillbooks"];
        if (skillbooks != null)
        {
            foreach (var skillbook in skillbooks)
            {
                string skillbookName = skillbook["name"]?.Value<string>();
                int quantity = skillbook["quantity"]?.Value<int>() ?? 1;
                
                if (!string.IsNullOrEmpty(skillbookName))
                {
                    string imagePath = $"UILoading/CharacterImages/Skillbook/{skillbookName}";
                    CreateRewardBlock(skillbookName, imagePath, quantity, currentIndex++);
                }
            }
        }
        
        // Display equipment
        var equipment = rewards["equipment"];
        if (equipment != null)
        {
            foreach (var equip in equipment)
            {
                string equipmentName = equip["name"]?.Value<string>();
                
                if (!string.IsNullOrEmpty(equipmentName))
                {
                    string imagePath = $"UILoading/Equipment/{equipmentName}";
                    CreateRewardBlock(equipmentName, imagePath, 1, currentIndex++);
                }
            }
        }
        
        // Display gemstones
        var gemstones = rewards["gemstones"];
        if (gemstones != null)
        {
            foreach (var gemstone in gemstones)
            {
                int level = gemstone["level"]?.Value<int>() ?? 1;
                string part = gemstone["part"]?.Value<string>() ?? "Unknown";
                
                string gemImagePath = $"UILoading/Gem/Stone/Gem_{level:D2}";
                string partImagePath = $"UILoading/Gem/Part/{part}";
                
                CreateGemstoneBlock($"Gem Level {level}", gemImagePath, partImagePath, 1, currentIndex++);
            }
        }
        
        return currentIndex;
    }
    
    /// <summary>
    /// Create a reward block for regular items (non-gemstones)
    /// </summary>
    private void CreateRewardBlock(string itemName, string imagePath, int quantity, int blockIndex)
    {
        GameObject newBlock = Instantiate(blockPrefab, grid.transform);
        if (newBlock == null) return;
        
        newBlock.name = $"Block_{blockIndex}_{itemName}";
        
        // Load and set image
        Sprite sprite = Resources.Load<Sprite>(imagePath);
        Image blockImage = newBlock.transform.Find("Image")?.GetComponent<Image>();
        if (blockImage != null && sprite != null)
        {
            blockImage.sprite = sprite;
            blockImage.color = Color.white;
        }
        else
        {
            Debug.LogWarning($"[GameOverUI] Could not load sprite: {imagePath}");
        }
        
        // Hide Part component for non-gemstones
        Image partImage = newBlock.transform.Find("Part")?.GetComponent<Image>();
        if (partImage != null)
        {
            partImage.gameObject.SetActive(false);
        }
        
        // Set quantity text
        TMP_Text qntyText = newBlock.transform.Find("Qnty")?.GetComponent<TMP_Text>();
        if (qntyText != null)
        {
            qntyText.text = quantity.ToString();
            qntyText.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// Create a reward block for gemstones (with both gem and part images)
    /// </summary>
    private void CreateGemstoneBlock(string itemName, string gemImagePath, string partImagePath, int quantity, int blockIndex)
    {
        GameObject newBlock = Instantiate(blockPrefab, grid.transform);
        if (newBlock == null) return;
        
        newBlock.name = $"Block_{blockIndex}_{itemName}";
        
        // Load and set gem image
        Sprite gemSprite = Resources.Load<Sprite>(gemImagePath);
        Image blockImage = newBlock.transform.Find("Image")?.GetComponent<Image>();
        if (blockImage != null && gemSprite != null)
        {
            blockImage.sprite = gemSprite;
            blockImage.color = Color.white;
        }
        else
        {
            Debug.LogWarning($"[GameOverUI] Could not load gem sprite: {gemImagePath}");
        }
        
        // Load and set part image
        Sprite partSprite = Resources.Load<Sprite>(partImagePath);
        Image partImage = newBlock.transform.Find("Part")?.GetComponent<Image>();
        if (partImage != null && partSprite != null)
        {
            partImage.sprite = partSprite;
            partImage.color = Color.white;
            partImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[GameOverUI] Could not load part sprite: {partImagePath}");
            if (partImage != null)
            {
                partImage.gameObject.SetActive(false);
            }
        }
        
        // Set quantity text
        TMP_Text qntyText = newBlock.transform.Find("Qnty")?.GetComponent<TMP_Text>();
        if (qntyText != null)
        {
            qntyText.text = quantity.ToString();
            qntyText.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// Adjust grid layout following gacha pattern
    /// </summary>
    private void AdjustGridLayout(int totalBlocks)
    {
        if (gridLayoutGroup == null)
        {
            Debug.LogWarning("[GameOverUI] GridLayoutGroup not assigned!");
            return;
        }
        
        RectTransform gridRect = grid.GetComponent<RectTransform>();
        float panelWidth = gridRect.rect.width;
        
        // Calculate dimensions following gacha pattern
        float blockWidth = panelWidth / (BLOCKS_PER_ROW + 1);
        float leftPadding = blockWidth * 0.25f;
        float rightPadding = blockWidth * 0.25f;
        float spacingX = blockWidth * 0.125f;
        float spacingY = blockWidth * 0.125f;
        
        // Configure grid layout
        gridLayoutGroup.cellSize = new Vector2(blockWidth, blockWidth);
        gridLayoutGroup.spacing = new Vector2(spacingX, spacingY);
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = BLOCKS_PER_ROW;
        
        // Set padding
        gridLayoutGroup.padding.left = Mathf.RoundToInt(leftPadding);
        gridLayoutGroup.padding.right = Mathf.RoundToInt(rightPadding);
        gridLayoutGroup.padding.top = Mathf.RoundToInt(spacingY);
        gridLayoutGroup.padding.bottom = Mathf.RoundToInt(spacingY);
        
        // Adjust height based on total blocks
        int totalRows = Mathf.Max(1, Mathf.CeilToInt((float)totalBlocks / BLOCKS_PER_ROW));
        float contentHeight = totalRows * (blockWidth + spacingY) - spacingY;
        gridRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
        
        Debug.Log($"[GameOverUI] Grid adjusted for {totalBlocks} blocks, {totalRows} rows, height: {contentHeight}");
    }
    
    /// <summary>
    /// Show game over screen based on win/lose result
    /// </summary>
    public void ShowGameOver(bool isWin)
    {       
        Debug.Log($"[GameOverUI] ShowGameOver called - isWin: {isWin}");
        
        // Activate the parent GameObject (End) first
        gameObject.SetActive(true);
        buttonGroup.SetActive(true);
        clickTextPanel.SetActive(false);
        

        if (isWin)
        {
            // Show win page
            if (winPage) winPage.SetActive(true);
            if (losePage) losePage.SetActive(false);
            Debug.Log("[GameOverUI] Win page activated");
        }
        else
        {
            // Show lose page
            if (losePage) losePage.SetActive(true);
            if (winPage) winPage.SetActive(false);
            Debug.Log("[GameOverUI] Lose page activated");
        }
        
        Debug.Log("[GameOverUI] ShowGameOver completed - waiting for real API response");
    }
    
    /// <summary>
    /// TEST: Coroutine to test rewards after delay
    /// </summary>
    private System.Collections.IEnumerator TestRewardsAfterDelay()
    {
        Debug.Log("[GameOverUI] TEST: Waiting 2 seconds then testing rewards...");
        yield return new WaitForSecondsRealtime(2f);
        
        Debug.Log("[GameOverUI] TEST: 2 seconds passed, now calling TestDummyRewards");
        TestDummyRewards();
    }
    
    /// <summary>
    /// TEST METHOD: Create dummy rewards data to test if block creation works
    /// </summary>
    private void TestDummyRewards()
    {
        Debug.Log("[GameOverUI] TEST: Creating dummy rewards to test block creation");
        
        // Create dummy JSON rewards data with REAL sprites that exist
        string dummyRewardsJson = @"{
            ""fixed"": {
                ""exp"": 100,
                ""gold_coin"": 500
            },
            ""gemstones"": [
                {""level"": 1, ""part"": ""Head""},
                {""level"": 2, ""part"": ""Body""}
            ]
        }";
        
        try
        {
            var dummyRewards = JToken.Parse(dummyRewardsJson);
            Debug.Log($"[GameOverUI] TEST: Parsed dummy rewards: {dummyRewards}");
            
            // Call DisplayRewards with dummy data
            DisplayRewards(dummyRewards, false);
            
            Debug.Log("[GameOverUI] TEST: DisplayRewards called with dummy data");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameOverUI] TEST: Failed to create dummy rewards: {e.Message}");
        }
    }
    
    /// <summary>
    /// TEMPORARY DEBUG: Show test reward blocks
    /// </summary>
    /*private void ShowTestRewards()
    {
        Debug.Log("[GameOverUI] DEBUG: Showing test rewards");
        
        if (grid == null)
        {
            Debug.LogError("[GameOverUI] DEBUG: Grid reference is null! Please assign it in Inspector.");
            Debug.LogError("[GameOverUI] DEBUG: Looking for Grid under End GameObject...");
            
            // Try to find Grid automatically
            Transform endTransform = transform;
            Transform gridTransform = endTransform.Find("Grid");
            if (gridTransform != null)
            {
                grid = gridTransform.gameObject;
                Debug.Log($"[GameOverUI] DEBUG: Found Grid automatically: {grid.name}");
            }
            else
            {
                Debug.LogError("[GameOverUI] DEBUG: Could not find Grid under End GameObject!");
                return;
            }
        }
        
        if (blockPrefab == null)
        {
            Debug.LogError("[GameOverUI] DEBUG: BlockPrefab reference is null! Please assign it in Inspector.");
            
            // Try to find BlockItem prefab 
            Debug.LogError("[GameOverUI] DEBUG: Cannot auto-load prefab from Assets/Perfabes/UI/ - requires Inspector assignment");
            return;
        }
        
        Debug.Log($"[GameOverUI] DEBUG: Grid = {grid.name}, BlockPrefab = {blockPrefab.name}");
        
        // Show grid
        grid.SetActive(true);
        Debug.Log("[GameOverUI] DEBUG: Grid activated");
        
        // Clear existing blocks
        ClearGrid();
        Debug.Log("[GameOverUI] DEBUG: Grid cleared");
        
        // Create 3 test blocks - even if sprites don't load, blocks should appear
        for (int i = 0; i < 3; i++)
        {
            GameObject newBlock = Instantiate(blockPrefab, grid.transform);
            if (newBlock != null)
            {
                newBlock.name = $"DEBUG_Block_{i}";
                Debug.Log($"[GameOverUI] DEBUG: Created test block {i}: {newBlock.name}");
                
                // Set quantity text regardless of sprite loading
                TMP_Text qntyText = newBlock.transform.Find("Qnty")?.GetComponent<TMP_Text>();
                if (qntyText != null)
                {
                    qntyText.text = (i + 1).ToString();
                    qntyText.gameObject.SetActive(true);
                    Debug.Log($"[GameOverUI] DEBUG: Set quantity text for block {i}");
                }
                else
                {
                    Debug.LogWarning($"[GameOverUI] DEBUG: Could not find Qnty component in block {i}");
                }
            }
            else
            {
                Debug.LogError($"[GameOverUI] DEBUG: Failed to instantiate block {i}");
            }
        }
        
        // Try to get or assign gridLayoutGroup
        if (gridLayoutGroup == null)
        {
            gridLayoutGroup = grid.GetComponent<GridLayoutGroup>();
            if (gridLayoutGroup != null)
            {
                Debug.Log("[GameOverUI] DEBUG: Found GridLayoutGroup component on Grid");
            }
            else
            {
                Debug.LogError("[GameOverUI] DEBUG: No GridLayoutGroup found on Grid!");
            }
        }
        
        // Adjust layout
        AdjustGridLayout(3);
        Debug.Log("[GameOverUI] DEBUG: Grid layout adjusted for 3 blocks");
    }*/
    
    /// <summary>
    /// (1) Close game over UI and return to main game
    /// </summary>
    public void CloseGameOver()
    {       
        // Clear rewards data and hide grid
        allRewards.Clear();
        if (grid) grid.SetActive(false);
        ClearGrid();
        
        // Hide both pages
        if (winPage) winPage.SetActive(false);
        if (losePage) losePage.SetActive(false);
        
        // Hide End GameObject (game over panel)
        gameObject.SetActive(false);
        
        // Resume game time
        Time.timeScale = 1f;
        
        // CRITICAL: Enable Main Canvas component that was disabled when battle started
        GameObject mainCanvas = GameObject.Find("Main Canvas");
        if (mainCanvas != null)
        {
            mainCanvas.SetActive(true);
            
            // Enable the Canvas component specifically
            Canvas canvasComponent = mainCanvas.GetComponent<Canvas>();
            if (canvasComponent != null)
            {
                canvasComponent.enabled = true;
            }
        }

        // Disable battle object
        if (battleObject) 
        {
            battleObject.SetActive(false);
        }
    }
    
    
    /// <summary>
    /// (2) Watch ads for rewards - integrated with GoogleMobileAds
    /// </summary>
    public void WatchAds()
    {
        Debug.Log("[GameOverUI] Watch ads button clicked");
        
        // Use the same ad pattern as gacha system
        if (GoogleMobileAdsScript.This.CheckRewardedAd())
        {
            GoogleMobileAdsScript.This.ShowRewardedAd("battle_reward", () =>
            {
                Debug.Log("[GameOverUI] Rewarded ad watched successfully - calling API for rewards B!");
                
                // Call BattleManager to get ad rewards (Rewards B)
                if (BattleManager.Instance != null)
                {
                    BattleManager.Instance.CallAdRewardsAPI();
                }
                
                // Hide button group and show click text after ad
                if (buttonGroup) buttonGroup.SetActive(false);
                if (clickTextPanel) clickTextPanel.SetActive(true);
            });
        }
        else
        {
            GoogleMobileAdsScript.This.ShowInterstitialAd(() =>
            {
                Debug.Log("[GameOverUI] Interstitial ad watched successfully - calling API for rewards B!");
                
                // Call BattleManager to get ad rewards (Rewards B)
                if (BattleManager.Instance != null)
                {
                    BattleManager.Instance.CallAdRewardsAPI();
                }
                
                // Hide button group and show click text after ad
                if (buttonGroup) buttonGroup.SetActive(false);
                if (clickTextPanel) clickTextPanel.SetActive(true);
            });
        }
    }
    
    
    /// <summary>
    /// (3) Load equipment get page - placeholder implementation  
    /// </summary>
    public void LoadEquipmentGet()
    {
        Debug.Log("[GameOverUI] PLACEHOLDER: Loading equipment rewards page...");
        
        // TODO: Implement equipment rewards UI
        // For now, just close the game over screen
        CloseGameOver();
    }
    
    /// <summary>
    /// Restart the battle (only available on lose page)
    /// </summary>
    public void RestartBattle()
    {      
        // Hide game over UI
        if (winPage) winPage.SetActive(false);
        if (losePage) losePage.SetActive(false);
        
        // Find and trigger battle starter
        BattleStarter battleStarter = FindObjectOfType<BattleStarter>();
        if (battleStarter != null)
        {
            battleStarter.StartBattle();
        }
    }
    
    /// <summary>
    /// Simulate ad watching with coroutine
    /// </summary>
    private System.Collections.IEnumerator SimulateAdWatching()
    {       
        // Simulate 3 second ad
        yield return new WaitForSecondsRealtime(3f);
        
        Debug.Log("[GameOverUI] Ad finished! Giving reward...");
        
        // TODO: Give actual rewards (coins, gems, etc.)
        Debug.Log("[GameOverUI] PLACEHOLDER: +100 coins, +10 gems awarded!");
        
        // Could stay on current page or close automatically
        // For now, keep the game over screen open so user can choose next action
    }
    
    /// <summary>
    /// Public method for other scripts to trigger game over
    /// </summary>
    public static void TriggerGameOver(bool isWin)
    {
        GameOverUI gameOverUI = FindObjectOfType<GameOverUI>();
        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver(isWin);
        }
    }
}