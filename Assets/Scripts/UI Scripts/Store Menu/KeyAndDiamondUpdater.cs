using UnityEngine;
using TMPro;
using model; 

public class KeyAndDiamondUpdater : MonoBehaviour
{
    public TextMeshProUGUI diamondText;
    public TextMeshProUGUI heroKeyText;
    public TextMeshProUGUI rareKeyText;
    public TextMeshProUGUI epicKeyText;

    void Start()
    {
        // Subscribe to player data changes for automatic updates
        PlayerProfile.Data.AddListener(UpdatePlayerValues, "Player");
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
}
