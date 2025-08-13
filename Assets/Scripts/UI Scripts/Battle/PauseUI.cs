using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles pause confirmation dialog during battle
/// 
/// Setup Instructions:
/// 1. Create a pause panel GameObject in your battle UI
/// 2. Add Continue and Quit buttons (2 buttons only)
/// 3. Assign this script to a GameObject in the battle scene
/// 4. Assign the PauseUI reference in BattleManager
/// 5. Connect button references and battleObject in the inspector
/// 
/// Usage:
/// - Pause button shows confirmation dialog with 2 options
/// - Continue: Resume battle at previous speed  
/// - Quit: Return to main menu (same as GameOverUI close)
/// </summary>
public class PauseUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pausePanel;
    public Button continueButton;
    public Button quitButton;
    
    [Header("References")]
    public GameObject battleObject;

    void Start()
    {
        // Setup button listeners
        SetupButtonListeners();
        
        // Initially hide pause panel
        if (pausePanel) pausePanel.SetActive(false);
    }
    
    void SetupButtonListeners()
    {
        if (continueButton) continueButton.onClick.AddListener(() => ContinueBattle());
        if (quitButton) quitButton.onClick.AddListener(() => QuitBattle());
    }
    
    /// <summary>
    /// Show the pause confirmation panel
    /// </summary>
    public void ShowPauseMenu()
    {
        Debug.Log("[PauseUI] Showing pause menu");
        
        // Pause the game (using BattleManager's logic)
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.PauseBattle();
        }
        else
        {
            // Fallback if BattleManager not available
            Time.timeScale = 0f;
        }
        
        // Show pause panel
        if (pausePanel) 
        {
            pausePanel.SetActive(true);
            Debug.Log("[PauseUI] Pause panel activated");
        }
        
        // Activate main UI object if needed
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Continue the battle (resume from pause)
    /// </summary>
    public void ContinueBattle()
    {
        Debug.Log("[PauseUI] Continuing battle");
        
        // Hide pause panel
        if (pausePanel) pausePanel.SetActive(false);
        
        // Resume the game using BattleManager's method
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.ResumeBattle();
        }
        
        Debug.Log("[PauseUI] Battle resumed");
    }
    
    /// <summary>
    /// Quit the battle and return to main menu
    /// </summary>
    public void QuitBattle()
    {
        Debug.Log("[PauseUI] Quitting battle - cleaning up properly");
        
        // Hide pause panel
        if (pausePanel) pausePanel.SetActive(false);
        
        // CRITICAL: Call BattleManager.GameOver to properly clean up battle state
        // This ensures monsters are destroyed and battle state is reset
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.GameOver(false); // false = lose, to trigger cleanup
            // GameOver will handle all the cleanup, then we just need to close the GameOver UI
        }
        
        // Wait a frame then close the GameOver UI that was just shown
        StartCoroutine(CloseGameOverUIAfterFrame());
    }
    
    /// <summary>
    /// Close the GameOver UI that appears after calling GameOver
    /// </summary>
    private System.Collections.IEnumerator CloseGameOverUIAfterFrame()
    {
        yield return null; // Wait one frame
        
        // Find and close the GameOver UI
        GameOverUI gameOverUI = FindObjectOfType<GameOverUI>();
        if (gameOverUI != null)
        {
            gameOverUI.CloseGameOver();
            Debug.Log("[PauseUI] GameOver UI closed after cleanup");
        }
    }
    
    /// <summary>
    /// Public method for other scripts to trigger pause menu
    /// </summary>
    public static void TriggerPauseMenu()
    {
        PauseUI pauseUI = FindObjectOfType<PauseUI>();
        if (pauseUI != null)
        {
            pauseUI.ShowPauseMenu();
        }
        else
        {
            Debug.LogError("[PauseUI] PauseUI instance not found in scene!");
        }
    }
    
    /// <summary>
    /// Hide pause menu without resuming (for external control)
    /// </summary>
    public void HidePauseMenu()
    {
        if (pausePanel) pausePanel.SetActive(false);
    }
}