using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using model;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;
using WebSocket;

public class OtherDetailBox : MonoBehaviour
{
    [SerializeField] private GameObject popup;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI methodText;
    [SerializeField] private TextMeshProUGUI itemQuantity;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private AllyStandPage allyStandPage; // Reference to celebration popup
    
    public static OtherDetailBox Instance;

    // Store current item info for action handling
    private string currentFileName;
    private int currentQuantity;
    private string currentType;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        PlayerProfile.Data.AddListener((arg0 => popup.SetActive(false)), "Bag");
    }

    public void Init(string fileName, int quantity, string type)
    {
        // Safety check: don't show detail box for items with quantity 0 or less
        if (quantity <= 0)
        {
            popup.SetActive(false);
            return;
        }

        currentFileName = fileName;
        currentQuantity = quantity;
        currentType = type;

        // Set the item icon
        Sprite itemSprite = LoadOtherItemSprite(fileName, type);
        if (itemSprite != null)
        {
            itemIcon.sprite = itemSprite;
            itemIcon.color = Color.white;
        }
        else
        {
            itemIcon.color = Color.clear;
        }

        // Set item details
        typeText.text = GetTypeText(type);
        nameText.text = GetDisplayName(fileName, type);
        descText.text = GetDescText(type);
        methodText.text = GetMethodText(type);
        itemQuantity.text = quantity.ToString();

        // Update action button
        UpdateActionButton(fileName, quantity, type);

        popup.SetActive(true);
    }

    private Sprite LoadOtherItemSprite(string fileName, string type)
    {
        string path = type == "skillbook"
            ? $"UILoading/CharacterImages/Skillbook/{fileName}"
            : $"UILoading/CharacterImages/Shard/{fileName}";
        return Resources.Load<Sprite>(path);
    }

    private string GetDisplayName(string fileName, string type)
    {
        if (type == "skillbook")
        {
            // For skillbooks: SKb_02_Gideon -> Gideon (extract the character name, not the index)
            string[] parts = fileName.Split('_');
            if (parts.Length >= 3 && parts[0] == "SKb")
            {
                // Return the character name (3rd part)
                return parts[2];
            }
            else
            {
                // Fallback to generic extraction
                return GetCharacterName(fileName);
            }
        }
        else if (type == "shard")
        {
            // For shards in "xx_name" format, extract and display character name only
            return GetCharacterName(fileName);
        }
        else
        {
            // For other items, return the fileName as-is
            return fileName;
        }
    }

    private string GetTypeText(string type)
    {
        if (type == "shard")
        {
            return "Shard";
        }
        else if (type == "skillbook")
        {
            return "Skillbook";
        }
        else
        {
            return type; // For other item types
        }
    }

    private string GetDescText(string type)
    {
        if (type == "shard")
        {
            return "Merge 10 to summon a unique ally (once only). Extras are used for star promotion.";
        }
        else if (type == "skillbook")
        {
            return "Used to level up your ally's skills.";
        }
        else
        {
            return "Item description"; // Default for other types
        }
    }

    private string GetMethodText(string type)
    {
        if (type == "shard")
        {
            return "Store";
        }
        else if (type == "skillbook")
        {
            return "Store, Main Quests, Dungeon Quests, Events";
        }
        else
        {
            return "Various methods"; // Default for other types
        }
    }

    private string GetCharacterName(string fileName)
    {
        // Extract character name from "xx_name" format
        string[] parts = fileName.Split('_');
        return parts.Length > 1 ? parts[1] : fileName;
    }

    private void UpdateActionButton(string fileName, int quantity, string type)
    {
        string characterName = GetCharacterName(fileName);
        string allyName = GetAllyNameFromFileName(fileName);
        bool hasBeenSummoned = HasCharacterBeenSummoned(allyName);

        if (type == "shard")
        {
            if (hasBeenSummoned)
            {
                // Character already summoned, show Utilize
                actionButtonText.text = "Utilize";
                actionButton.interactable = true;
                actionButton.onClick.RemoveAllListeners();
                actionButton.onClick.AddListener(() => OnUtilizeClicked());
                actionButton.gameObject.SetActive(true);
            }
            else if (quantity >= 10)
            {
                // Never summoned but enough shards to merge for first time
                actionButtonText.text = "Merge";
                actionButton.interactable = true;
                actionButton.onClick.RemoveAllListeners();
                actionButton.onClick.AddListener(() => OnMergeClicked());
                actionButton.gameObject.SetActive(true);
            }
            else
            {
                // Never summoned and not enough shards - hide button
                actionButton.gameObject.SetActive(false);
            }
        }
        else if (type == "skillbook")
        {
            if (hasBeenSummoned)
            {
                // Only show skillbook utilize if corresponding ally has been summoned
                actionButtonText.text = "Utilize";
                actionButton.interactable = true;
                actionButton.onClick.RemoveAllListeners();
                actionButton.onClick.AddListener(() => OnUtilizeClicked());
                actionButton.gameObject.SetActive(true);
            }
            else
            {
                // Hide skillbook button if ally never summoned
                actionButton.gameObject.SetActive(false);
            }
        }
        else
        {
            // For other item types - default behavior
            actionButtonText.text = "Utilize";
            actionButton.interactable = true;
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => OnUtilizeClicked());
            actionButton.gameObject.SetActive(true);
        }
    }

    // UPDATED: Now uses backend sidekick data to check if character was summoned
    private bool HasCharacterBeenSummoned(string characterName)
    {
        // Check both the full format (e.g., "10_Cedric") and character name only (e.g., "Cedric")
        bool isUnlocked = PlayerProfile.Data.IsAllyUnlocked(characterName);
        
        // Also check just the character name part if not found
        if (!isUnlocked)
        {
            string nameOnly = GetCharacterName(characterName);
            isUnlocked = PlayerProfile.Data.IsAllyUnlocked(nameOnly);
        }
        
        return isUnlocked;
    }

    private void OnMergeClicked()
    {
        string allyName = GetAllyNameFromFileName(currentFileName);
        string allyIndex = GetAllyIndexFromFileName(currentFileName);
        string allyDisplayName = GetAllyDisplayNameFromFileName(currentFileName);

        // Immediately show AllyStandPage and let it handle the summon
        if (allyStandPage != null)
        {
            allyStandPage.ShowAllyStandPage(allyDisplayName, allyIndex, allyName);
        }
    }
    
    private string GetAllyNameFromFileName(string fileName)
    {
        // Extract ally name from file name to match the summoned allies format
        if (fileName.StartsWith("SKb_"))
        {
            // For skillbooks: SKb_02_Gideon → 02_Gideon (to match summoned allies format)
            string[] parts = fileName.Split('_');
            if (parts.Length >= 3)
            {
                return $"{parts[1]}_{parts[2]}"; // "02_Gideon"
            }
        }
        
        // For shards: "10_Cedric" → "10_Cedric" (already in correct format)
        string allyName = fileName.Replace("_shard", "");
        return allyName;
    }
    
    private string GetAllyIndexFromFileName(string fileName)
    {
        // Extract ally index from file name (e.g., "10_Cedric" -> "10")
        string fileNameWithoutShard = fileName.Replace("_shard", "");
        string[] parts = fileNameWithoutShard.Split('_');
        return parts.Length > 0 ? parts[0] : "01"; // Default to "01" if parsing fails
    }
    
    private string GetAllyDisplayNameFromFileName(string fileName)
    {
        // Extract ally display name from file name (e.g., "10_Cedric" -> "Cedric")
        string fileNameWithoutShard = fileName.Replace("_shard", "");
        string[] parts = fileNameWithoutShard.Split('_');
        return parts.Length > 1 ? parts[1] : "Unknown"; // Default to "Unknown" if parsing fails
    }
    
    public void RefreshDetailBox()
    {
        // Refresh the current display to show updated shard count and button state
        if (!string.IsNullOrEmpty(currentFileName) && !string.IsNullOrEmpty(currentType))
        {
            var player = PlayerProfile.Data.Player;
            // Use the ally name (without _shard suffix) as the key
            string allyKey = GetAllyNameFromFileName(currentFileName);
            int quantity = player?.ItemsJson.TryGetValue(allyKey, out int count) ?? false ? count : 0;
            
            // If quantity is 0 or less, close the detail box and refresh the pack UI
            if (quantity <= 0)
            {
                popup.SetActive(false);
                
                // Trigger pack UI refresh to remove the item from the pack
                PlayerProfile.Data.NotifyListeners("OtherItems");
                return;
            }
            
            Init(currentFileName, quantity, currentType);
        }
    }

    private void OnUtilizeClicked()
    {
        
        // Step 1: Close OtherDetailBox (this popup)
        popup.SetActive(false);
        
        // Step 2: Find MenuController and switch to Allies Menu first
        MenuController menuController = FindObjectOfType<MenuController>();
        if (menuController != null)
        {
            // Extract ally info from the current shard/skillbook filename BEFORE switching menus
            string allyIndex;
            string allyName;
            
            // Special handling for skillbook format in utilize flow
            if (currentType == "skillbook" && currentFileName.StartsWith("SKb_"))
            {
                // For skillbooks: SKb_05_Lyanna -> index="05", name="Lyanna"
                string[] parts = currentFileName.Split('_');
                allyIndex = parts.Length >= 2 ? parts[1] : "01";
                allyName = parts.Length >= 3 ? parts[2] : "Unknown";
            }
            else
            {
                // For shards and other formats, use the existing methods
                allyIndex = GetAllyIndexFromFileName(currentFileName);
                allyName = GetAllyDisplayNameFromFileName(currentFileName);
            }
            
            // Activate Allies Menu first by clicking the button
            if (menuController.buttons != null && menuController.buttons.Length > 0)
            {
                // Simulate clicking the Allies Menu button to ensure proper UI state
                menuController.buttons[0].onClick.Invoke();
                
                // 🔹 Use minimal delay to reduce visible pack UI transition
                // This prevents timing conflicts with UI state management
                StartCoroutine(DelayedUtilizeStep2(allyIndex, allyName, currentType));
            }
        }
    }

    private IEnumerator DelayedUtilizeStep2(string allyIndex, string allyName, string itemType)
    {
        // Minimal wait time to reduce pack UI visibility
        yield return new WaitForSeconds(0.02f); // Reduced from 0.05f to 0.02f
        
        // Now find AlliesGridSetup and proceed with Step 2
        AlliesGridSetup alliesGridSetup = FindObjectOfType<AlliesGridSetup>();
        if (alliesGridSetup != null)
        {
            // Pass the item type to ensure correct button mode is set (shard->StarUp, skillbook->LevelUp)
            alliesGridSetup.OpenUtilizeStep2Simple(allyIndex, allyName, itemType);
        }
    }
}
