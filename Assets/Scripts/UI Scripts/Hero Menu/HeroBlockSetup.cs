using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using model;
using PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.Gemstones;
using PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.PopUpBox; // This is required for using Dictionary
using TMPro;
using Debug = UnityEngine.Debug;

public class HeroBlockSetup : MonoBehaviour
{
    public GridLayoutGroup grid; // Assign the GridLayoutGroup in Inspector
    public RectTransform contentPanel; // Assign the ContentPanel RectTransform
    public GameObject blockPrefab; 
    public GameObject Pack; 
    public Button Button_Dismantle;
    public ScrollRect scrollView; // Assign the ScrollRect in Inspector
    public int blocksPerRow = 5; // Fixed columns (5 per row)
    public int totalBlocks; // Total number of blocks (dynamically fetched from PlayerPrefs)

    private float blockWidth; // This will dynamically adjust the block width based on available space
    private float leftPadding;
    private float rightPadding;
    private float spacingX;
    private float spacingY;
    public GameObject heroStep2Panel; // Reference to Hero Step 2 Panel
    public GameObject allyStep2Panel; // Reference to Ally Step 2 Panel

    [SerializeField]
    private ItemLoader itemLoader;

    public GameObject step3Panel; // Add reference to Step 3 panel
    public Button closeButton; // Reference to the Close Button on Step 3
    public GameObject equipmentPages; // Assign EquipmentPages in Inspector
    public GameObject gemPages;       // Assign GemPages in Inspector
    public GameObject page1;  
    public GameObject page2;  
    public GameObject page3; 
    public GameObject page4;   
    public GameObject ForgePage; 
    public GameObject CommonPage; 
    public Button enhanceButton;
    public Button upgradeButton;
    public GameObject enhancePanel;
    public GameObject upgradePanel;
    public GameObject enhanceOrangeImage;
    public GameObject enhanceGreyImage;
    public GameObject upgradeOrangeImage;
    public GameObject upgradeGreyImage;
    public Transform forgePackContent; 
    public GridLayoutGroup forgePackGrid;     public GameObject forgeBlock; 

    public GameObject commonPage; 


    // A dictionary to store the mapping between gem image file names and their localized names
    private Dictionary<string, string> gemNameLocalization = new Dictionary<string, string>
    {
        { "Gem_01", "Common Gem" },
        { "Gem_02", "Superior Gem" },
        { "Gem_03", "Rare Gem" },
        { "Gem_04", "Epic Gem" },
        { "Gem_05", "Legendary Gem" },
        { "Gem_06", "Mythic Gem" },
        { "Gem_07", "Ultimate Gem" }
    };

    // Flag to track if we need to refresh pack UI when GameObject becomes active
    private bool needsRefreshOnEnable = false;

    void Start()
    {
        // Initially update the grid
        UpdateTotalBlocks(); // Fetch and update the grid layout based on TotalItemsCount.
        
        // Listen to specific notifications - Equipment and Gem tabs should only refresh 
        // when equipment or gems are actually added, not for hero draws (shards)
        PlayerProfile.Data.AddListener(UpdateUI, "Equipments");
        PlayerProfile.Data.AddListener(UpdateUI, "Gemstones");
        
        // Also listen to OtherItems for the Others tab refresh
        PlayerProfile.Data.AddListener(UpdateOthersTabOnly, "OtherItems");
    }
    
    private void OnEnable()
    {
        // If we have a pending refresh, do it now that the GameObject is active
        if (needsRefreshOnEnable)
        {
            needsRefreshOnEnable = false;
            StartCoroutine(DelayedUpdateTotalBlocks());
        }
    }
    
    private void OnDestroy()
    {
        // Clean up listeners to prevent memory leaks
        if (PlayerProfile.Data != null)
        {
            PlayerProfile.Data.RemoveListener(UpdateUI, "Equipments");
            PlayerProfile.Data.RemoveListener(UpdateUI, "Gemstones");
            PlayerProfile.Data.RemoveListener(UpdateOthersTabOnly, "OtherItems");
        }
    }

    private void UpdateUI(ApplicationModel model)
    {
        // Check if the GameObject is active before starting coroutine
        if (gameObject.activeInHierarchy)
        {
            // Add a small delay to ensure player data is fully updated before refreshing pack UI
            StartCoroutine(DelayedUpdateTotalBlocks());
        }
        else
        {
            // If the GameObject is inactive, mark that we need to update when it becomes active
            needsRefreshOnEnable = true;
        }
    }
    
    private void UpdateOthersTabOnly(ApplicationModel model)
    {
        // Only refresh the Others tab when OtherItems change (e.g., from hero draws)
        UpdateOthersTab();
    }
    
    private System.Collections.IEnumerator DelayedUpdateTotalBlocks()
    {
        // Wait one frame to ensure all data updates are complete
        yield return null;
        UpdateTotalBlocks();
    }
    
    public void UpdateTotalBlocks()
    {
        if (itemLoader.currentItemType == ItemLoader.ItemType.Equipment)
        {
            var equipments = PlayerProfile.Data.GetEquipmentsInPack();
            totalBlocks = equipments?.Count ?? 0;
            Debug.Log($"[HeroBlockSetup] Equipment count: {totalBlocks}");
        }
        else if (itemLoader.currentItemType == ItemLoader.ItemType.Gem)
        {
            var gemstones = PlayerProfile.Data.GetGemstonesInPack();
            totalBlocks = gemstones?.Count ?? 0;
            Debug.Log($"[HeroBlockSetup] Gemstone count: {totalBlocks}");
        }

        // Update the grid layout dynamically to match the number of blocks
        UpdateGridLayout();
    }

    private void UpdateGridLayout()
    {
        // Dynamically calculate block width based on content panel width
        float panelWidth = contentPanel.rect.width;

        // Calculate block width and padding dynamically
        blockWidth = panelWidth / (blocksPerRow + 1);
        leftPadding = blockWidth * 0.25f;
        rightPadding = blockWidth * 0.25f;

        spacingX = blockWidth * 0.125f;
        spacingY = blockWidth * 0.125f;
        grid.padding.top = Mathf.RoundToInt(spacingY);

        // Adjust GridLayoutGroup settings
        grid.cellSize = new Vector2(blockWidth, blockWidth); // Apply calculated block width
        grid.spacing = new Vector2(spacingX, spacingY);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = blocksPerRow;

        // Apply padding to the grid
        grid.padding.left = Mathf.RoundToInt(leftPadding);
        grid.padding.right = Mathf.RoundToInt(rightPadding);

        // Adjust Content Panel RectTransform to stretch horizontally
        contentPanel.anchorMin = new Vector2(0, 0); // Align to the bottom-left of the parent
        contentPanel.anchorMax = new Vector2(1, 1); // Stretch horizontally and vertically
        contentPanel.offsetMin = new Vector2(leftPadding, contentPanel.offsetMin.y); // Left padding
        contentPanel.offsetMax = new Vector2(-rightPadding, contentPanel.offsetMax.y); // Right padding

        // Calculate total rows dynamically without enforcing a minimum row count
        int totalRows = Mathf.Max(1, Mathf.CeilToInt((float)totalBlocks / blocksPerRow));

        // Adjust Content size to fit the blocks
        RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
        float contentHeight = totalRows * (blockWidth + spacingY) - spacingY; // Total height based on rows and spacing
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentHeight);

        // Create the blocks dynamically based on the number of blocks
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject); // Clear previous blocks
        }
        
        Debug.Log($"[HeroBlockSetup] Creating {totalBlocks} blocks for {itemLoader.currentItemType}");

        for (int i = 0; i < totalBlocks; i++)
        {
            GameObject newBlock = Instantiate(blockPrefab, contentPanel);
            newBlock.name = $"Block_{i + 1}"; // Optional naming for easy identification

            // Add a Button Component to the Block (if not already added)
            Button blockButton = newBlock.GetComponent<Button>();
            if (blockButton == null)
            {
                blockButton = newBlock.AddComponent<Button>();
            }
            
            if (itemLoader.currentItemType == ItemLoader.ItemType.Equipment)
            {
                List<Equipment> equipments = PlayerProfile.Data.GetEquipmentsInPack();
                if (i < equipments.Count)
                {
                    Equipment equipment = equipments[i];
                    Debug.Log($"[HeroBlockSetup] Loading equipment block {i}: {equipment?.Name}");
                    itemLoader.LoadEquipmentItems(newBlock.transform, equipment);
                    blockButton.onClick.AddListener(() => 
                    {
                        // Detect context based on active panels
                        EquipmentComparisonManager.EquippedOn context = EquipmentComparisonManager.EquippedOn.Hero;
                        
                        Debug.Log($"[HeroBlockSetup] Context Detection - allyStep2Panel null: {allyStep2Panel == null}");
                        if (allyStep2Panel != null)
                        {
                            Debug.Log($"[HeroBlockSetup] Context Detection - allyStep2Panel active: {allyStep2Panel.activeInHierarchy}");
                        }
                        Debug.Log($"[HeroBlockSetup] Context Detection - heroStep2Panel null: {heroStep2Panel == null}");
                        if (heroStep2Panel != null)
                        {
                            Debug.Log($"[HeroBlockSetup] Context Detection - heroStep2Panel active: {heroStep2Panel.activeInHierarchy}");
                        }
                        
                        if (allyStep2Panel != null && allyStep2Panel.activeInHierarchy)
                        {
                            context = EquipmentComparisonManager.EquippedOn.Sidekick;
                            Debug.Log("[HeroBlockSetup] Detected Ally context - using Sidekick");
                        }
                        else
                        {
                            Debug.Log("[HeroBlockSetup] Detected Hero context - using Hero");
                        }
                        
                        EquipmentComparisonManager.Instance.Init(context, equipment?.Id); 
                    });
                }
                
            }
            else if (itemLoader.currentItemType == ItemLoader.ItemType.Gem)
            {
                var gemstones = PlayerProfile.Data.GetGemstonesInPack();
                if (i < gemstones.Count)
                {
                    Gemstone gemstone = gemstones[i];
                    Debug.Log($"[HeroBlockSetup] Loading gem block {i}: Gem_{gemstone?.Level:D2}");
                    itemLoader.LoadGemItems(newBlock.transform, gemstone);
                    blockButton.onClick.AddListener(() => 
                    {
                        GemDetailWithInlaid.Instance.Init(gemstone, null); 
                    });
                }
            }

            

            // OnClick listener
            blockButton.onClick.AddListener(() => 
            {
                
                // Call OpenStep3 with the clicked block
                // OpenStep3(newBlock);
            });
        }
    }

    private void OpenStep3(GameObject blockItem)
    {
        // Hide Step 2 and show Step 3 panel
        heroStep2Panel.SetActive(false);
        step3Panel.SetActive(true);

        // Call the appropriate page logic based on the item type
        if (itemLoader.currentItemType == ItemLoader.ItemType.Equipment)
        {
            equipmentPages.SetActive(true);
            gemPages.SetActive(false);
            OpenPage2(blockItem); // Delegate to OpenPage2
            OpenPage1(blockItem); // Add call for OpenPage1
        }
        else if (itemLoader.currentItemType == ItemLoader.ItemType.Gem)
        {
            equipmentPages.SetActive(false);
            gemPages.SetActive(true);
            OpenPage3(blockItem); // Delegate to OpenPage3
            OpenPage4(blockItem);
        }
    }

    private void OpenPage2(GameObject blockItem)
    {
        // Get the Image component from the BlockItem's child (Image)
        Image blockImage = blockItem.transform.Find("Image").GetComponent<Image>();

        if (blockImage != null && blockImage.sprite != null)
        {
            string imageFileName = blockImage.sprite.name; // Get the image file name
            string equipmentName = imageFileName.Split('_')[0]; // Equipment name (before the "_")
            string rankValue = imageFileName.Split('_')[1]; // Rank value (after the "_")

            // Use the Inspector-assigned Page_2 object
            Transform blockTransform = page2.transform.Find("UpperGroup/Block");

            // Use the helper method to set the values
            SetPageBlockValues(blockTransform, imageFileName, equipmentName, rankValue);
        }
    }

    private void OpenPage1(GameObject blockItem)
    {
        // Get the Image component from the BlockItem's child (Image)
        Image blockImage = blockItem.transform.Find("Image").GetComponent<Image>();
        page1.SetActive(true);

        RectTransform page2Rect = page2.GetComponent<RectTransform>();
        page2Rect.anchorMin = new Vector2(0.51f, page2Rect.anchorMin.y); // Set X for Min
        page2Rect.anchorMax = new Vector2(0.975f, page2Rect.anchorMax.y); // Set X for Max

        if (blockImage != null && blockImage.sprite != null)
        {
            string imageFileName = blockImage.sprite.name; // Get the image file name
            string equipmentName = imageFileName.Split('_')[0]; // Equipment name (before the "_")

            // Use the Inspector-assigned heroStep2Panel > UpperGroup > Right Panel > Grid
            Transform gridTransform = heroStep2Panel.transform.Find("Upper Group/Right Panel/Grid");

            if (gridTransform != null)
            {
                // Iterate through all blocks in the grid to find a matching image
                foreach (Transform block in gridTransform)
                {
                    Image gridBlockImage = block.Find("Image")?.GetComponent<Image>();

                    if (gridBlockImage != null && gridBlockImage.sprite != null && gridBlockImage.sprite.name.StartsWith(equipmentName))
                    {
                        string matchedImageFileName = gridBlockImage.sprite.name; // Matching image file name
                        string matchedRankValue = matchedImageFileName.Split('_')[1]; // Extract rank value

                        // Use the Inspector-assigned Page_1 object
                        Transform blockTransform = page1.transform.Find("UpperGroup/Block");

                        if (blockTransform != null)
                        {
                            // Find Image, TopText, and RankValue in the block
                            Image image = blockTransform.Find("Image")?.GetComponent<Image>();
                            TextMeshProUGUI topText = blockTransform.Find("TopText")?.GetComponent<TextMeshProUGUI>();
                            TextMeshProUGUI rankValueText = blockTransform.Find("RankValue")?.GetComponent<TextMeshProUGUI>();

                            if (image != null && topText != null && rankValueText != null)
                            {
                                // Load and set the sprite
                                Sprite equipmentSprite = Resources.Load<Sprite>($"UILoading/Equipment/{matchedImageFileName}");
                                if (equipmentSprite != null)
                                {
                                    image.sprite = equipmentSprite;
                                    image.color = Color.white;
                                }

                                // Assign text values from the matched block in Step 2
                                topText.text = equipmentName;
                                rankValueText.text = matchedRankValue;
                            }
                        }

                        return; // Exit once the matching block is found
                    }
                }

                page2Rect.anchorMin = new Vector2(0.2675f, page2Rect.anchorMin.y); // Set X for Min
                page2Rect.anchorMax = new Vector2(0.7325f, page2Rect.anchorMax.y); // Set X for Max

                page1.SetActive(false);
            }
        }
    }

    private void SetPageBlockValues(Transform pageBlock, string imageFileName, string equipmentName, string rankValue)
    {
        if (pageBlock != null)
        {
            // Find Image, TopText, and RankValue in the block
            Image image = pageBlock.Find("Image")?.GetComponent<Image>();
            TextMeshProUGUI topText = pageBlock.Find("TopText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI rankValueText = pageBlock.Find("RankValue")?.GetComponent<TextMeshProUGUI>();

            if (image != null && topText != null && rankValueText != null)
            {
                // Load and set the sprite
                Sprite equipmentSprite = Resources.Load<Sprite>($"UILoading/Equipment/{imageFileName}");
                if (equipmentSprite != null)
                {
                    image.sprite = equipmentSprite;
                    image.color = Color.white; // Ensure the image is visible
                }

                // Assign text values
                topText.text = equipmentName;
                rankValueText.text = rankValue;
            }
        }
    }

    private void OpenPage3(GameObject blockItem)
    {      
        // Get the Image component from the BlockItem's child (Image)
        Image blockImage = blockItem.transform.Find("Image").GetComponent<Image>();

        if (blockImage != null && blockImage.sprite != null)
        {
            string imageFileName = blockImage.sprite.name; // Get the image file name

            // Use the Inspector-assigned Page_3 object
            Transform blockTransform = page3.transform.Find("UpperGroup/Block");

            if (blockTransform != null)
            {
                // Find Image, TopText, and QntyValue in the block
                Image image = blockTransform.Find("Image")?.GetComponent<Image>();
                TextMeshProUGUI topText = blockTransform.Find("TopText")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI qntyText = blockTransform.Find("QntyValue")?.GetComponent<TextMeshProUGUI>();

                if (image != null && topText != null && qntyText != null)
                {
                    // Load and set the sprite
                    Sprite gemSprite = Resources.Load<Sprite>($"UILoading/Gem/Stone/{imageFileName}");
                    if (gemSprite != null)
                    {
                        image.sprite = gemSprite;
                        image.color = Color.white;
                    }

                    // Assign text values
                    topText.text = gemNameLocalization.ContainsKey(imageFileName)
                        ? gemNameLocalization[imageFileName]
                        : "Unknown Gem"; // Handle missing localization
                    qntyText.text = blockItem.transform.Find("Qnty").GetComponent<TextMeshProUGUI>().text;
                }
            }
        }
    }

    private void OpenPage4(GameObject blockItem)
    {
        // Re-enable all groups at the beginning
        for (int i = 1; i <= 5; i++)
        {
            Transform groupTransform = page4.transform.Find($"Group_{i}");
            if (groupTransform != null)
            {
                groupTransform.gameObject.SetActive(true); // Re-enable the group
            }
        }

        bool anyGroupActive = false; // Track if any group remains active

        // Retrieve the part image from the blockItem (e.g., Helm, Pants, etc.)
        Image partImage = blockItem.transform.Find("Part")?.GetComponent<Image>();

        if (partImage != null && partImage.sprite != null)
        {
            string partName = partImage.sprite.name; // Retrieve the name of the part image

            // Use the Inspector-assigned heroStep2Panel > UpperGroup > Right Panel > Grid
            Transform gridTransform = heroStep2Panel.transform.Find("Upper Group/Right Panel/Grid");

            if (gridTransform != null)
            {
                // Iterate through all blocks in the grid to find the matching equipment
                foreach (Transform block in gridTransform)
                {
                    Image gridBlockImage = block.Find("Image")?.GetComponent<Image>();

                    if (gridBlockImage != null && gridBlockImage.sprite != null && gridBlockImage.sprite.name.StartsWith(partName))
                    {
                        // Retrieve the "Dots" transform under the matching block
                        Transform dotsTransform = block.Find("Dots");
                        if (dotsTransform != null)
                        {
                            for (int i = 1; i <= 5; i++) // Assume there are 5 dots
                            {
                                string dotName = $"D{i}";
                                Transform dotTransform = dotsTransform.Find(dotName); // Access D1, D2, ..., D5

                                if (dotTransform != null)
                                {
                                    Image dotImage = dotTransform.GetComponent<Image>();
                                    if (dotImage != null && dotImage.sprite != null)
                                    {
                                        Debug.Log($"Found {dotName} Image File: {dotImage.sprite.name}"); // Debug dot image file name

                                        Transform groupTransform = page4.transform.Find($"Group_{i}");
                                        if (groupTransform != null)
                                        {
                                            if (dotImage.sprite.name == "Dot_00")
                                            {
                                                groupTransform.gameObject.SetActive(false); // Disable the group
                                                Debug.Log($"Disabled Group_{i} because the dot is Dot_00");
                                                continue; // Skip further processing for this group
                                            }

                                            anyGroupActive = true; // At least one group is active

                                            // Derive the Gem image based on the dot's index (e.g., Gem_01 for D1)
                                            string gemImageFileName = dotImage.sprite.name.Replace("Dot", "Gem");

                                            // Map dots to Page_4 groups
                                            Image groupImage = groupTransform.Find("Image")?.GetComponent<Image>();

                                            if (groupImage != null)
                                            {
                                                // Load and assign the corresponding Gem image
                                                Sprite gemSprite = Resources.Load<Sprite>($"UILoading/Gem/Stone/{gemImageFileName}");
                                                if (gemSprite != null)
                                                {
                                                    groupImage.sprite = gemSprite; // Set the gem image
                                                    groupImage.color = Color.white; // Ensure the image is visible
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // After processing the blocks and groups
                        if (anyGroupActive)
                        {
                            Debug.Log("At least one group active. Ensuring Page_4 is visible.");
                            ResetPage4AndPage3(); // Only reset if groups are active
                        }
                        else
                        {
                            // Handle the case where all groups are disabled
                            RectTransform page3Rect = page3.GetComponent<RectTransform>();
                            page3Rect.anchorMin = new Vector2(0.2675f, page3Rect.anchorMin.y); // Set X for Min
                            page3Rect.anchorMax = new Vector2(0.7325f, page3Rect.anchorMax.y); // Set X for Max
                            page4.SetActive(false);
                        }

                        return; // Exit once the matching block is processed
                    }
                }
            }
        }
    }

    // Reset Page_3's anchor and ensure Page_4 is enabled only when necessary
    private void ResetPage4AndPage3()
    {
        // Enable Page_4
        page4.SetActive(true);

        // Reset Page_3's anchor to its original position
        RectTransform page3ResetRect = page3.GetComponent<RectTransform>();
        page3ResetRect.anchorMin = new Vector2(0.025f, page3ResetRect.anchorMin.y); // Set X for Min
        page3ResetRect.anchorMax = new Vector2(0.49f, page3ResetRect.anchorMax.y); // Set X for Max

        Debug.Log("Page_4 re-enabled, and Page_3 anchor reset.");
    }

    public void CloseStep3()
    {
        // Hide the current panel (e.g., Step 2)
        step3Panel.SetActive(false);

        // Show Step 3 panel
        heroStep2Panel.SetActive(true);
    }

    public void OpenForgePage()
    {
        ForgePage.SetActive(true);
        CommonPage.SetActive(false);
        CloseStep3();
        heroStep2Panel.SetActive(false);
        allyStep2Panel.SetActive(false);
        LoadForgePagePack(); // Load Pack after activating Hero Step 2

        // Add button listeners when ForgePage opens
        enhanceButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.RemoveAllListeners();
        enhanceButton.onClick.AddListener(() => ToggleEnhanceUpgrade(true));
        upgradeButton.onClick.AddListener(() => ToggleEnhanceUpgrade(false));

        // Set default state
        ToggleEnhanceUpgrade(true);
    }


    // This Loading block data method will be only triggered manualy by the button on the Page_1
    public void LoadForgePageData()
    {
        if (forgeBlock == null)
        {
            Debug.LogError("❌ ForgePage Block NOT assigned in Inspector!");
            return;
        }

        // Reference Hero Step 3 → Page_1 → UpperGroup → Block
        Transform heroBlock = page1.transform.Find("UpperGroup/Block");
        if (heroBlock == null)
        {
            Debug.LogError("❌ Hero Step 3 Block NOT found!");
            return;
        }

        // Copy TopText
        TextMeshProUGUI heroTopText = heroBlock.Find("TopText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI forgeTopText = forgeBlock.transform.Find("TopText")?.GetComponent<TextMeshProUGUI>();
        if (heroTopText != null && forgeTopText != null)
        {
            forgeTopText.text = heroTopText.text;
        }

        // Copy Image
        Image heroImage = heroBlock.Find("Image")?.GetComponent<Image>();
        Image forgeImage = forgeBlock.transform.Find("Image")?.GetComponent<Image>();
        if (heroImage != null && forgeImage != null)
        {
            forgeImage.sprite = heroImage.sprite;
            forgeImage.color = Color.white; // Ensure visibility
        }

        Debug.Log("✅ ForgePage Block Updated Successfully!");
    }

    public void LoadForgePagePack()
    {
        if (forgePackContent == null)
        {
            Debug.LogError("❌ ForgePage Pack Content NOT assigned in Inspector!");
            return;
        }

        // Clear existing items in ForgePage Pack
        foreach (Transform child in forgePackContent)
        {
            Destroy(child.gameObject);
        }

        // Clone each item from the existing Pack
        foreach (Transform item in contentPanel) // contentPanel is already assigned in HeroBlockSetup
        {
            GameObject newItem = Instantiate(item.gameObject, forgePackContent);
            newItem.name = item.name; // Keep the same name
        }

        // ✅ Apply dynamic grid adjustments
        if (forgePackGrid != null)
        {
            forgePackGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            forgePackGrid.constraintCount = 5; // Match LowerGroup settings
            forgePackGrid.cellSize = new Vector2(blockWidth, blockWidth); 
            forgePackGrid.spacing = new Vector2(spacingX, spacingY);
            forgePackGrid.padding.left = Mathf.RoundToInt(leftPadding);
            forgePackGrid.padding.right = Mathf.RoundToInt(rightPadding);
        }

        Debug.Log("✅ ForgePage Pack Loaded Successfully!");
    }

    private void ToggleEnhanceUpgrade(bool isEnhance)
    {
        // Enable Enhance Panel, Disable Upgrade Panel
        enhancePanel.SetActive(isEnhance);
        upgradePanel.SetActive(!isEnhance);

        // Handle Enhance Button visuals
        enhanceButton.transform.Find("OrangeButton").gameObject.SetActive(isEnhance);
        enhanceButton.transform.Find("GreyButton").gameObject.SetActive(!isEnhance);

        // Handle Upgrade Button visuals
        upgradeButton.transform.Find("OrangeButton").gameObject.SetActive(!isEnhance);
        upgradeButton.transform.Find("GreyButton").gameObject.SetActive(isEnhance);
    }

    public void CloseForgePage()
    {
        ForgePage.SetActive(false);
        CommonPage.SetActive(true);
        heroStep2Panel.SetActive(true);

        // Clear ForgeBlock TopText and Image
        TextMeshProUGUI forgeTopText = forgeBlock.transform.Find("TopText")?.GetComponent<TextMeshProUGUI>();
        Image forgeImage = forgeBlock.transform.Find("Image")?.GetComponent<Image>();

        if (forgeTopText != null) forgeTopText.text = "";
        if (forgeImage != null)
        {
            forgeImage.sprite = null;
            forgeImage.color = new Color(0, 0, 0, 0); // Fully transparent
        }

        Debug.Log("✅ ForgePage Block Cleared on Close!");
    }







    // Call this when the 'Other' tab is selected to refresh the UI
    public void OnOtherTabSelected()
    {
        Debug.Log("[OtherTab] Selected. Updating Others tab UI.");
        UpdateOthersTab();
    }

    // copilot agent 2025-06-29: Update the Others tab to display all items from ItemsJson (hero only)
    public void UpdateOthersTab()
    {
        var otherItemsRaw = model.PlayerProfile.Data.GetOtherItemsInPack();
        Debug.Log($"[OtherTab] ItemsJson count: {otherItemsRaw?.Count ?? 0}");
        if (otherItemsRaw == null || otherItemsRaw.Count == 0)
        {
            Debug.LogWarning("[OtherTab] ItemsJson is empty! Add test data or check server sync.");
            return;
        }

        // 1. Exclude unwanted keys
        var excludeKeys = new HashSet<string> { "heroKey", "rareKey", "epicKey" };

        // 2. Group and sum by true item name (shard/skillbook logic)
        var grouped = new Dictionary<string, int>(); // key: fileName, value: total qnty
        var typeMap = new Dictionary<string, string>(); // key: fileName, value: "shard" or "skillbook"
        foreach (var kvp in otherItemsRaw)
        {
            string rawKey = kvp.Key;
            int qnty = kvp.Value;
            if (excludeKeys.Contains(rawKey)) continue;

            // GachaController logic: key format is like "20_Nyx" for shards, "SKb_SkillbookName" for skillbooks
            string fileName;
            string type;
            if (rawKey.StartsWith("SKb_"))
            {
                fileName = rawKey;
                type = "skillbook";
            }
            else
            {
                fileName = rawKey;
                type = "shard";
            }
            if (!grouped.ContainsKey(fileName))
            {
                grouped[fileName] = 0;
                typeMap[fileName] = type;
            }
            grouped[fileName] += qnty;
        }

        // 3. Clear existing UI
        foreach (Transform child in contentPanel)
            Destroy(child.gameObject);

        Debug.Log($"[OtherTab] Creating blocks for {grouped.Count} grouped items");

        // 4. Create blocks for each grouped item (skip items with quantity 0)
        int blocksCreated = 0;
        foreach (var kvp in grouped)
        {
            string fileName = kvp.Key;
            int totalQnty = kvp.Value;
            string type = typeMap[fileName];

            // Skip items with quantity 0 - they should not appear in the pack
            if (totalQnty <= 0)
            {
                Debug.Log($"[OtherTab] Skipping {fileName} with quantity {totalQnty}");
                continue;
            }

            blocksCreated++;
            GameObject newBlock = Instantiate(blockPrefab, contentPanel);
            newBlock.name = $"OtherItem_{fileName}";

            // Set item image
            Image image = newBlock.transform.Find("Image").GetComponent<Image>();
            image.sprite = LoadOtherItemSprite_GachaStyle(fileName, type);
            if (image.sprite != null)
            {
                image.color = Color.white;
            }
            else
            {
                image.color = Color.clear;
                Debug.LogWarning($"[OtherTab] Sprite not found for fileName: {fileName}");
            }

            // Set quantity
            TextMeshProUGUI qntyText = newBlock.transform.Find("Qnty").GetComponent<TextMeshProUGUI>();
            qntyText.text = totalQnty.ToString();

            // Add click handler for Other items
            Button blockButton = newBlock.GetComponent<Button>();
            if (blockButton == null)
            {
                blockButton = newBlock.AddComponent<Button>();
            }
            
            // Capture variables for the lambda
            string capturedFileName = fileName;
            int capturedQuantity = totalQnty;
            string capturedType = type;
            
            blockButton.onClick.AddListener(() => 
            {
                Debug.Log($"[OtherTab] Block clicked: {capturedFileName}");
                if (OtherDetailBox.Instance != null)
                {
                    OtherDetailBox.Instance.Init(capturedFileName, capturedQuantity, capturedType);
                }
                else
                {
                    Debug.LogError("[OtherTab] OtherDetailBox.Instance is null!");
                }
            });
        }
        
        Debug.Log($"[OtherTab] UpdateOthersTab complete: {blocksCreated} blocks created from {grouped.Count} total items");
    }

    // Helper: match GachaController logic for image path
    private Sprite LoadOtherItemSprite_GachaStyle(string fileName, string type)
    {
        string path = type == "skillbook"
            ? $"UILoading/CharacterImages/Skillbook/{fileName}"
            : $"UILoading/CharacterImages/Shard/{fileName}";
        Sprite s = Resources.Load<Sprite>(path);
        if (s == null) Debug.LogWarning($"[OtherTab] Sprite not found at {path}");
        return s;
    }

    // Add these references at the top of the class
    public Button equipmentTabButton;
    public Button gemTabButton;
    public Button otherTabButton;

    // Programmatically select the Equipment tab (index 0) and refresh UI
    public void SelectEquipmentTab()
    {
        if (equipmentTabButton != null)
        {
            equipmentTabButton.onClick.Invoke(); // Simulate user click for full tab logic
            Debug.Log("[HeroBlockSetup] Equipment tab selected via onClick.Invoke().");
            return;
        }
        Debug.LogWarning("[HeroBlockSetup] equipmentTabButton not assigned!");
    }
}