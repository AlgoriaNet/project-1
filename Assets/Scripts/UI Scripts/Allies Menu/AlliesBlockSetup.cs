using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // This is required for using Dictionary
using TMPro;

public class AlliesBlockSetup : MonoBehaviour
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
    public GameObject step2Panel; // Reference to Step 2 Panel

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
    public Button enhanceButton;
    public Button upgradeButton;
    public GameObject enhancePanel;
    public GameObject upgradePanel;
    public GameObject enhanceOrangeImage;
    public GameObject enhanceGreyImage;
    public GameObject upgradeOrangeImage;
    public GameObject upgradeGreyImage;
    public Transform forgePackContent; // Assign in Inspector: "ForgePage/Pack/Scroll View/Viewport/Content"
    public GridLayoutGroup forgePackGrid; // Assign the GridLayoutGroup of ForgePage's Pack in Inspector
    public GameObject forgeBlock; // Assign ForgePage → Block in Inspector

    public GameObject DismantlePage;
    public Transform dismantlePackContent; 
    public GridLayoutGroup dismantlePackGrid; 

    public GameObject CongratsPage;
    public GameObject DismantlePack;
    public RectTransform dismantleBlock; 

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

    void Start()
    {
        // Initially update the grid
        UpdateTotalBlocks(); // Fetch and update the grid layout based on TotalItemsCount.
        Debug.Log($"Page_3 active state at method start: {page3.activeSelf}");

    }

    public void UpdateTotalBlocks()
    {
        // Get the updated total item count from PlayerPrefs (it will change depending on selected item type)
        int totalItems = PlayerPrefs.GetInt("TotalItemsCount", 10); // Default to 10 if not set

        // // Dynamically calculate the total blocks (based on the number of items available and blocks per row)
        // totalBlocks = Mathf.CeilToInt((float)totalItems / blocksPerRow) * blocksPerRow;

        // Set totalBlocks to exactly the number of available items
        totalBlocks = totalItems;


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

        // // Calculate total rows (based on number of blocks)
        // int totalRows = Mathf.CeilToInt((float)totalBlocks / blocksPerRow);

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

            // OnClick listener
            blockButton.onClick.AddListener(() => 
            {
                // Get the Image component from the BlockItem's child (Image)
                Image blockImage = newBlock.transform.Find("Image").GetComponent<Image>();
                if (blockImage != null && blockImage.sprite != null)
                {
                    string imageFileName = blockImage.sprite.name; // Get the image file name
                }

                // Get the Qnty (quantity) text
                Transform qntyTransform = newBlock.transform.Find("Qnty");
                if (qntyTransform != null)
                {
                    TextMeshProUGUI qntyText = qntyTransform.GetComponent<TextMeshProUGUI>();
                }

                // Get the Part (image for the part, if exists)
                Transform partTransform = newBlock.transform.Find("Part");
                if (partTransform != null)
                {
                    Image partImage = partTransform.GetComponent<Image>();
                }

                // Call OpenStep3 with the clicked block
                OpenStep3(newBlock);
            });
        }
    }

    private void OpenStep3(GameObject blockItem)
    {
        // Hide Step 2 and show Step 3 panel
        step2Panel.SetActive(false);
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

            // Use the Inspector-assigned Step2Panel > UpperGroup > Right Panel > Grid
            Transform gridTransform = step2Panel.transform.Find("Upper Group/Right Panel/Grid");

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
            Debug.Log($"Part name from blockItem: {partName}"); // Debug the part name

            // Use the Inspector-assigned Step2Panel > UpperGroup > Right Panel > Grid
            Transform gridTransform = step2Panel.transform.Find("Upper Group/Right Panel/Grid");

            if (gridTransform != null)
            {
                Debug.Log($"Step2 Grid has {gridTransform.childCount} blocks."); // Debug total blocks

                // Iterate through all blocks in the grid to find the matching equipment
                foreach (Transform block in gridTransform)
                {
                    Image gridBlockImage = block.Find("Image")?.GetComponent<Image>();

                    if (gridBlockImage != null && gridBlockImage.sprite != null && gridBlockImage.sprite.name.StartsWith(partName))
                    {
                        Debug.Log($"Matching block found: {gridBlockImage.sprite.name}"); // Debug matching block

                        // Retrieve the "Dots" transform under the matching block
                        Transform dotsTransform = block.Find("Dots");
                        if (dotsTransform != null)
                        {
                            Debug.Log($"Dots found under block: {block.name}"); // Debug dots presence

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
                                                    Debug.Log($"Assigned Gem Image: {gemImageFileName} to Group_{i}"); // Debug assignment
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
                            Debug.Log($"Page_3 active state after centering: {page3.activeSelf}");
                            page4.SetActive(false);
                            Debug.Log("All groups disabled. Page_4 hidden, and Page_3 centered.");
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
        step2Panel.SetActive(true);

        // Any additional setup for Step 3 can be done here
        Debug.Log("Step 3 closed!");
    }

    public void OpenForgePage()
    {
        ForgePage.SetActive(true);
        CloseStep3();
        step2Panel.SetActive(false);

        LoadForgePagePack(); // Load Pack after activating Allies Step 2

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

        // Reference Allies Step 3 → Page_1 → UpperGroup → Block
        Transform AlliesBlock = page1.transform.Find("UpperGroup/Block");
        if (AlliesBlock == null)
        {
            Debug.LogError("❌ Allies Step 3 Block NOT found!");
            return;
        }

        // Copy TopText
        TextMeshProUGUI alliesTopText = AlliesBlock.Find("TopText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI forgeTopText = forgeBlock.transform.Find("TopText")?.GetComponent<TextMeshProUGUI>();
        if (alliesTopText != null && forgeTopText != null)
        {
            forgeTopText.text = alliesTopText.text;
        }

        // Copy Image
        Image alliesImage = AlliesBlock.Find("Image")?.GetComponent<Image>();
        Image forgeImage = forgeBlock.transform.Find("Image")?.GetComponent<Image>();
        if (alliesImage != null && forgeImage != null)
        {
            forgeImage.sprite = alliesImage.sprite;
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
        foreach (Transform item in contentPanel) // contentPanel is already assigned in AlliesBlockSetup
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
        step2Panel.SetActive(true);

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

    public void OpenDismantlePage()
    {
        DismantlePage.SetActive(true);
        CloseStep3();
        step2Panel.SetActive(false);

        LoadDismantlePagePack();
    }

    public void LoadDismantlePagePack()
    {
        if (dismantlePackContent == null)
        {
            Debug.LogError("❌ ForgePage Pack Content NOT assigned in Inspector!");
            return;
        }

        // Clear existing items in ForgePage Pack
        foreach (Transform child in dismantlePackContent)
        {
            Destroy(child.gameObject);
        }

        // Clone each item from the existing Pack
        foreach (Transform item in contentPanel) // contentPanel is already assigned in HeroBlockSetup
        {
            GameObject newItem = Instantiate(item.gameObject, dismantlePackContent);
            newItem.name = item.name; // Keep the same name
        }

        // ✅ Apply dynamic grid adjustments
        if (dismantlePackGrid != null)
        {
            dismantlePackGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            dismantlePackGrid.constraintCount = 5; // Match LowerGroup settings
            dismantlePackGrid.cellSize = new Vector2(blockWidth, blockWidth); 
            dismantlePackGrid.spacing = new Vector2(spacingX, spacingY);
            dismantlePackGrid.padding.left = Mathf.RoundToInt(leftPadding);
            dismantlePackGrid.padding.right = Mathf.RoundToInt(rightPadding);
        }

        Debug.Log("✅ dismantlePage Pack Loaded Successfully!");
    }

    public void OpenCongratsPage()
    {
        CongratsPage.SetActive(true);
        DismantlePack.SetActive(false);
        MoveBlockDown();
    }

    public void CloseDismantlePage()
    {
        DismantlePage.SetActive(false);
        step2Panel.SetActive(true);

        CongratsPage.SetActive(false);
        DismantlePack.SetActive(true);

        MoveBlockUp();
    }

    public void MoveBlockDown()
    {
        dismantleBlock.anchorMin = new Vector2(dismantleBlock.anchorMin.x, 0.5f);
        dismantleBlock.anchorMax = new Vector2(dismantleBlock.anchorMax.x, 0.5f);
    }

    public void MoveBlockUp()
    {
        dismantleBlock.anchorMin = new Vector2(dismantleBlock.anchorMin.x, 0.75f);
        dismantleBlock.anchorMax = new Vector2(dismantleBlock.anchorMax.x, 0.7f);
    }
}