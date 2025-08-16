using UnityEngine;
using model;

public class BattleStarter : MonoBehaviour
{
    public Canvas mainCanvas;
    public GameObject battleObject;
    
    private const int REQUIRED_STAMINA = 10;

    public void StartBattle()
    {
        // Validate stamina before starting battle
        if (!ValidateStamina())
        {
            return;
        }
        
        if (mainCanvas != null) mainCanvas.enabled = false;
        if (battleObject != null) battleObject.SetActive(true);
        
        // Wait a frame then reset battle state (after battleObject is active)
        StartCoroutine(ResetBattleAfterFrame());
        
        Debug.Log("Battle initiated. Main canvas hidden.");
    }
    
    private bool ValidateStamina()
    {
        var player = PlayerProfile.Data.Player;
        if (player == null)
        {
            Debug.LogWarning("[BattleStarter] Cannot start battle: Player data not loaded");
            // TODO: Show UI message "Player data loading..."
            return false;
        }
        
        if (player.Stamina < REQUIRED_STAMINA)
        {
            Debug.LogWarning($"[BattleStarter] Cannot start battle: Insufficient stamina ({player.Stamina}/{REQUIRED_STAMINA})");
            // TODO: Show UI message "Need 10 stamina to start battle"
            return false;
        }
        
        Debug.Log($"[BattleStarter] Stamina check passed: {player.Stamina}/{REQUIRED_STAMINA}");
        return true;
    }
    
    private System.Collections.IEnumerator ResetBattleAfterFrame()
    {
        yield return null; // Wait one frame for battleObject to be fully active
        
        // Reset battle state for new game
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.ResetBattle();
        }
    }
}