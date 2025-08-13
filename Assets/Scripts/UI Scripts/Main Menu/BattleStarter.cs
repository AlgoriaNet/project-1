using UnityEngine;

public class BattleStarter : MonoBehaviour
{
    public Canvas mainCanvas;
    public GameObject battleObject;

    public void StartBattle()
    {
        if (mainCanvas != null) mainCanvas.enabled = false;
        if (battleObject != null) battleObject.SetActive(true);
        
        // Wait a frame then reset battle state (after battleObject is active)
        StartCoroutine(ResetBattleAfterFrame());
        
        Debug.Log("Battle initiated. Main canvas hidden.");
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