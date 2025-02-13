using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;  // For IEnumerator and coroutines
using TMPro;

public class ItemLoader : MonoBehaviour
{
    public enum ItemType { Equipment, Gem, Other }
    public ItemType currentItemType = ItemType.Equipment;

    public Dictionary<ItemType, int> itemCounts = new Dictionary<ItemType, int>
    {
        { ItemType.Equipment, 13 },
        { ItemType.Gem, 8 },
        { ItemType.Other, 5 }
    };

    public string resourcesPath = "ItemImages";
    public delegate void OnItemCountChanged(int itemCount); // Event for data transfer
    public event OnItemCountChanged ItemCountChanged;

    private MenuController menuController; // Declare MenuController variable

    // Action button and its text
    public Button actionButton;
    public TextMeshProUGUI actionButtonText;

    // Reference to HeroMenuPopup to open the Gem Merge page
    public HeroMenuPopup heroMenuPopup;

    void Awake()
    {
        menuController = FindObjectOfType<MenuController>();
    }

    void Start()
    {
        NotifyItemCount();
        StartCoroutine(PopulateItemsDelayed());
    }

    public void SwitchItemType(ItemType itemType)
    {
        Debug.Log($"SwitchItemType called with {itemType} (Before: {currentItemType})");
        currentItemType = itemType;
        Debug.Log($"Updated currentItemType: {currentItemType}");
        
        NotifyItemCount();

        // Dynamically update action button based on selected item type
        UpdateActionButton(itemType);

        if (menuController == null)
        {
            Debug.LogError("menuController is NULL in SwitchItemType!");
            return;
        }

        if (menuController.IsMenuActive(0)) // Allies Menu
        {
            FindObjectOfType<AlliesBlockSetup>()?.UpdateTotalBlocks();
        }
        else if (menuController.IsMenuActive(1)) // Hero Menu
        {
            FindObjectOfType<HeroBlockSetup>()?.UpdateTotalBlocks();
        }

        // Wait until the grid update is complete, then reload the items
        StartCoroutine(PopulateItemsDelayed());
    }

    private void UpdateActionButton(ItemType itemType)
    {
        // Update action button text and listener based on the selected item type
        if (menuController.IsMenuActive(0)) // Allies Menu
        {
            // Allies Menu
            switch (itemType)
            {
                case ItemType.Equipment:
                    actionButtonText.text = "Auto Equip";
                    actionButton.onClick.RemoveAllListeners(); // Remove any previous listeners
                    actionButton.onClick.AddListener(OpenAutoEquipPage); // Placeholder for Auto Equip action
                    break;

                case ItemType.Gem:
                    actionButtonText.text = "Auto Embed";
                    actionButton.onClick.RemoveAllListeners(); // Remove any previous listeners
                    actionButton.onClick.AddListener(OpenAutoEmbedPage); // Placeholder for Auto Embed action
                    break;

                case ItemType.Other:
                    actionButtonText.text = "Other Action";
                    actionButton.onClick.RemoveAllListeners(); // Remove any previous listeners
                    actionButton.onClick.AddListener(OpenOtherPage); // Placeholder for other action
                    break;
            }
        }
        else if (menuController.IsMenuActive(1)) // Hero Menu
        {
            // Hero Menu
            switch (itemType)
            {
                case ItemType.Equipment:
                    actionButtonText.text = "Dismantle";
                    actionButton.onClick.RemoveAllListeners(); // Remove any previous listeners
                    actionButton.onClick.AddListener(OpenDismantlePage); // Add listener for dismantle action
                    break;

                case ItemType.Gem:
                    actionButtonText.text = "Auto Merge";
                    actionButton.onClick.RemoveAllListeners(); // Remove any previous listeners
                    actionButton.onClick.AddListener(OpenGemMergePage); // Add listener for auto merge action
                    break;

                case ItemType.Other:
                    actionButtonText.text = "Other Action";
                    actionButton.onClick.RemoveAllListeners(); // Remove any previous listeners
                    actionButton.onClick.AddListener(OpenOtherPage); // Placeholder for other action
                    break;
            }
        }
    }

    private void OpenAutoEquipPage()
    {
        // Logic for Auto Equip (to be implemented)
        Debug.Log("Opening Auto Equip Page...");
    }

    private void OpenAutoEmbedPage()
    {
        // Logic for Auto Embed (to be implemented)
        Debug.Log("Opening Auto Embed Page...");
    }

    private void OpenDismantlePage()
    {
        // Logic for Dismantle (to be implemented)
        Debug.Log("Opening Dismantle Page...");
    }

    private void OpenGemMergePage()
    {
        // Call OpenGemMergePage in HeroMenuPopup
        if (heroMenuPopup != null)
        {
            heroMenuPopup.OpenGemMergePage();
        }
        else
        {
            Debug.LogError("HeroMenuPopup is not assigned!");
        }
    }

    private void OpenOtherPage()
    {
        // Logic for Other Action (to be implemented)
        Debug.Log("Opening Other Page...");
    }

    private IEnumerator PopulateItemsDelayed()
    {
        yield return null; // Wait one frame to ensure all UI elements are initialized
        PopulateItems();
    }

    private void NotifyItemCount()
    {
        int itemCount = itemCounts[currentItemType];
        PlayerPrefs.SetInt("TotalItemsCount", itemCount); // Save item count
        PlayerPrefs.Save(); 
        ItemCountChanged?.Invoke(itemCount); // Notify listeners
        Debug.Log($"ItemCount for {currentItemType}: {itemCount}");
    }

    public void PopulateItems()
    {
        int itemCount = PlayerPrefs.GetInt("TotalItemsCount", itemCounts[currentItemType]); // Get the actual number of items

        // Set resourcesPath based on currentItemType
        switch (currentItemType)
        {
            case ItemType.Equipment:
                resourcesPath = "UILoading/Equipment"; // Equipment path
                break;
            case ItemType.Gem:
                resourcesPath = "UILoading/Gem/Stone"; // Gem path
                break;
            case ItemType.Other:
                resourcesPath = "UILoading/Other"; // Other path
                break;
        }

        // Determine the correct contentPanel based on the active menu
        Transform contentPanel = null;

        if (menuController.IsMenuActive(0)) // Allies Menu
        {
            contentPanel = FindObjectOfType<AlliesBlockSetup>()?.contentPanel;
        }
        else if (menuController.IsMenuActive(1)) // Hero Menu
        {
            contentPanel = FindObjectOfType<HeroBlockSetup>()?.contentPanel;
        }

        // Iterate over each block in the contentPanel
        foreach (Transform block in contentPanel)
        {
            int blockIndex = block.GetSiblingIndex();

            // Fill only blocks for actual items
            if (blockIndex < itemCount)
            {
                // Generate random item details
                var (itemName, quantity) = GenerateRandomItemDetails();

                // Assign sprite to the Image based on resourcesPath
                Sprite itemSprite = Resources.Load<Sprite>($"{resourcesPath}/{itemName}");
                if (itemSprite != null)
                {
                    Image blockImage = block.Find("Image").GetComponent<Image>();
                    blockImage.sprite = itemSprite;
                    blockImage.color = Color.white; // Ensure the color is not transparent
                }
                else
                {
                    Debug.LogWarning($"Image not found for: {itemName}");
                }

                // Assign quantity to the Qnty child
                Transform qntyTransform = block.Find("Qnty");
                if (qntyTransform != null)
                {
                    TextMeshProUGUI qntyText = qntyTransform.GetComponent<TextMeshProUGUI>();
                    if (qntyText != null)
                    {
                        qntyText.text = quantity.ToString();
                    }
                }

                // Load part image for Gem items only
                if (currentItemType == ItemType.Gem)
                {
                    string[] equipmentParts = { "Helm", "Shoulder", "Chest", "Pants", "Gloves", "Boots" };
                    string partName = equipmentParts[Random.Range(0, equipmentParts.Length)]; // Pick a random part name
                    Sprite partSprite = Resources.Load<Sprite>($"UILoading/Gem/Part/{partName}");
                    if (partSprite != null)
                    {
                        Image partImage = block.Find("Part").GetComponent<Image>();
                        partImage.sprite = partSprite;
                        partImage.color = Color.white; // Ensure the color is not transparent
                    }
                    else
                    {
                        Debug.LogWarning($"Part image not found for: {partName}");
                    }
                }
            }
            else
            {
                // Clear the image and set it to transparent
                Transform imageTransform = block.Find("Image");
                if (imageTransform != null)
                {
                    Image blockImage = imageTransform.GetComponent<Image>();
                    if (blockImage != null)
                    {
                        blockImage.sprite = null;
                        blockImage.color = new Color(0, 0, 0, 0); // Set to transparent
                    }
                }

                // Clear quantity text
                Transform qntyTransform = block.Find("Qnty");
                if (qntyTransform != null)
                {
                    TextMeshProUGUI qntyText = qntyTransform.GetComponent<TextMeshProUGUI>();
                    if (qntyText != null)
                    {
                        qntyText.text = "";
                    }
                }

                // Clear part image if not Gem
                Transform partTransform = block.Find("Part");
                if (partTransform != null && currentItemType != ItemType.Gem)
                {
                    Image partImage = partTransform.GetComponent<Image>();
                    partImage.sprite = null; // Set to transparent
                    partImage.color = new Color(0, 0, 0, 0); // Set to fully transparent
                }
            }
        }
    }

    public (string itemName, int quantity) GenerateRandomItemDetails()
    {
        string itemName = "";
        int quantity = Random.Range(1, 10); // Quantity 1-9

        if (currentItemType == ItemType.Equipment)
        {
            string[] equipmentNames = { "Helm", "Shoulder", "Chest", "Pants", "Gloves", "Boots" };
            itemName = equipmentNames[Random.Range(0, equipmentNames.Length)];
            int itemLevel = Random.Range(1, 7); // Levels 1-6
            itemName = $"{itemName}_{itemLevel:D2}"; // Equipment name like "Helm_01"
        }
        else if (currentItemType == ItemType.Gem)
        {
            itemName = $"Gem_{Random.Range(1, 8):D2}"; // Gem name like "Gem_01", "Gem_02", ...
        }
        else if (currentItemType == ItemType.Other)
        {
            itemName = "OtherItem"; // Define a different name for "Other"
        }

        return (itemName, quantity);
    }
}
