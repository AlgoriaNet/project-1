using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Pages")]
    public GameObject winPage;
    public GameObject losePage;
    public GameObject battleObject;

    [Header("Shared Buttons")]
    public Button watchAdsButton;
    public Button closeButton;

    
    void Start()
    {
        // Setup button listeners
        SetupButtonListeners();
        
        // Initially hide both pages
        if (winPage) winPage.SetActive(false);
        if (losePage) losePage.SetActive(false);
    }
    
    void SetupButtonListeners()
    {
        // Shared buttons
        if (closeButton) closeButton.onClick.AddListener(() => CloseGameOver());
        if (watchAdsButton) watchAdsButton.onClick.AddListener(() => WatchAds());
    }
    
    /// <summary>
    /// Show game over screen based on win/lose result
    /// </summary>
    public void ShowGameOver(bool isWin)
    {
        Debug.Log($"[GameOverUI] Showing game over screen - Win: {isWin}");
        
        // Activate the parent GameObject (End) first
        gameObject.SetActive(true);
        Debug.Log($"[GameOverUI] Activated parent GameObject: {gameObject.name}");
        
        if (isWin)
        {
            // Show win page
            if (winPage) winPage.SetActive(true);
            if (losePage) losePage.SetActive(false);
            Debug.Log("[GameOverUI] Win page displayed");
        }
        else
        {
            // Show lose page
            if (losePage) losePage.SetActive(true);
            if (winPage) winPage.SetActive(false);
            Debug.Log("[GameOverUI] Lose page displayed");
        }
    }
    
    /// <summary>
    /// (1) Close game over UI and return to main game
    /// </summary>
    public void CloseGameOver()
    {
        Debug.Log("[GameOverUI] Closing game over screen");
        
        // Hide both pages
        if (winPage) winPage.SetActive(false);
        if (losePage) losePage.SetActive(false);
        
        // Hide End GameObject (game over panel)
        gameObject.SetActive(false);
        
        // Resume game time
        Time.timeScale = 1f;
        
        // CRITICAL: Enable Main Canvas component that was disabled when battle started
        GameObject mainCanvas = GameObject.Find("Main Canvas");
        if (mainCanvas != null)
        {
            mainCanvas.SetActive(true);
            
            // Enable the Canvas component specifically
            Canvas canvasComponent = mainCanvas.GetComponent<Canvas>();
            if (canvasComponent != null)
            {
                canvasComponent.enabled = true;
                Debug.Log("[GameOverUI] Main Canvas component enabled");
            }
            else
            {
                Debug.LogError("[GameOverUI] Canvas component not found on Main Canvas!");
            }
        }
        else
        {
            Debug.LogError("[GameOverUI] Main Canvas not found!");
        }
        
        // Disable battle object
        if (battleObject) 
        {
            battleObject.SetActive(false);
            Debug.Log("[GameOverUI] Battle disabled");
        }
    }
    
    /// <summary>
    /// (2) Watch ads for rewards - placeholder implementation
    /// </summary>
    public void WatchAds()
    {
        Debug.Log("[GameOverUI] Watch ads button clicked");
        
        // TODO: Integrate with ad system
        // Placeholder implementation:
        
        Debug.Log("[GameOverUI] PLACEHOLDER: Showing rewarded ad...");
        
        // Simulate ad watched and reward given
        StartCoroutine(SimulateAdWatching());
    }
    
    /// <summary>
    /// (3) Load equipment get page - placeholder implementation  
    /// </summary>
    public void LoadEquipmentGet()
    {
        Debug.Log("[GameOverUI] PLACEHOLDER: Loading equipment rewards page...");
        
        // TODO: Implement equipment rewards UI
        // For now, just close the game over screen
        CloseGameOver();
    }
    
    /// <summary>
    /// Restart the battle (only available on lose page)
    /// </summary>
    public void RestartBattle()
    {
        Debug.Log("[GameOverUI] Restart battle button clicked");
        
        // Hide game over UI
        if (winPage) winPage.SetActive(false);
        if (losePage) losePage.SetActive(false);
        
        // Find and trigger battle starter
        BattleStarter battleStarter = FindObjectOfType<BattleStarter>();
        if (battleStarter != null)
        {
            battleStarter.StartBattle();
            Debug.Log("[GameOverUI] Battle restarted successfully");
        }
        else
        {
            Debug.LogError("[GameOverUI] BattleStarter not found! Cannot restart battle.");
        }
    }
    
    /// <summary>
    /// Simulate ad watching with coroutine
    /// </summary>
    private System.Collections.IEnumerator SimulateAdWatching()
    {
        Debug.Log("[GameOverUI] Ad started playing...");
        
        // Simulate 3 second ad
        yield return new WaitForSecondsRealtime(3f);
        
        Debug.Log("[GameOverUI] Ad finished! Giving reward...");
        
        // TODO: Give actual rewards (coins, gems, etc.)
        Debug.Log("[GameOverUI] PLACEHOLDER: +100 coins, +10 gems awarded!");
        
        // Could stay on current page or close automatically
        // For now, keep the game over screen open so user can choose next action
    }
    
    /// <summary>
    /// Public method for other scripts to trigger game over
    /// </summary>
    public static void TriggerGameOver(bool isWin)
    {
        GameOverUI gameOverUI = FindObjectOfType<GameOverUI>();
        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver(isWin);
        }
        else
        {
            Debug.LogError("[GameOverUI] GameOverUI instance not found in scene!");
        }
    }
}