using UnityEngine;
using TMPro;
using model; 

public class IndexDynamicSize : MonoBehaviour
{
    public RectTransform topPanel;       // User topPanel RectTransform
    public RectTransform indexGroup;     // Index Group RectTransform
    public RectTransform index1;         // Index 1 RectTransform
    public RectTransform index2;         // Index 2 RectTransform
    public RectTransform index3;         // Index 3 RectTransform
    public RectTransform index4;         // Index 3 RectTransform

    public TextMeshProUGUI diamondText;
    public TextMeshProUGUI heroKeyText;
    public TextMeshProUGUI rareKeyText;
    public TextMeshProUGUI epicKeyText;

    void Start()
    {
        // Subscribe to player data changes for automatic updates
        PlayerProfile.Data.AddListener(UpdatePlayerValues, "Player");
        
        // Dynamically adjust the layout
        AdjustLayout();
    }

    // This method will be called automatically whenever PlayerProfile data changes
    private void UpdatePlayerValues(ApplicationModel model)
    {    
        var playerProfile = model as PlayerProfile;
        var player = playerProfile?.Player;

        if (player == null) return;

        // Update diamond value
        if (diamondText != null) 
            diamondText.text = player.Diamond.ToString();
        
        if (heroKeyText != null)
            heroKeyText.text = player.ItemsJson.TryGetValue("heroKey", out int key1) ? key1.ToString() : "0";

        if (rareKeyText != null)
            rareKeyText.text = player.ItemsJson.TryGetValue("rareKey", out int key2) ? key2.ToString() : "0";

        if (epicKeyText != null)
            epicKeyText.text = player.ItemsJson.TryGetValue("epicKey", out int key3) ? key3.ToString() : "0";
    }

    // Unsubscribe when the object is destroyed to prevent memory leaks
    private void OnDestroy()
    {
        if (PlayerProfile.Data != null)
        {
            PlayerProfile.Data.RemoveListener(UpdatePlayerValues, "Player");
        }
    }

    private void AdjustLayout()
    {
        float panelWidth = topPanel.rect.width; // Use the rendered width of the Top Panel

        float indexGroupWidth = panelWidth * 0.8f; // Remaining space after user
        float indexWidth = indexGroupWidth / 8f;     // Divide equally among indexes
        float indexGroupHeight = indexGroup.rect.height / 2f;

        // Set Index Group size
        indexGroup.sizeDelta = new Vector2(indexGroupWidth, indexGroupHeight);

        // Set Index sizes dynamically
        index1.sizeDelta = new Vector2(indexWidth, indexGroupHeight);
        index2.sizeDelta = new Vector2(indexWidth, indexGroupHeight);
        index3.sizeDelta = new Vector2(indexWidth, indexGroupHeight);
        index4.sizeDelta = new Vector2(indexWidth, indexGroupHeight);
    }
}