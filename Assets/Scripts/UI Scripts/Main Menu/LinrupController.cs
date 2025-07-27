using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using model;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;

public class LineupController : MonoBehaviour
{
    public GameObject lineupPopup;
    public GridLayoutGroup grid;
    public RectTransform contentPanel;
    public GameObject allyItemPrefab;
    public int blocksPerRow = 5;

    private float blockWidth;
    private float nameTextWidth;
    private float nameTextHeight;
    private float starGroupWidth;
    private float starGroupHeight;
    private float skillIconWidth;

    private List<(string index, string name)> unlockedAllies;
    private Dictionary<string, int> starLevels;
    
    private GameObject selectedAlly = null;
    private HashSet<GameObject> officiallySelectedAllies = new HashSet<GameObject>();
    private Dictionary<Button, GameObject> lineupDictionary = new Dictionary<Button, GameObject>();

    public Button slot1, slot2, slot3, slot4;

    // Track original deployment state - KEY FIX: Store by ally index, not sidekick id
    private Dictionary<string, bool> originalDeployment = new Dictionary<string, bool>();

    void Start()
    {
        PlayerProfile.Data.AddListener(OnPlayerDataLoaded, "Player");
        
        if (PlayerProfile.Data.Player != null)
        {
            LoadAlliesData();
            AdjustGridForFivePerRow();
            LoadAllyItems();
        }
    }
    
    private void OnDestroy()
    {
        PlayerProfile.Data.RemoveListener(OnPlayerDataLoaded, "Player");
    }
    
    private void OnPlayerDataLoaded(ApplicationModel model)
    {
        LoadAlliesData();
        AdjustGridForFivePerRow();
        LoadAllyItems();
    }

    private void LoadAlliesData()
    {
        string[] characterNames = {
            "Zorath", "Gideon", "Sylas", "Aurelia", "Lyanna", "Zhara", "Elenya", "Rowan",
            "Liraen", "Cedric", "Selena", "Morgath", "Zyphira", "Kaelith", "Velan", "Ragnar",
            "Lucien", "Ugra", "Eleanor", "Nyx"
        };

        List<string> unlockedIcons = GetUnlockedAllyIndices(characterNames);
        starLevels = new Dictionary<string, int>();

        unlockedAllies = new List<(string index, string name)>();
        for (int i = 0; i < characterNames.Length; i++)
        {
            string index = (i + 1).ToString("D2");
            if (unlockedIcons.Contains(index))
            {
                unlockedAllies.Add((index, characterNames[i]));
            }
        }
    }

    private void AdjustGridForFivePerRow()
    {
        float panelWidth = contentPanel.rect.width;
        if (panelWidth == 0)
        {
            bool wasActive = lineupPopup.activeSelf;
            if (!wasActive)
            {
                lineupPopup.SetActive(true);
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel);
                panelWidth = contentPanel.rect.width;
                lineupPopup.SetActive(false);
            }
        }

        blockWidth = panelWidth / 5.75f;
        nameTextWidth   = blockWidth * 0.6f;
        nameTextHeight  = blockWidth * 0.12f;
        starGroupWidth  = blockWidth * 0.5f;
        starGroupHeight = blockWidth * 0.1f;
        skillIconWidth  = blockWidth * 0.2f;

        grid.cellSize = new Vector2(blockWidth, blockWidth * 1.342f);

        float leftPadding = blockWidth / 4f;
        float spacingX = leftPadding / 4f;
        float spacingY = spacingX;
        grid.padding.left = Mathf.RoundToInt(leftPadding);
        grid.padding.right = Mathf.RoundToInt(leftPadding);
        grid.padding.top = Mathf.RoundToInt(leftPadding * 1.2f);
        grid.padding.bottom = Mathf.RoundToInt(leftPadding * 1.2f);
        grid.spacing = new Vector2(spacingX, spacingY);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = blocksPerRow;
    }

    private void LoadAllyItems()
    {
        foreach (Transform child in grid.transform)
        {
            if (child.name.StartsWith("AllyItem_"))
            {
                Destroy(child.gameObject);
            }
        }
        
        foreach (var ally in unlockedAllies)
        {
            GameObject newAlly = Instantiate(allyItemPrefab, grid.transform, false);
            newAlly.name = $"AllyItem_{ally.index}";

            // Setup components (same as before)
            TextMeshProUGUI nameText = newAlly.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            nameText.text = ally.name;
            RectTransform nameTextRect = nameText.GetComponent<RectTransform>();
            nameTextRect.sizeDelta = new Vector2(nameTextWidth, nameTextHeight);
            nameTextRect.anchoredPosition = new Vector2(nameTextRect.anchoredPosition.x, starGroupHeight + 13);

            Image allyImage = newAlly.transform.Find("AllyImage").GetComponent<Image>();
            string imagePath = $"UILoading/CharacterImages/CardDisplay/C_{ally.index}_{ally.name}";
            Sprite allySprite = Resources.Load<Sprite>(imagePath);
            if (allySprite != null)
            {
                allyImage.sprite = allySprite;
            }

            Image skillIcon = newAlly.transform.Find("SkillIcon").GetComponent<Image>();
            RectTransform skillIconRect = skillIcon.GetComponent<RectTransform>();
            skillIconRect.sizeDelta = new Vector2(skillIconWidth, skillIconRect.sizeDelta.y);

            Transform starGroup = newAlly.transform.Find("StarGroup");
            RectTransform starGroupRect = starGroup.GetComponent<RectTransform>();
            starGroupRect.sizeDelta = new Vector2(starGroupWidth, starGroupHeight);

            GridLayoutGroup starGrid = starGroup.GetComponent<GridLayoutGroup>();
            if (starGrid != null)
            {
                starGrid.cellSize = new Vector2(starGroupHeight, starGroupHeight);
            }

            int starLevel = starLevels.ContainsKey(ally.index) ? starLevels[ally.index] : 0;
            for (int i = 1; i <= 5; i++)
            {
                Transform starYellow = starGroup.Find($"Star{i}/yellow");
                if (starYellow != null)
                {
                    starYellow.gameObject.SetActive(i <= starLevel);
                }
            }

            Button allyButton = newAlly.GetComponent<Button>();
            if (allyButton == null)
            {
                allyButton = newAlly.AddComponent<Button>();
            }
            allyButton.onClick.AddListener(() => SelectAlly(newAlly));
        }
    }

    private void SelectAlly(GameObject allyItem)
    {
        if (lineupDictionary.Count >= 4)
        {
            Debug.LogWarning("❌ Lineup is full! You cannot select more allies.");
            return;
        }

        if (officiallySelectedAllies.Contains(allyItem))
        {
            Debug.LogWarning("❌ This ally is already deployed in a slot and cannot be selected again.");
            return;
        }

        if (selectedAlly == allyItem) return;

        if (selectedAlly != null && !officiallySelectedAllies.Contains(selectedAlly))
        {
            SetAllyColor(selectedAlly, Color.white);
        }

        selectedAlly = allyItem;
        SetAllyColor(selectedAlly, new Color(0.4f, 0.4f, 0.4f, 1f));
    }

    private void SetAllyColor(GameObject allyItem, Color color)
    {
        if (allyItem == null) return;

        Image allyImage = allyItem.transform.Find("AllyImage")?.GetComponent<Image>();
        if (allyImage != null) allyImage.color = color;

        TextMeshProUGUI nameText = allyItem.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null) nameText.color = color;

        Image skillIcon = allyItem.transform.Find("SkillIcon")?.GetComponent<Image>();
        if (skillIcon != null) skillIcon.color = color;

        Transform starGroup = allyItem.transform.Find("StarGroup");
        if (starGroup != null)
        {
            foreach (Transform star in starGroup)
            {
                Image whiteStar = star.Find("white")?.GetComponent<Image>();
                Image yellowStar = star.Find("yellow")?.GetComponent<Image>();

                if (whiteStar != null) whiteStar.color = color;
                if (yellowStar != null) yellowStar.color = color;
            }
        }
    }

    public void EnableBattleIcon(Button slotButton)
    {
        if (lineupDictionary.ContainsKey(slotButton))
        {
            GameObject assignedAlly = lineupDictionary[slotButton];
            Transform battleIcon = slotButton.transform.Find("battleIcon");
            if (battleIcon != null) battleIcon.gameObject.SetActive(false);
            SetAllyColor(assignedAlly, Color.white);
            officiallySelectedAllies.Remove(assignedAlly);
            lineupDictionary.Remove(slotButton);
            
            // Clear selectedAlly if it's the same as the removed ally
            if (selectedAlly == assignedAlly)
            {
                selectedAlly = null;
            }
            
            // ...
            return;
        }

        if (selectedAlly == null)
        {
            Debug.LogWarning("❌ No ally selected! Select an AllyItem before clicking a slot.");
            return;
        }

        if (officiallySelectedAllies.Contains(selectedAlly))
        {
            Debug.LogWarning("❌ This ally is already deployed in another slot.");
            return;
        }

        string allyIndex = selectedAlly.name.Split('_')[1];
        string allyName = selectedAlly.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text;

        string battleIconPath = $"UILoading/CharacterImages/UserIcons/BattlYellowIcons/L_{allyIndex}_{allyName}";
        Sprite battleIconSprite = Resources.Load<Sprite>(battleIconPath);
        if (battleIconSprite == null)
        {
            Debug.LogWarning($"❌ Battle icon not found at path: {battleIconPath}");
            return;
        }

        Transform slotBattleIcon = slotButton.transform.Find("battleIcon");
        if (slotBattleIcon != null)
        {
            slotBattleIcon.gameObject.SetActive(true);
            Image slotBattleIconImage = slotBattleIcon.GetComponent<Image>();
            if (slotBattleIconImage != null)
            {
                slotBattleIconImage.sprite = battleIconSprite;
                slotBattleIconImage.color = Color.white;
            }
        }

        if (!officiallySelectedAllies.Contains(selectedAlly))
        {
            officiallySelectedAllies.Add(selectedAlly);
        }

        lineupDictionary[slotButton] = selectedAlly;

        // Clear selectedAlly after successful assignment
        selectedAlly = null;
    }

    public void CloseLineup()
    {
        lineupPopup.SetActive(false);
        SaveLineup();
        
        var currentLineup = lineupDictionary.Values.Select(go => go.name.Split('_')[1]).ToList();
        // ...
        
        UpdateDeploymentToBackend();
    }

    // ADD THIS NEW METHOD - completely new
    private string GetBaseIdFromAllyIndex(string allyIndex)
    {
        if (PlayerProfile.Data?.Player?.Sidekicks != null)
        {
            foreach (var sidekick in PlayerProfile.Data.Player.Sidekicks)
            {
                // Match ally index to sidekick - adjust this logic based on your data structure
                if (sidekick.id == allyIndex || sidekick.base_id == allyIndex)
                {
                    return sidekick.base_id;
                }
            }
        }
        
        // Fallback: return the ally index if no match found
        Debug.LogWarning($"No sidekick found for ally index: {allyIndex}");
        return allyIndex;
    }

    // REPLACE THE EXISTING UpdateDeploymentToBackend METHOD with this:
    private void UpdateDeploymentToBackend()
    {
        // Get current lineup ally indices
        List<string> currentlyDeployedIndices = lineupDictionary.Values
            .Select(go => go.name.Split('_')[1])
            .ToList();
        
        // Convert ally indices to base_ids from actual sidekick records
        List<string> currentlyDeployedBaseIds = new List<string>();
        
        foreach (string allyIndex in currentlyDeployedIndices)
        {
            if (PlayerProfile.Data?.Player?.Sidekicks != null)
            {
                var matchingSidekick = PlayerProfile.Data.Player.Sidekicks.FirstOrDefault(s => 
                    s.id == allyIndex || s.base_id == allyIndex || 
                    s.id == allyIndex.TrimStart('0') || s.base_id == allyIndex.TrimStart('0'));
                
                if (matchingSidekick != null)
                {
                    // Convert zero-padded base_id to non-zero-padded format for backend
                    string originalBaseId = matchingSidekick.base_id;
                    string baseId = originalBaseId;
                    if (int.TryParse(originalBaseId, out int numericId))
                    {
                        baseId = numericId.ToString(); // Remove zero-padding
                    }
                    currentlyDeployedBaseIds.Add(baseId);
                    // ...
                }
                else
                {
                    Debug.LogWarning($"[LineupController] No sidekick found for ally index {allyIndex}");
                    // Convert ally index to non-zero-padded format as fallback
                    string fallbackBaseId = allyIndex;
                    if (int.TryParse(allyIndex, out int numericId))
                    {
                        fallbackBaseId = numericId.ToString(); // Remove zero-padding
                    }
                    currentlyDeployedBaseIds.Add(fallbackBaseId);
                    // ...
                }
            }
        }

        // Get originally deployed base_ids for comparison
        List<string> originallyDeployedBaseIds = new List<string>();
        foreach (var kvp in originalDeployment)
        {
            if (kvp.Value) // if was deployed
            {
                var matchingSidekick = PlayerProfile.Data.Player.Sidekicks?.FirstOrDefault(s => 
                    s.id == kvp.Key || s.base_id == kvp.Key || 
                    s.id == kvp.Key.TrimStart('0') || s.base_id == kvp.Key.TrimStart('0'));
                
                if (matchingSidekick != null)
                {
                    // Convert zero-padded base_id to non-zero-padded format for backend
                    string originalBaseId = matchingSidekick.base_id;
                    string baseId = originalBaseId;
                    if (int.TryParse(originalBaseId, out int numericId))
                    {
                        baseId = numericId.ToString(); // Remove zero-padding
                    }
                    originallyDeployedBaseIds.Add(baseId);
                }
            }
        }

        // ...

        // Check if there are any changes
        bool hasChanges = !currentlyDeployedBaseIds.SequenceEqual(originallyDeployedBaseIds.OrderBy(x => x).ToList()) ||
                        currentlyDeployedBaseIds.Count != originallyDeployedBaseIds.Count;

        if (hasChanges)
        {
            SendCompleteDeploymentUpdate(currentlyDeployedBaseIds);
        }
    }

    // REPLACE THE EXISTING SendCompleteDeploymentUpdate METHOD with this:
    private void SendCompleteDeploymentUpdate(List<string> deployedBaseIds)
    {
        // ...
        
        if (WebSocketManager.Instance != null)
        {
            // ...
            WebSocketManager.Instance.Action(
                "PlayerChannel",
                "update_sidekick_deployment",
                new {
                    deployed_ids = deployedBaseIds.ToArray()  // Now sending actual base_ids from sidekick records
                }
            );
            // ...
        }
        else
        {
            Debug.LogError("[LineupController][AGENT] WebSocketManager.Instance is null! Cannot send deployment update.");
        }
    }

    private bool AreAllSlotsEmpty()
    {
        return !slot1.transform.Find("battleIcon").gameObject.activeSelf &&
            !slot2.transform.Find("battleIcon").gameObject.activeSelf &&
            !slot3.transform.Find("battleIcon").gameObject.activeSelf &&
            !slot4.transform.Find("battleIcon").gameObject.activeSelf;
    }

    private void LoadLineup()
    {
        if (!PlayerPrefs.HasKey("SavedLineup")) return;

        string savedData = PlayerPrefs.GetString("SavedLineup");
        string[] savedAllies = savedData.Split(',');

        // ...

        foreach (string entry in savedAllies)
        {
            string[] parts = entry.Split(':');
            if (parts.Length != 2) continue;

            string slotName = parts[0];
            string allyIndex = parts[1];

            GameObject allyItem = GameObject.Find($"AllyItem_{allyIndex}");
            if (allyItem == null)
            {
                Debug.LogWarning($"❌ AllyItem_{allyIndex} not found in scene!");
                continue;
            }

            Button slotButton = null;
            if (slotName == "Slot_1") slotButton = slot1;
            if (slotName == "Slot_2") slotButton = slot2;
            if (slotName == "Slot_3") slotButton = slot3;
            if (slotName == "Slot_4") slotButton = slot4;

            if (slotButton != null)
            {
                selectedAlly = allyItem;
                EnableBattleIcon(slotButton);
                officiallySelectedAllies.Add(allyItem);
                SetAllyColor(allyItem, new Color(0.4f, 0.4f, 0.4f, 1f));
            }
        }
    }

    private void SaveLineup()
    {
        List<string> lineupData = new List<string>();

        foreach (var slot in lineupDictionary.Keys)
        {
            GameObject ally = lineupDictionary[slot];
            string allyIndex = ally.name.Split('_')[1];
            lineupData.Add($"{slot.name}:{allyIndex}");
        }

        PlayerPrefs.SetString("SavedLineup", string.Join(",", lineupData));
        PlayerPrefs.Save();

        // ...
    }

    private bool IsAllyUnlocked(string allyName)
    {
        if (PlayerProfile.Data?.Player == null)
        {
            return false;
        }
        
        return PlayerProfile.Data.IsAllyUnlocked(allyName);
    }

    private List<string> GetUnlockedAllyIndices(string[] characterNames)
    {
        List<string> unlockedIndices = new List<string>();
        for (int i = 0; i < characterNames.Length; i++)
        {
            string index = (i + 1).ToString("D2");
            string allyName = characterNames[i];
            string fullKey = $"{index}_{allyName}";
            
            if (IsAllyUnlocked(fullKey) || IsAllyUnlocked(allyName))
            {
                unlockedIndices.Add(index);
            }
        }
        return unlockedIndices;
    }

    public void OpenLineup()
    {
        lineupPopup.SetActive(true);

        // FIXED: Track original deployment state by ally index (not sidekick id)
        originalDeployment.Clear();
        
        // Store currently deployed allies from the lineup dictionary
        foreach (var kvp in lineupDictionary)
        {
            GameObject allyItem = kvp.Value;
            string allyIndex = allyItem.name.Split('_')[1];
            originalDeployment[allyIndex] = true;
        }

        // Also check if we can get deployment state from backend data
        if (PlayerProfile.Data?.Player?.Sidekicks != null)
        {
            foreach (var sidekick in PlayerProfile.Data.Player.Sidekicks)
            {
                if (!originalDeployment.ContainsKey(sidekick.id))
                {
                    originalDeployment[sidekick.id] = sidekick.is_deployed;
                }
            }
        }

        // ...

        // Remove old listeners and add new ones
        slot1.onClick.RemoveAllListeners();
        slot2.onClick.RemoveAllListeners();
        slot3.onClick.RemoveAllListeners();
        slot4.onClick.RemoveAllListeners();

        slot1.onClick.AddListener(() => EnableBattleIcon(slot1));
        slot2.onClick.AddListener(() => EnableBattleIcon(slot2));
        slot3.onClick.AddListener(() => EnableBattleIcon(slot3));
        slot4.onClick.AddListener(() => EnableBattleIcon(slot4));

        if (AreAllSlotsEmpty())
        {
            LoadLineup();
        }
        
        // Ensure allies in slots are dimmed in the grid
        foreach (var kvp in lineupDictionary)
        {
            GameObject allyInSlot = kvp.Value;
            if (allyInSlot != null)
            {
                SetAllyColor(allyInSlot, new Color(0.4f, 0.4f, 0.4f, 1f));
                if (!officiallySelectedAllies.Contains(allyInSlot))
                {
                    officiallySelectedAllies.Add(allyInSlot);
                }
            }
        }
    }

    public void AutoDeploy()
    {
        // Clear all current selections and slots first
        ClearAllSelections();
        
        // Get first 4 available allies
        var availableAllies = unlockedAllies.Take(4).ToList();
        var slots = new List<Button> { slot1, slot2, slot3, slot4 };
        
        for (int i = 0; i < Math.Min(availableAllies.Count, slots.Count); i++)
        {
            string allyObjName = $"AllyItem_{availableAllies[i].index}";
            GameObject allyItem = GameObject.Find(allyObjName);
            if (allyItem != null)
            {
                selectedAlly = allyItem;
                EnableBattleIcon(slots[i]);
                // Ensure ally is marked as officially selected and dimmed
                if (!officiallySelectedAllies.Contains(allyItem))
                {
                    officiallySelectedAllies.Add(allyItem);
                }
                SetAllyColor(allyItem, new Color(0.4f, 0.4f, 0.4f, 1f));
            }
        }
        
        // ...
    }
    
    private void ClearAllSelections()
    {
        // Clear manually selected ally that's not in any slot
        if (selectedAlly != null && !officiallySelectedAllies.Contains(selectedAlly))
        {
            SetAllyColor(selectedAlly, Color.white);
        }
        
        // Clear all slots
        var slots = new List<Button> { slot1, slot2, slot3, slot4 };
        foreach (var slot in slots)
        {
            if (lineupDictionary.ContainsKey(slot))
            {
                GameObject assignedAlly = lineupDictionary[slot];
                Transform battleIcon = slot.transform.Find("battleIcon");
                if (battleIcon != null) battleIcon.gameObject.SetActive(false);
                SetAllyColor(assignedAlly, Color.white);
                officiallySelectedAllies.Remove(assignedAlly);
            }
        }
        
        // Clear all collections
        lineupDictionary.Clear();
        officiallySelectedAllies.Clear();
        selectedAlly = null;
        
        // ...
    }
}