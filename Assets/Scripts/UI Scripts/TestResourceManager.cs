using UnityEngine;
using model;

public class TestResourceManager : MonoBehaviour
{
    // Test method to add diamonds and keys
    public void AddTestResources()
    {
        var player = PlayerProfile.Data.Player;
        if (player == null)
        {
            Debug.LogError("❌ Player data is null!");
            return;
        }

        // Modify diamond count
        player.Diamond = 10000; // Set to 10,000 diamonds for testing

        // Modify items_json for keys
        var itemsJson = player.ItemsJson;
        itemsJson["heroKey"] = 100;  // Set heroKey to 100
        itemsJson["rareKey"] = 100;  // Set rareKey to 100
        itemsJson["epicKey"] = 100;  // Set epicKey to 100

        // Notify listeners to update only Player data
        PlayerProfile.Data.NotifyListeners("Player");

        Debug.Log($"🎯 Test resources added: Diamond={player.Diamond}, heroKey={itemsJson["heroKey"]}, rareKey={itemsJson["rareKey"]}, epicKey={itemsJson["epicKey"]}");
    }
}