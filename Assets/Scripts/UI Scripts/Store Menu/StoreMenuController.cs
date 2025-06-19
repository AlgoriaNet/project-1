using UnityEngine;
using model;

public class StoreMenuController : MonoBehaviour
{
    // Panel references for x1 and x10 draws for each chest
    public GameObject x1heroKeyPanel;
    public GameObject x1heroDiamondPanel;
    public GameObject x10heroKeyPanel;
    public GameObject x10heroDiamondPanel;

    public GameObject x1rareKeyPanel;
    public GameObject x1rareDiamondPanel;
    public GameObject x10rareKeyPanel;
    public GameObject x10rareDiamondPanel;

    public GameObject x1epicKeyPanel;
    public GameObject x1epicDiamondPanel;
    public GameObject x10epicKeyPanel;
    public GameObject x10epicDiamondPanel;

    void Start()
    {
        // Subscribe to player data changes for automatic updates
        PlayerProfile.Data.AddListener(UpdatePlayerValues, "Player");

        // Initial panel update
        var player = PlayerProfile.Data.Player; // Get player profile
        if (player != null)
        {
            UpdatePanels(player); // Pass the player to update panels
        }
    }

    private void UpdatePlayerValues(ApplicationModel model)
    {
        // Fetch the Player object from PlayerProfile
        var player = PlayerProfile.Data.Player;

        if (player == null) return;

        // Update the panels after key value change
        UpdatePanels(player);
    }

    private void UpdatePanels(Player player)
    {
        // Get the key counts from ItemsJson
        int heroKeyCount = player.ItemsJson.TryGetValue("heroKey", out int heroKey) ? heroKey : 0;
        int rareKeyCount = player.ItemsJson.TryGetValue("rareKey", out int rareKey) ? rareKey : 0;
        int epicKeyCount = player.ItemsJson.TryGetValue("epicKey", out int epicKey) ? epicKey : 0;

        // Update Hero Chest panels
        UpdateChestPanel(heroKeyCount, x1heroKeyPanel, x1heroDiamondPanel, x10heroKeyPanel, x10heroDiamondPanel, 1, 10);

        // Update Rare Chest panels
        UpdateChestPanel(rareKeyCount, x1rareKeyPanel, x1rareDiamondPanel, x10rareKeyPanel, x10rareDiamondPanel, 1, 10);

        // Update Epic Chest panels
        UpdateChestPanel(epicKeyCount, x1epicKeyPanel, x1epicDiamondPanel, x10epicKeyPanel, x10epicDiamondPanel, 1, 10);
    }

    private void UpdateChestPanel(int keyCount, GameObject x1KeyPanel, GameObject x1DiamondPanel, GameObject x10KeyPanel, GameObject x10DiamondPanel, int requiredFor1, int requiredFor10)
    {
        // For x1 draw: Show key panel if enough keys, otherwise show diamond panel
        x1KeyPanel.SetActive(keyCount >= requiredFor1);
        x1DiamondPanel.SetActive(keyCount < requiredFor1);

        // For x10 draw: Show key panel if enough keys, otherwise show diamond panel
        x10KeyPanel.SetActive(keyCount >= requiredFor10);
        x10DiamondPanel.SetActive(keyCount < requiredFor10);
    }

    private void OnDestroy()
    {
        if (PlayerProfile.Data != null)
        {
            PlayerProfile.Data.RemoveListener(UpdatePlayerValues, "Player");
        }
    }
}