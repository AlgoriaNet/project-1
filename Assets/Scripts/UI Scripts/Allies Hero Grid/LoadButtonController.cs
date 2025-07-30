using UnityEngine;
using UnityEngine.UI;

public class LoadButtonController : MonoBehaviour
{
    public Button[] buttons; // Assign the buttons
    public Image[] buttonImages; // Assign the Image components of the button backgrounds
    public ItemLoader itemLoader; // Reference to the ItemLoader script
    private int activeButtonIndex = -1; // No button is active initially

    public void ToggleButtonVisibility(int buttonIndex)
    {
        for (int i = 0; i < buttonImages.Length; i++)
        {
            if (i == buttonIndex)
            {
                // Show the clicked button's image
                buttonImages[i].color = new Color(buttonImages[i].color.r, buttonImages[i].color.g, buttonImages[i].color.b, 1f);
                activeButtonIndex = i;

                // Update the item type and load the corresponding images
                itemLoader.SwitchItemType((ItemLoader.ItemType)i); // Correct method to switch item type
            }
            else
            {
                // Hide other button images
                buttonImages[i].color = new Color(buttonImages[i].color.r, buttonImages[i].color.g, buttonImages[i].color.b, 0f);
            }
        }
        
        // Update the Orange button text when tabs change
        UpdateOrangeButtonText();
    }
    
    /// <summary>
    /// Update the Orange button text via SwitchPanels when tabs change
    /// </summary>
    private void UpdateOrangeButtonText()
    {
        SwitchPanels switchPanels = FindObjectOfType<SwitchPanels>();
        if (switchPanels != null)
        {
            switchPanels.UpdateButtonText();
        }
    }

    private void Start()
    {
        // Set all images to hidden at start
        foreach (var img in buttonImages)
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
        }

        // Only highlight Button 1 by default if no button is currently active
        // This prevents automatic switch to Equipment when switching allies
        if (buttons.Length > 0 && activeButtonIndex == -1)
        {
            ToggleButtonVisibility(0); // Highlight first button (Equipment by default)
            Debug.Log("[LoadButtonController] First time initialization - setting Equipment as default");
        }
        else if (buttons.Length > 0 && activeButtonIndex != -1)
        {
            // If a button was already active, restore its visual state without changing the tab
            Debug.Log($"[LoadButtonController] Reactivated - preserving current tab {activeButtonIndex} instead of switching to Equipment");
            RestoreActiveButtonVisual();
        }
    }
    
    /// <summary>
    /// Restore the visual state of the currently active button without triggering tab switch
    /// </summary>
    private void RestoreActiveButtonVisual()
    {
        if (activeButtonIndex >= 0 && activeButtonIndex < buttonImages.Length)
        {
            // Show only the active button's image
            buttonImages[activeButtonIndex].color = new Color(
                buttonImages[activeButtonIndex].color.r, 
                buttonImages[activeButtonIndex].color.g, 
                buttonImages[activeButtonIndex].color.b, 1f);
                
            // Update the Orange button text when restoring tab state
            UpdateOrangeButtonText();
        }
    }
}