using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using model;
using PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.Allies_Menu;

public class AlliesGridSetup : MonoBehaviour
{
    public GridLayoutGroup grid; // Assign the GridLayoutGroup
    public RectTransform contentPanel; // Assign the ContentPanel RectTransform
    public GameObject allyItemPrefab; // Assign the AllyItem prefab
    public int totalAllies = 20;
    private List<string> unlockedIcons;
    public GameObject step1Panel;
    private float blockWidth;
    private int currentAllyIndex;
    private List<(string index, string name)> unlockedAllies; // Store unlocked allies
    private Dictionary<string, int> starLevels;
    public Dictionary<string, GameObject> allyItemsDict = new Dictionary<string, GameObject>();
    private float nameTextWidth;
    private float nameTextHeight;
    private float starGroupWidth;
    private float starGroupHeight;
    private float skillIconWidth;
    public GameObject step2Panel;
    public GameObject lowerGroup;
    public GameObject Board;
    public GameObject Pack;
    public Image step2AllyImage;
    public Transform step2StarGroup; // Assign in the Inspector
    public Button leftArrowButton;
    public Button rightArrowButton;
    
    // Reference to UpgradePanelManager for loading ally upgrade levels
    public UpgradePanelManager upgradePanelManager;
    
    // Reference to AlliesEquipments for loading ally equipment display
    public AlliesEquipments alliesEquipments;

    // Step 2 LevelUp/StarUp UI components
    public Image itemImage; // Single image that switches between shard/skillbook
    public TextMeshProUGUI upButtonText; // Button text that changes between "LevelUp" and "StarUp"
    
    // Store current ally info for the toggle methods
    private string currentAllyName;
    private string currentItemType; // Store the current item type for utilize flow
    private string currentUtilizeAllyIndex; // Store the current ally index for utilize flow

    void Start()
    {
        // Subscribe to player data changes to refresh allies when new ones are summoned
        PlayerProfile.Data.AddListener(OnPlayerDataChanged, "Player");
        PlayerProfile.Data.AddListener(OnPlayerDataChanged, "Sidekicks");
        
        // Clear existing ally items first for fresh star data
        ClearExistingAllyItems();
        
        InitializeAlliesGrid();
    }
    
    private void OnDestroy()
    {
        // Clean up listeners
        PlayerProfile.Data.RemoveListener(OnPlayerDataChanged, "Player");
        PlayerProfile.Data.RemoveListener(OnPlayerDataChanged, "Sidekicks");
    }
    
    private void OnPlayerDataChanged(ApplicationModel model)
    {
        // Skip if this GameObject is inactive (prevents coroutine error)
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        
        // Only reload grid if Allies Menu (step1Panel) is actually active
        if (step1Panel != null && !step1Panel.activeInHierarchy)
        {
            return;
        }
        
        // Force full reload to ensure star displays show current data
        if (PlayerProfile.Data?.Player != null && grid != null)
        {
            ClearExistingAllyItems();
            InitializeAlliesGrid();
        }
        else
        {
            InitializeAlliesGrid();
        }
    }
    
    /// <summary>
    /// Force reload grid with fresh backend data (called when explicitly opening Allies Menu)
    /// </summary>
    public void ForceReloadWithFreshData()
    {
        ClearExistingAllyItems();
        InitializeAlliesGrid();
    }
    
    public void InitializeAlliesGrid()
    {
        
        // Don't initialize if player data is not available yet (except for force reload)
        if (PlayerProfile.Data?.Player == null)
        {
        }
        
        
        // Get the content panel width
        float panelWidth = contentPanel.rect.width;

        // Calculate block width
        int constraintCount = grid.constraintCount;

        blockWidth = panelWidth / (constraintCount * 13f / 12f + 0.25f); // 🔹 Store blockWidth
        nameTextWidth = blockWidth * 0.6f;
        nameTextHeight = blockWidth * 0.12f;
        starGroupWidth = blockWidth * 0.5f;
        starGroupHeight = blockWidth * 0.1f;
        skillIconWidth = blockWidth * 0.2f;

        // Calculate padding and spacing
        float leftPadding = blockWidth / 6;
        float rightPadding = leftPadding;
        float spacingX = leftPadding / 2;
        float spacingY = spacingX;
        float topPadding = leftPadding * 1.5f;
        float bottomPadding = topPadding;

        // Apply calculated padding and spacing
        grid.padding.left = Mathf.RoundToInt(leftPadding);
        grid.padding.right = Mathf.RoundToInt(rightPadding);
        grid.padding.top = Mathf.RoundToInt(topPadding);
        grid.padding.bottom = Mathf.RoundToInt(bottomPadding);
        grid.spacing = new Vector2(spacingX, spacingY);

        // Apply calculated block size
        grid.cellSize = new Vector2(blockWidth, blockWidth * 1.342f); // Adjust height proportionally

        LoadAllyItems();
    }

    public void ClearExistingAllyItems()
    {
        // Clear existing ally items to prevent duplicates when refreshing
        int destroyedCount = 0;
        
        // SAFE DESTROY: Only destroy AllyItem prefabs, not other UI elements
        List<Transform> allyItemsToDestroy = new List<Transform>();
        foreach (Transform child in contentPanel)
        {
            if (child.name.StartsWith("AllyItem_"))
            {
                allyItemsToDestroy.Add(child);
            }
        }
        
        foreach (Transform child in allyItemsToDestroy)
        {
            Destroy(child.gameObject);
            destroyedCount++;
        }
        
        
        // Clear the dictionary as well
        allyItemsDict.Clear();
    }

    private IEnumerator LoadCharacterDataAndContinue()
    {
        yield return CharacterService.LoadCharacters(
            onSuccess: (characters) => {
                Debug.Log($"[AlliesGridSetup] Successfully loaded {characters.Length} characters");
                ContinueLoadAllyItems(); // Continue with the loading process
            },
            onError: (error) => {
                Debug.LogError($"[AlliesGridSetup] Failed to load characters: {error}");
                ContinueLoadAllyItems(); // Continue anyway with fallback behavior
            }
        );
    }

    private void LoadAllyItems()
    {
        // Use CharacterService to get character names
        string[] characterNames = CharacterService.GetCharacterNames();
        
        // If character data is not loaded yet, load it first
        if (characterNames.Length == 0)
        {
            StartCoroutine(LoadCharacterDataAndContinue());
            return;
        }

        ContinueLoadAllyItems();
    }

    private void ContinueLoadAllyItems()
    {
        // Use CharacterService to get character names
        string[] characterNames = CharacterService.GetCharacterNames();

        // UPDATED: Get unlocked allies from backend data instead of hardcoded values
        unlockedIcons = GetUnlockedAllyIndices(characterNames);

        // Star levels should come from backend sidekick data - no hardcoded values
        starLevels = new Dictionary<string, int>();
        // TODO: Populate star levels from backend sidekick data when available

        // Prepare lists for sorting
        List<(string index, string name, bool isUnlocked)> sortedAllies = new List<(string, string, bool)>();

        for (int i = 0; i < totalAllies; i++)
        {
            string index = (i + 1).ToString("D2");

            // Skip "00" as it doesn't correspond to any AllyItem
            if (index == "00") continue;

            bool isUnlocked = unlockedIcons.Contains(index);
            sortedAllies.Add((index, characterNames[i], isUnlocked));
        }

        // Sort by unlock status first, then by index in ascending order
        sortedAllies.Sort((a, b) =>
        {
            int unlockComparison = b.isUnlocked.CompareTo(a.isUnlocked); // Unlocked first
            return unlockComparison == 0 ? a.index.CompareTo(b.index) : unlockComparison;
        });

        // ***** NEW: Populate unlockedAllies with only unlocked entries *****
        unlockedAllies = new List<(string index, string name)>();
        foreach (var ally in sortedAllies)
        {
            if (ally.isUnlocked)
            {
                unlockedAllies.Add((ally.index, ally.name));
            }
        }

        // Instantiate ally items
        ClearExistingAllyItems(); // Clear old items before creating new ones
        foreach (var ally in sortedAllies)
        {
            GameObject newAlly = Instantiate(allyItemPrefab, contentPanel, false);

            // ***** RENAME THE GAMEOBJECT FOR RefreshAllAllyStarDisplays() TO WORK *****
            newAlly.name = $"AllyItem_{ally.index}_{ally.name}";

            // Store reference
            allyItemsDict[ally.index] = newAlly;

            string imagePath = $"UILoading/CharacterImages/CardDisplay/C_{ally.index}_{ally.name}";

            // Assign Ally Image
            Image allyImage = newAlly.transform.Find("AllyImage").GetComponent<Image>();
            Image skillIcon = newAlly.transform.Find("SkillIcon").GetComponent<Image>();
            Sprite allySprite = Resources.Load<Sprite>(imagePath);

            if (allySprite != null)
            {
                allyImage.sprite = allySprite;
            }
            else
            {
                Debug.LogWarning($"❌ Image not found at path: {imagePath}");
            }

            // Assign Ally Name
            TextMeshProUGUI nameText = newAlly.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            nameText.text = ally.name;

            // Assign Gray Cover for Locked/Unlocked Logic
            allyImage.color = ally.isUnlocked ? Color.white : Color.gray;
            skillIcon.color = ally.isUnlocked ? Color.white : Color.gray;
            nameText.color = ally.isUnlocked ? Color.white : Color.gray;

            // Find StarGroup inside newAlly
            Transform starGroup = newAlly.transform.Find("StarGroup");

            // Get star level from PlayerProfile sidekick data
            string allyId = $"{ally.index}_{ally.name}";
            int starLevel = GetSidekickStarLevel(allyId);

            for (int i = 1; i <= 5; i++)
            {
                Transform starYellow = starGroup.Find($"Star{i}/yellow"); // Direct lookup

                if (starYellow != null)
                {
                    bool shouldActivate = i <= starLevel;
                    starYellow.gameObject.SetActive(shouldActivate);
                }
                else
                {
                    Debug.LogWarning($"❌ 'yellow' NOT FOUND inside Star{i} for {allyId}!");
                }
            }

            // Force Canvas update for star changes
            Canvas canvas = newAlly.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = false;
                canvas.enabled = true;
            }

            // Retain Gray Tint for Locked Allies
            foreach (Image star in starGroup.GetComponentsInChildren<Image>())
            {
                star.color = ally.isUnlocked ? Color.white : Color.gray;
            }

            // 🔹 NEW: Adjust UI sizes dynamically
            AdjustAllyItemSize(newAlly); // 🔹 Now using stored blockWidth

            // Keep existing setup function call
            SetupAllyButton(newAlly, ally.index, ally.name, ally.isUnlocked);
        }
    }

    private void AdjustAllyItemSize(GameObject allyItem)
    {
        // Adjust NameText width (60% of block width)
        TextMeshProUGUI nameText = allyItem.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        RectTransform nameTextRect = nameText.GetComponent<RectTransform>();
        nameTextRect.sizeDelta = new Vector2(nameTextWidth, nameTextHeight);

        // Set NameText position (Y = starGroupHeight + 5)
        nameTextRect.anchoredPosition = new Vector2(nameTextRect.anchoredPosition.x, starGroupHeight + 13);

        // Adjust StarGroup width and height (50% width, 10% height)
        Transform starGroup = allyItem.transform.Find("StarGroup");
        RectTransform starGroupRect = starGroup.GetComponent<RectTransform>();
        starGroupRect.sizeDelta = new Vector2(starGroupWidth, starGroupHeight);

        // Adjust GridLayoutGroup inside StarGroup
        GridLayoutGroup starGrid = starGroup.GetComponent<GridLayoutGroup>();
        if (starGrid != null)
        {
            starGrid.cellSize = new Vector2(starGroupRect.sizeDelta.y, starGroupRect.sizeDelta.y); // Square cell
        }

        // Adjust SkillIcon width (20% of block width)
        Image skillIcon = allyItem.transform.Find("SkillIcon").GetComponent<Image>();
        RectTransform skillIconRect = skillIcon.GetComponent<RectTransform>();
        skillIconRect.sizeDelta = new Vector2(skillIconWidth, skillIconRect.sizeDelta.y);
    }

    private void SetupAllyButton(GameObject allyItem, string index, string name, bool isUnlocked)
    {
        Button allyButton = allyItem.GetComponent<Button>();

        if (allyButton == null)
        {
            allyButton = allyItem.AddComponent<Button>();
        }

        allyButton.interactable = isUnlocked;

        if (isUnlocked)
        {
            allyButton.onClick.AddListener(() => OpenStep2(allyItem, index, name));
        }
    }

    private void OpenStep2(GameObject allyItem, string index, string name)
    {
        if (step2AllyImage == null)
        {
            Debug.LogError("❌ step2AllyImage is NOT assigned in Inspector!");
            return;
        }

        // TEST: Destroy Step1 grid content when opening Step2
        ClearExistingAllyItems();

        // Switch panels
        step1Panel.SetActive(false);
        step2Panel.SetActive(true);
        lowerGroup.SetActive(true);
        Board.SetActive(true);
        Pack.SetActive(false);
        
        // Reset Orange button text to default when entering Step 2
        ResetOrangeButtonText();

        // Construct and load illustration
        string illustrationPath = $"UILoading/CharacterImages/Stand_Illustration/P_{index}_{name}";
        Sprite illustrationSprite = Resources.Load<Sprite>(illustrationPath);

        if (illustrationSprite != null)
        {
            step2AllyImage.sprite = illustrationSprite;
        }
        else
        {
            Debug.LogWarning($"❌ Illustration not found at path: {illustrationPath}");        }


        // Store current ally info for LevelUp/StarUp toggle methods
        currentAllyName = name;

        // Track the current ally index for navigation
        currentAllyIndex = unlockedAllies.FindIndex(a => a.index == index);

        // Assign button events
        leftArrowButton.onClick.RemoveAllListeners();
        rightArrowButton.onClick.RemoveAllListeners();
        leftArrowButton.onClick.AddListener(() => NavigateStep2Ally(-1));
        rightArrowButton.onClick.AddListener(() => NavigateStep2Ally(1));
    
        // ***** NEW: Enable/disable arrows based on position *****
        leftArrowButton.interactable = (currentAllyIndex > 0);
        rightArrowButton.interactable = (currentAllyIndex < unlockedAllies.Count - 1);  

        // 🔹 Get StarGroup from the clicked allyItem in Step 1
        Transform step1StarGroup = allyItem.transform.Find("StarGroup");
        if (step1StarGroup == null)
        {
            Debug.LogWarning($"❌ Step 1 StarGroup not found for Ally {index} ({name})!");
            return;
        }

        // 🔹 Set Step2 stars based on actual backend star level (not mirroring from Step1)
        string allyId = $"{index}_{name}";
        int actualStarLevel = GetSidekickStarLevel(allyId);
        
        // Set complete star state based on actual star level
        for (int i = 1; i <= 5; i++)
        {
            Transform step2Star = step2StarGroup.Find($"Star{i}/yellow");
            if (step2Star != null)
            {
                bool shouldActivate = i <= actualStarLevel;
                step2Star.gameObject.SetActive(shouldActivate);
            }
            else
            {
                Debug.LogWarning($"❌ Step2 Star{i}/yellow not found!");
            }
        }

        // Delay the size adjustment to allow layout updates
        Invoke(nameof(AdjustStep2StarGroupSize), 0.05f); // Reduced from 0.1f

        // 🔹 CRITICAL FIX: Refresh the lower section to match the current ally
        // This ensures shard/skillbook items are coordinated with the ally image
        RefreshLowerSectionForCurrentAlly();
        
        // Load upgrade levels for the selected ally
        if (upgradePanelManager != null)
        {
            upgradePanelManager.LoadUpgradePanelsForAlly($"{index}_{name}");
        }
        
        // Load equipment display for the selected ally
        if (alliesEquipments != null)
        {
            alliesEquipments.InitForCurrentAlly();
        }
        
        // CRITICAL FIX: Ensure UpgradePanelManager mode is synced after navigation
        // This must happen after RefreshLowerSectionForCurrentAlly() to ensure proper mode sync
        if (upgradePanelManager != null)
        {
            // Check current button state and sync UpgradePanelManager mode
            PageButtonController pageButtonController = FindObjectOfType<PageButtonController>();
            if (pageButtonController != null)
            {
                var activeButtonIndexField = typeof(PageButtonController).GetField("activeButtonIndex", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (activeButtonIndexField != null)
                {
                    int activeButtonIndex = (int)activeButtonIndexField.GetValue(pageButtonController);
                    bool isStarUpMode = (activeButtonIndex == 1);
                    upgradePanelManager.SetMode(isStarUpMode);
                }
            }
        }
    }

    private void AdjustStep2StarGroupSize()
    {
        GridLayoutGroup step2Grid = step2StarGroup.GetComponent<GridLayoutGroup>();
        RectTransform step2StarGroupRect = step2StarGroup.GetComponent<RectTransform>();

        if (step2Grid != null && step2StarGroupRect != null)
        {
            float updatedWidth = step2StarGroupRect.rect.width;
            float starCellSize = updatedWidth * 0.2f;
            step2Grid.cellSize = new Vector2(starCellSize, starCellSize);
        }
    }

    private void NavigateStep2Ally(int direction)
    {
        int newIndex = currentAllyIndex + direction;
        if (newIndex >= 0 && newIndex < unlockedAllies.Count)
        {
            currentAllyIndex = newIndex;
            string newIndexStr = unlockedAllies[newIndex].index;
            string newName = unlockedAllies[newIndex].name;

            // 🔹 Check if we're in utilize mode (currentItemType is set)
            if (!string.IsNullOrEmpty(currentItemType))
            {
                // In utilize mode: update ally image and maintain utilize flow state
                NavigateUtilizeAlly(newIndexStr, newName);
            }
            else
            {
                // Normal navigation mode: use Step2-only logic (no Step1 dependency)
                OpenStep2Direct(newIndexStr, newName);
            }
        }
    }

    /// <summary>
    /// Open Step2 directly without needing Step1 allyItem (for navigation)
    /// </summary>
    /// <param name="index">The ally index (e.g., "04", "10")</param>
    /// <param name="name">The ally name (e.g., "Aurelia", "Cedric")</param>
    private void OpenStep2Direct(string index, string name)
    {
        if (step2AllyImage == null)
        {
            Debug.LogError("❌ step2AllyImage is NOT assigned in Inspector!");
            return;
        }


        // Load ally illustration (same as original OpenStep2)
        string illustrationPath = $"UILoading/CharacterImages/Stand_Illustration/P_{index}_{name}";
        Sprite illustrationSprite = Resources.Load<Sprite>(illustrationPath);

        if (illustrationSprite != null)
        {
            step2AllyImage.sprite = illustrationSprite;
        }
        else
        {
            Debug.LogWarning($"❌ Illustration not found at path: {illustrationPath}");
        }

        // Store current ally info for LevelUp/StarUp toggle methods
        currentAllyName = name;

        // Track the current ally index for navigation
        currentAllyIndex = unlockedAllies.FindIndex(a => a.index == index);

        // Assign button events
        leftArrowButton.onClick.RemoveAllListeners();
        rightArrowButton.onClick.RemoveAllListeners();
        leftArrowButton.onClick.AddListener(() => NavigateStep2Ally(-1));
        rightArrowButton.onClick.AddListener(() => NavigateStep2Ally(1));

        // Enable/disable arrows based on position
        leftArrowButton.interactable = (currentAllyIndex > 0);
        rightArrowButton.interactable = (currentAllyIndex < unlockedAllies.Count - 1);

        // Set Step2 stars based on actual backend star level
        string allyId = $"{index}_{name}";
        int actualStarLevel = GetSidekickStarLevel(allyId);
        
        // Set complete star state based on actual star level
        for (int i = 1; i <= 5; i++)
        {
            Transform step2Star = step2StarGroup.Find($"Star{i}/yellow");
            if (step2Star != null)
            {
                bool shouldActivate = i <= actualStarLevel;
                step2Star.gameObject.SetActive(shouldActivate);
            }
            else
            {
                Debug.LogWarning($"❌ Step2 Star{i}/yellow not found!");
            }
        }

        // Delay the size adjustment to allow layout updates
        Invoke(nameof(AdjustStep2StarGroupSize), 0.05f);

        // Refresh the lower section to match the current ally
        RefreshLowerSectionForCurrentAlly();
        
        // Load upgrade levels for the selected ally
        if (upgradePanelManager != null)
        {
            upgradePanelManager.LoadUpgradePanelsForAlly($"{index}_{name}");
        }
        
        // Load equipment display for the selected ally
        if (alliesEquipments != null)
        {
            alliesEquipments.InitForCurrentAlly();
        }
        
        // Sync UpgradePanelManager mode after navigation
        if (upgradePanelManager != null)
        {
            PageButtonController pageButtonController = FindObjectOfType<PageButtonController>();
            if (pageButtonController != null)
            {
                var activeButtonIndexField = typeof(PageButtonController).GetField("activeButtonIndex", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (activeButtonIndexField != null)
                {
                    int activeButtonIndex = (int)activeButtonIndexField.GetValue(pageButtonController);
                    bool isStarUpMode = (activeButtonIndex == 1);
                    upgradePanelManager.SetMode(isStarUpMode);
                }
            }
        }
    }

    /// <summary>
    /// Check if an ally is unlocked based on backend data.
    /// This replaces the hardcoded unlockedIcons logic.
    /// </summary>
    /// <param name="allyName">The ally name (e.g., "Nyx", "Aurelia")</param>
    /// <returns>True if the ally has been summoned/unlocked</returns>
    private bool IsAllyUnlocked(string allyName)
    {
        // Return false if player data is not loaded yet
        if (PlayerProfile.Data?.Player == null)
        {
            return false;
        }
        
        // Use the new PlayerProfile method that handles both sidekick and legacy systems
        return PlayerProfile.Data.IsAllyUnlocked(allyName);
    }

    /// <summary>
    /// Get all unlocked ally indices for backwards compatibility with existing code.
    /// This generates a list based on backend data instead of hardcoded values.
    /// </summary>
    /// <param name="characterNames">Array of character names</param>
    /// <returns>List of indices (as strings) of unlocked allies</returns>
    private List<string> GetUnlockedAllyIndices(string[] characterNames)
    {
        List<string> unlockedIndices = new List<string>();
        
        for (int i = 0; i < characterNames.Length; i++)
        {
            string index = (i + 1).ToString("D2");
            string allyName = characterNames[i];
            
            // **FIX**: Check for ally using the backend format "XX_Name"
            string backendFormat = $"{index}_{allyName}";
            
            if (IsAllyUnlocked(allyName) || IsAllyUnlocked(backendFormat))
            {
                unlockedIndices.Add(index);
            }
        }
        
        return unlockedIndices;
    }

    /// <summary>
    /// Public method to navigate to Step 2 for a specific ally from Step 1.
    /// This is the original method for normal ally navigation with full functionality.
    /// </summary>
    /// <param name="allyIndex">The ally index (e.g., "02", "10")</param>
    /// <param name="allyName">The ally name (e.g., "Gideon", "Cedric")</param>
    /// <returns>True if Step 2 was successfully set up, false otherwise</returns>
    public bool NavigateToAllyStep2(string allyIndex, string allyName)
    {
        // Find the ally item in Step 1 for full navigation functionality
        if (allyItemsDict.TryGetValue(allyIndex, out GameObject allyItem))
        {
            OpenStep2(allyItem, allyIndex, allyName);
            return true;
        }
        else
        {
            Debug.LogWarning($"❌ AllyItem_{allyIndex} not found in dictionary for navigation!");
            return false;
        }
    }

    /// <summary>
    /// Simplified method to open Step 2 for utilize actions (upgrade/upstar).
    /// Called from external scripts like OtherDetailBox when utilizing shards/skillbooks.
    /// This method focuses purely on showing the ally in Step 2 without navigation complexity.
    /// </summary>
    /// <param name="allyIndex">The ally index (e.g., "02", "10")</param>
    /// <param name="allyName">The ally name (e.g., "Gideon", "Cedric")</param>
    /// <returns>True if Step 2 was successfully set up, false otherwise</returns>
    public bool OpenUtilizeStep2(string allyIndex, string allyName)
    {
        
        // Basic validation
        if (step2AllyImage == null)
        {
            Debug.LogError("❌ step2AllyImage is NOT assigned in Inspector!");
            return false;
        }

        // IMPORTANT: Properly activate the Allies Menu button in MenuController
        MenuController menuController = FindObjectOfType<MenuController>();
        if (menuController != null)
        {
            // Find and "click" the Allies Menu button (index 0)
            if (menuController.buttons != null && menuController.buttons.Length > 0)
            {
                // Simulate clicking the Allies Menu button to ensure proper UI state
                menuController.buttons[0].onClick.Invoke();
            }
        }

        // Switch panels - same as normal Step 2
        step1Panel.SetActive(false);
        step2Panel.SetActive(true);
        lowerGroup.SetActive(true);
        Board.SetActive(true);
        Pack.SetActive(false);
        
        // Reset Orange button text to default when entering Step 2
        ResetOrangeButtonText();

        // Load ally illustration
        string illustrationPath = $"UILoading/CharacterImages/Stand_Illustration/P_{allyIndex}_{allyName}";
        Sprite illustrationSprite = Resources.Load<Sprite>(illustrationPath);

        if (illustrationSprite != null)
        {
            step2AllyImage.sprite = illustrationSprite;
        }
        else
        {
            Debug.LogWarning($"❌ Illustration not found at path: {illustrationPath}");
        }

        // Store ally info for dynamic image switching
        currentAllyName = allyName;

        // Note: Item image (shard/skillbook) will be set dynamically by LevelUpLoading() or StarUpLoading() methods
        // when the user clicks the toggle buttons

        // For utilize flow, disable navigation arrows since we're not in a navigation context
        leftArrowButton.onClick.RemoveAllListeners();
        rightArrowButton.onClick.RemoveAllListeners();
        leftArrowButton.interactable = false;
        rightArrowButton.interactable = false;

        // Set Step2 stars based on actual star level for utilize flow
        if (step2StarGroup != null)
        {
            string allyId = $"{allyIndex}_{allyName}";
            int actualStarLevel = GetSidekickStarLevel(allyId);
            
            for (int i = 1; i <= 5; i++)
            {
                Transform step2Star = step2StarGroup.Find($"Star{i}/yellow");
                if (step2Star != null)
                {
                    bool shouldActivate = i <= actualStarLevel;
                    step2Star.gameObject.SetActive(shouldActivate);
                }
            }
        }

        // Delay the size adjustment to allow layout updates
        Invoke(nameof(AdjustStep2StarGroupSize), 0.05f); // Reduced from 0.1f

        return true;
    }

    /// <summary>
    /// Simplified version that doesn't handle menu switching - assumes menu is already active.
    /// This is called after the menu has been properly activated.
    /// </summary>
    /// <param name="allyIndex">The ally index (e.g., "02", "10")</param>
    /// <param name="allyName">The ally name (e.g., "Gideon", "Cedric")</param>
    /// <param name="itemType">The item type ("shard" or "skillbook")</param>
    /// <returns>True if Step 2 was successfully set up, false otherwise</returns>
    public bool OpenUtilizeStep2Simple(string allyIndex, string allyName, string itemType)
    {
        // Basic validation
        if (step2AllyImage == null)
        {
            Debug.LogError("❌ step2AllyImage is NOT assigned in Inspector!");
            return false;
        }

        // 🔹 CRITICAL: Ensure allies grid is properly initialized before proceeding
        // This fixes the timing issue where utilize flow is called before initialization
        if (unlockedAllies == null || unlockedAllies.Count == 0)
        {
            InitializeAlliesGrid();
            
            // If still no unlocked allies after initialization, we can't proceed
            if (unlockedAllies == null || unlockedAllies.Count == 0)
            {
                return false;
            }
        }

        // Switch panels - same as normal Step 2 (no menu switching here)
        step1Panel.SetActive(false);
        step2Panel.SetActive(true);
        lowerGroup.SetActive(true);
        Board.SetActive(true);
        Pack.SetActive(false);
        
        // Reset Orange button text to default when entering Step 2
        ResetOrangeButtonText();

        // Load ally illustration
        string illustrationPath = $"UILoading/CharacterImages/Stand_Illustration/P_{allyIndex}_{allyName}";
        Sprite illustrationSprite = Resources.Load<Sprite>(illustrationPath);

        if (illustrationSprite != null)
        {
            step2AllyImage.sprite = illustrationSprite;
        }
        else
        {
            Debug.LogWarning($"❌ Illustration not found at path: {illustrationPath}");
        }

        // Store ally info for dynamic image switching
        currentAllyName = allyName;
        currentItemType = itemType; // Store item type for button mode logic
        currentUtilizeAllyIndex = allyIndex; // Store ally index for utilize flow

        // 🔹 ENABLE navigation arrows for utilize flow - users should be able to navigate between allies
        // Track the current ally index for navigation (same as normal Step 2 flow)
        currentAllyIndex = unlockedAllies.FindIndex(a => a.index == allyIndex);

        // Assign button events for navigation
        leftArrowButton.onClick.RemoveAllListeners();
        rightArrowButton.onClick.RemoveAllListeners();
        leftArrowButton.onClick.AddListener(() => NavigateStep2Ally(-1));
        rightArrowButton.onClick.AddListener(() => NavigateStep2Ally(1));

        // Enable/disable arrows based on position (same logic as normal flow)
        leftArrowButton.interactable = (currentAllyIndex > 0);
        rightArrowButton.interactable = (currentAllyIndex < unlockedAllies.Count - 1);

        // Set Step2 stars based on actual star level for utilize flow
        if (step2StarGroup != null)
        {
            string allyId = $"{allyIndex}_{allyName}";
            int actualStarLevel = GetSidekickStarLevel(allyId);
            
            for (int i = 1; i <= 5; i++)
            {
                Transform step2Star = step2StarGroup.Find($"Star{i}/yellow");
                if (step2Star != null)
                {
                    bool shouldActivate = i <= actualStarLevel;
                    step2Star.gameObject.SetActive(shouldActivate);
                }
            }
        }

        // Delay the size adjustment to allow layout updates
        Invoke(nameof(AdjustStep2StarGroupSize), 0.05f); // Reduced from 0.1f

        // 🔹 CRITICAL FIX: Initialize ally equipment context immediately (like working Step 1 → Step 2 flow)
        // This fixes the "Current ally name is empty" error by properly setting ally context
        if (alliesEquipments != null)
        {
            alliesEquipments.InitForCurrentAlly();
        }
        
        // Load upgrade levels for the selected ally immediately (like working Step 1 → Step 2 flow)
        if (upgradePanelManager != null)
        {
            upgradePanelManager.LoadUpgradePanelsForAlly($"{allyIndex}_{allyName}");
        }

        // 🔹 DELAY: Button mode setup to ensure all initialization is complete
        // This ensures coordination between the item type and the button/UI state
        Invoke(nameof(SetUtilizeModeAndRefresh), 0.08f); // Reduced from 0.12f

        return true;
    }

    /// <summary>
    /// Updates the Step 2 board content for LevelUp vs StarUp actions.
    /// Called by PageButtonController when buttons are pressed.
    /// </summary>
    /// <param name="isLevelUp">True for LevelUp mode, false for StarUp mode</param>
    public void UpdateBoardContent(bool isLevelUp)
    {
        if (isLevelUp)
        {
            LevelUpLoading();
        }
        else
        {
            StarUpLoading();
        }
    }

    /// <summary>
    /// Sets up the board for LevelUp mode - shows skillbook image and "LevelUp" text
    /// </summary>
    public void LevelUpLoading()
    {
        if (string.IsNullOrEmpty(currentAllyName))
        {
            return;
        }

        // Get current ally index from unlockedAllies (ally is guaranteed to be unlocked)
        if (unlockedAllies == null || unlockedAllies.Count == 0)
        {
            InitializeAlliesGrid();
            
            if (unlockedAllies == null || unlockedAllies.Count == 0)
            {
                return;
            }
        }
        
        var currentAlly = unlockedAllies.Find(a => a.name == currentAllyName);
        
        // If ally not found by name alone, and we're in utilize mode, try using stored index
        if (string.IsNullOrEmpty(currentAlly.index) && !string.IsNullOrEmpty(currentUtilizeAllyIndex))
        {
            // In utilize mode, we can construct the path directly without relying on unlockedAllies
            // This handles cases where the ally might not be in the unlockedAllies list yet
            
            // Load SkillBook Image using stored data
            string fallbackSkillBookPath = $"UILoading/CharacterImages/Skillbook/SKb_{currentUtilizeAllyIndex}_{currentAllyName}";
            Sprite fallbackSkillBookSprite = Resources.Load<Sprite>(fallbackSkillBookPath);

            if (fallbackSkillBookSprite != null && itemImage != null)
            {
                itemImage.sprite = fallbackSkillBookSprite;
                itemImage.color = Color.white;
            }
            else
            {
                Debug.LogWarning($"❌ SkillBook image not found at path: {fallbackSkillBookPath}");
                if (itemImage != null) itemImage.color = new Color(0, 0, 0, 0);
            }

            // Set button text to "LevelUp"
            if (upButtonText != null)
            {
                upButtonText.text = "LevelUp";
            }
            return; // Early return since we handled it with stored data
        }
        
        if (string.IsNullOrEmpty(currentAlly.index))
        {
            return;
        }

        // Load SkillBook Image
        string skillBookPath = $"UILoading/CharacterImages/Skillbook/Skb_{currentAlly.index}_{currentAllyName}";
        Sprite skillBookSprite = Resources.Load<Sprite>(skillBookPath);

        if (skillBookSprite != null && itemImage != null)
        {
            itemImage.sprite = skillBookSprite;
            itemImage.color = Color.white;
        }
        else
        {
            Debug.LogWarning($"❌ SkillBook image not found at path: {skillBookPath}");
            if (itemImage != null) itemImage.color = new Color(0, 0, 0, 0);
        }

        // Set button text to "LevelUp"
        if (upButtonText != null)
        {
            upButtonText.text = "LevelUp";
        }
        
        // Set UpgradePanelManager to LevelUp mode
        if (upgradePanelManager != null)
        {
            upgradePanelManager.SetMode(false); // false = LevelUp mode
        }
    }

    /// <summary>
    /// Sets up the board for StarUp mode - shows shard image and "StarUp" text
    /// </summary>
    public void StarUpLoading()
    {
        if (string.IsNullOrEmpty(currentAllyName))
        {
            return;
        }

        // Get current ally index from unlockedAllies (ally is guaranteed to be unlocked)
        if (unlockedAllies == null || unlockedAllies.Count == 0)
        {
            InitializeAlliesGrid();
            
            if (unlockedAllies == null || unlockedAllies.Count == 0)
            {
                return;
            }
        }
        
        var currentAlly = unlockedAllies.Find(a => a.name == currentAllyName);
        
        // If ally not found by name alone, and we're in utilize mode, try using stored index
        if (string.IsNullOrEmpty(currentAlly.index) && !string.IsNullOrEmpty(currentUtilizeAllyIndex))
        {
            // In utilize mode, we can construct the path directly without relying on unlockedAllies
            // This handles cases where the ally might not be in the unlockedAllies list yet
            
            // Load Shard Image using stored data
            string fallbackShardPath = $"UILoading/CharacterImages/Shard/{currentUtilizeAllyIndex}_{currentAllyName}";
            Sprite fallbackShardSprite = Resources.Load<Sprite>(fallbackShardPath);

            if (fallbackShardSprite != null && itemImage != null)
            {
                itemImage.sprite = fallbackShardSprite;
                itemImage.color = Color.white;
            }
            else
            {
                Debug.LogWarning($"❌ Shard image not found at path: {fallbackShardPath}");
                if (itemImage != null) itemImage.color = new Color(0, 0, 0, 0);
            }

            // Set button text to "StarUp"
            if (upButtonText != null)
            {
                upButtonText.text = "StarUp";
            }
            return; // Early return since we handled it with stored data
        }
        
        if (string.IsNullOrEmpty(currentAlly.index))
        {
            return;
        }

        // Load Shard Image
        string shardPath = $"UILoading/CharacterImages/Shard/{currentAlly.index}_{currentAllyName}";
        Sprite shardSprite = Resources.Load<Sprite>(shardPath);

        if (shardSprite != null && itemImage != null)
        {
            itemImage.sprite = shardSprite;
            itemImage.color = Color.white;
        }
        else
        {
            Debug.LogWarning($"❌ Shard image not found at path: {shardPath}");
            if (itemImage != null) itemImage.color = new Color(0, 0, 0, 0);
        }

        // Set button text to "StarUp"
        if (upButtonText != null)
        {
            upButtonText.text = "StarUp";
        }
        
        // Set UpgradePanelManager to StarUp mode
        if (upgradePanelManager != null)
        {
            upgradePanelManager.SetMode(true); // true = StarUp mode
        }
    }

    /// <summary>
    /// Refreshes the lower section (shard/skillbook) to match the current ally.
    /// This ensures coordination between ally image, items, and button text.
    /// Called after navigation or when opening Step 2.
    /// </summary>
    private void RefreshLowerSectionForCurrentAlly()
    {
        // Find the PageButtonController to determine which mode is currently active
        PageButtonController pageButtonController = FindObjectOfType<PageButtonController>();
        if (pageButtonController == null)
        {
            // Default to LevelUp mode if PageButtonController not found
            LevelUpLoading();
            return;
        }

        // Check which button is currently active and refresh accordingly
        if (pageButtonController.buttons != null && pageButtonController.buttons.Length > 0)
        {
            // Use reflection to get the private activeButtonIndex field
            var activeButtonIndexField = typeof(PageButtonController).GetField("activeButtonIndex", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (activeButtonIndexField != null)
            {
                int activeButtonIndex = (int)activeButtonIndexField.GetValue(pageButtonController);
                
                if (activeButtonIndex == 0)
                {
                    // Button 1 (LevelUp) is active - show skillbook
                    LevelUpLoading();
                }
                else if (activeButtonIndex == 1)
                {
                    // Button 2 (StarUp) is active - show shard
                    StarUpLoading();
                }
                else
                {
                    // Default to LevelUp if no clear active button
                    LevelUpLoading();
                }
            }
            else
            {
                // Fallback: Default to LevelUp mode
                LevelUpLoading();
            }
        }
        else
        {
            // Default to LevelUp mode if no buttons found
            LevelUpLoading();
        }
    }

    /// <summary>
    /// Sets the appropriate button mode and refreshes the lower section for utilize flow.
    /// Called after opening Step 2 from OtherDetailBox to ensure coordination.
    /// </summary>
    private void SetUtilizeModeAndRefresh()
    {
        // Find PageButtonController to set the appropriate button mode
        PageButtonController pageButtonController = FindObjectOfType<PageButtonController>();
        if (pageButtonController == null)
        {
            // If no PageButtonController found, default to LevelUp mode
            LevelUpLoading();
            return;
        }

        // Determine which button to activate based on item type
        int targetButtonIndex = 0; // Default to LevelUp (button 0)
        
        if (currentItemType == "shard")
        {
            targetButtonIndex = 1; // StarUp mode for shards (button 1)
        }
        else if (currentItemType == "skillbook")
        {
            targetButtonIndex = 0; // LevelUp mode for skillbooks (button 0)
        }

        // Simulate clicking the appropriate button to set the correct mode
        // This ensures the button visual state and the lower section are coordinated
        pageButtonController.ToggleButtonVisibility(targetButtonIndex);
        
        // The PageButtonController.ToggleButtonVisibility() will automatically call
        // the appropriate LevelUpLoading() or StarUpLoading() method to update the lower section
        
        // Note: upgradePanelManager.LoadUpgradePanelsForAlly() is now called immediately in OpenUtilizeStep2Simple()
    }

    /// <summary>
    /// Navigate to a different ally while maintaining utilize flow state.
    /// Updates ally image and item (shard/skillbook) while preserving utilize mode.
    /// </summary>
    /// <param name="allyIndex">The new ally index</param>
    /// <param name="allyName">The new ally name</param>
    private void NavigateUtilizeAlly(string allyIndex, string allyName)
    {
        // Update ally illustration
        string illustrationPath = $"UILoading/CharacterImages/Stand_Illustration/P_{allyIndex}_{allyName}";
        Sprite illustrationSprite = Resources.Load<Sprite>(illustrationPath);

        if (illustrationSprite != null && step2AllyImage != null)
        {
            step2AllyImage.sprite = illustrationSprite;
        }
        else
        {
            Debug.LogWarning($"❌ Illustration not found at path: {illustrationPath}");
        }

        // Update current ally info for utilize flow
        currentAllyName = allyName;
        currentUtilizeAllyIndex = allyIndex;

        // Update arrow button states
        leftArrowButton.interactable = (currentAllyIndex > 0);
        rightArrowButton.interactable = (currentAllyIndex < unlockedAllies.Count - 1);

        // Refresh the lower section (shard/skillbook) for the new ally
        // This ensures the item image matches the new ally while maintaining the same item type
        if (currentItemType == "shard")
        {
            StarUpLoading(); // Show shard for new ally
        }
        else if (currentItemType == "skillbook")
        {
            LevelUpLoading(); // Show skillbook for new ally
        }
        else
        {
            // Fallback to LevelUp mode
            LevelUpLoading();
        }
        
        // Load upgrade levels for the new ally
        if (upgradePanelManager != null)
        {
            upgradePanelManager.LoadUpgradePanelsForAlly($"{allyIndex}_{allyName}");
        }

        // Set Step2 stars based on actual star level for utilize flow
        if (step2StarGroup != null)
        {
            string allyId = $"{allyIndex}_{allyName}";
            int actualStarLevel = GetSidekickStarLevel(allyId);
            
            for (int i = 1; i <= 5; i++)
            {
                Transform step2Star = step2StarGroup.Find($"Star{i}/yellow");
                if (step2Star != null)
                {
                    bool shouldActivate = i <= actualStarLevel;
                    step2Star.gameObject.SetActive(shouldActivate);
                }
            }
        }
        
        // Load upgrade levels for the navigated ally (utilize mode only)
        if (upgradePanelManager != null)
        {
            upgradePanelManager.LoadUpgradePanelsForAlly($"{allyIndex}_{allyName}");
        }
        
        // Load equipment display for the navigated ally
        if (alliesEquipments != null)
        {
            alliesEquipments.InitForCurrentAlly();
            Debug.Log($"[AlliesGridSetup] Refreshed equipment slots for navigated ally: {allyName}");
        }
    }
    
    /// <summary>
    /// Get the current star level for a specific sidekick from PlayerProfile data
    /// </summary>
    /// <param name="allyId">The ally ID (e.g., "04_Aurelia")</param>
    /// <returns>Current star level from PlayerProfile data</returns>
    private int GetSidekickStarLevel(string allyId)
    {
        if (PlayerProfile.Data?.Sidekick == null)
        {
            return 0; // Default to 0 stars if no data
        }
        
        // Find the sidekick by matching the ally ID format
        // allyId is "04_Aurelia", base_id is "4", so we need to extract "04" and convert to int, then back to string
        string indexPart = allyId.Split('_')[0]; // "04"
        int allyIndex = int.Parse(indexPart); // 4
        string baseIdToMatch = allyIndex.ToString(); // "4"
        
        var sidekick = PlayerProfile.Data.Sidekick.FirstOrDefault(s => 
            s.base_id == baseIdToMatch
        );
        
        if (sidekick != null)
        {
            return sidekick.star;
        }
        
        // If sidekick not found, return 0 as default
        return 0;
    }
    
    /// <summary>
    /// Update star display for a specific ally item
    /// </summary>
    /// <param name="allyItem">The ally item GameObject</param>
    /// <param name="allyId">The ally ID (e.g., "04_Aurelia")</param>
    public void UpdateAllyStarDisplay(GameObject allyItem, string allyId)
    {
        Transform starGroup = allyItem.transform.Find("StarGroup");
        if (starGroup == null) 
        {
            return;
        }
        
        
        // Log current PlayerProfile data before getting star level
        if (PlayerProfile.Data?.Sidekick != null)
        {
            foreach (var s in PlayerProfile.Data.Sidekick)
            {
                if (s.base_id == allyId.Split('_')[0].TrimStart('0'))
                {
                    break;
                }
            }
        }
        
        int starLevel = GetSidekickStarLevel(allyId);
        
        // Update star states
        for (int i = 1; i <= 5; i++)
        {
            Transform starYellow = starGroup.Find($"Star{i}/yellow");
            if (starYellow != null)
            {
                bool shouldActivate = i <= starLevel;
                bool wasActive = starYellow.gameObject.activeSelf;
                starYellow.gameObject.SetActive(shouldActivate);
            }
            else
            {
            }
        }
        
    }
    
    
    /// <summary>
    /// Refresh all ally star displays (call this after star upgrades)
    /// </summary>
    public void RefreshAllAllyStarDisplays()
    {
        
        if (grid == null) 
        {
            return;
        }
        
        // Additional check: if grid has no children, force recreation
        if (grid.transform.childCount == 0)
        {
            InitializeAlliesGrid();
            return;
        }
        
        int processedCount = 0;
        for (int i = 0; i < grid.transform.childCount; i++)
        {
            Transform child = grid.transform.GetChild(i);
            
            if (child.name.StartsWith("AllyItem"))
            {
                // Extract ally info from the ally item
                string[] nameParts = child.name.Split('_');
                if (nameParts.Length >= 2)
                {
                    string allyIndex = nameParts[1]; // "04"
                    string allyName = nameParts.Length >= 3 ? nameParts[2] : ""; // "Aurelia" or empty
                    
                    // If we have both parts, create full ID, otherwise skip
                    if (!string.IsNullOrEmpty(allyName))
                    {
                        string allyId = $"{allyIndex}_{allyName}";
                        UpdateAllyStarDisplay(child.gameObject, allyId);
                        processedCount++;
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }
        }
        
        
    }
    
    /// <summary>
    /// Update Step2 star group display immediately - SIMPLE approach like S text
    /// </summary>
    /// <param name="starLevel">The star level (1-5)</param>
    public void UpdateStep2StarsFromSValue(int starLevel)
    {
        if (step2StarGroup == null)
        {
            return;
        }
        
        
        // Update Step2 star group - if S shows 3, enable Star1,2,3 and disable Star4,5
        for (int i = 1; i <= 5; i++)
        {
            Transform starYellow = step2StarGroup.Find($"Star{i}/yellow");
            if (starYellow != null)
            {
                bool shouldActivate = i <= starLevel;
                starYellow.gameObject.SetActive(shouldActivate);
            }
            else
            {
            }
        }
        
    }
    
    /// <summary>
    /// Update Step2 star group display immediately (for instant star upgrade feedback)
    /// </summary>
    /// <param name="allyId">The ally ID (e.g., "04_Aurelia")</param>
    public void UpdateStep2StarDisplay(string allyId)
    {
        if (step2StarGroup == null)
        {
            return;
        }
        
        
        // Get current star level from PlayerProfile data
        int starLevel = GetSidekickStarLevel(allyId);
        
        // Use the simple approach
        UpdateStep2StarsFromSValue(starLevel);
        
    }
    
    
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
    
    /// <summary>
    /// Reset the Orange button to default "Go to Pack" when entering Step 2 (both legacy text AND separate buttons)
    /// </summary>
    private void ResetOrangeButtonText()
    {
        SwitchPanels switchPanels = FindObjectOfType<SwitchPanels>();
        if (switchPanels != null)
        {
            switchPanels.UpdateButtonVisibility();
        }
    }
}