using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    public Transform indexGroup; // Assign the IndexGroup transform in the Inspector
    
    // Direct references to text components
    [SerializeField] private TextMeshProUGUI powerValueText;
    [SerializeField] private TextMeshProUGUI gemValueText;
    [SerializeField] private TextMeshProUGUI goldValueText;

    private const string powerKey = "PlayerPower";
    private const string gemKey = "PlayerGems";
    private const string goldKey = "PlayerGold";

    private int initialPower = 30;
    private int initialGem = 1452;
    private int initialGold = 300;

    void Start()
    {
        // Initialize PlayerPrefs if not set
        if (!PlayerPrefs.HasKey(powerKey)) PlayerPrefs.SetInt(powerKey, initialPower);
        if (!PlayerPrefs.HasKey(gemKey)) PlayerPrefs.SetInt(gemKey, initialGem);
        if (!PlayerPrefs.HasKey(goldKey)) PlayerPrefs.SetInt(goldKey, initialGold);
        PlayerPrefs.Save();

        // Find references if not already assigned
        FindTextReferences();
        
        // Update UI with values
        UpdateResourceUI();
    }
    
    private void FindTextReferences()
    {
        if (indexGroup == null)
        {
            Debug.LogError("IndexGroup is not assigned!");
            return;
        }
        
        if (powerValueText == null)
        {
            Transform power = indexGroup.Find("Power");
            if (power != null)
            {
                powerValueText = power.Find("ValueText")?.GetComponent<TextMeshProUGUI>();
                if (powerValueText == null) Debug.LogError("PowerValueText not found!");
            }
            else Debug.LogError("Power object not found!");
        }
        
        if (gemValueText == null)
        {
            Transform gem = indexGroup.Find("Gem");
            if (gem != null)
            {
                gemValueText = gem.Find("ValueText")?.GetComponent<TextMeshProUGUI>();
                if (gemValueText == null) Debug.LogError("GemValueText not found!");
            }
            else Debug.LogError("Gem object not found!");
        }
        
        if (goldValueText == null)
        {
            Transform gold = indexGroup.Find("Gold");
            if (gold != null)
            {
                goldValueText = gold.Find("ValueText")?.GetComponent<TextMeshProUGUI>();
                if (goldValueText == null) Debug.LogError("GoldValueText not found!");
            }
            else Debug.LogError("Gold object not found!");
        }
    }

    public void UpdateResourceUI()
    {
        // Get current values
        int currentPower = PlayerPrefs.GetInt(powerKey, initialPower);
        int currentGems = PlayerPrefs.GetInt(gemKey, initialGem);
        int currentGold = PlayerPrefs.GetInt(goldKey, initialGold);
        
        Debug.Log($"Updating UI - Power: {currentPower}, Gems: {currentGems}, Gold: {currentGold}");
        
        // Find references if needed
        if (powerValueText == null || gemValueText == null || goldValueText == null)
        {
            FindTextReferences();
        }
        
        // Update texts with proper error checking
        if (powerValueText != null)
        {
            powerValueText.text = $"{currentPower}/30";
            Debug.Log($"Power text updated to: {powerValueText.text}");
        }
        
        if (gemValueText != null)
        {
            gemValueText.text = currentGems.ToString();
            Debug.Log($"Gem text updated to: {gemValueText.text}");
        }
        
        if (goldValueText != null)
        {
            goldValueText.text = currentGold.ToString();
            Debug.Log($"Gold text updated to: {goldValueText.text}");
        }
    }

    public void UpdateGemCount(int delta)
    {
        int currentGems = PlayerPrefs.GetInt(gemKey, initialGem);
        currentGems += delta;
        PlayerPrefs.SetInt(gemKey, currentGems);
        PlayerPrefs.Save();
        Debug.Log($"Updated gem count: {currentGems} (delta: {delta})");
        UpdateResourceUI();
    }

    public int GetGemCount()
    {
        return PlayerPrefs.GetInt(gemKey, initialGem);
    }
}