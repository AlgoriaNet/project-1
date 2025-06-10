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
    public TextMeshProUGUI index2Text;
    public TextMeshProUGUI index3Text;
    public TextMeshProUGUI index4Text;

    void Start()
    {
        // Subscribe to player data changes for automatic updates
        PlayerProfile.Data.AddListener(UpdatePlayerValues, "Player");
        
        // Initialize values if player data is already available
        UpdatePlayerValues(PlayerProfile.Data);
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
        // TODO: Update other index values when they're defined in the Player model
        // For now, these are placeholders - replace with actual Player properties when available
        if (index2Text != null) 
            index2Text.text = "0"; // Replace with player.Key1 or relevant property
        
        if (index3Text != null) 
            index3Text.text = "0"; // Replace with player.Key2 or relevant property
        
        if (index4Text != null) 
            index4Text.text = "0"; // Replace with player.Key3 or relevant property
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
