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
    }

    private void Start()
    {
        // Set all images to hidden at start
        foreach (var img in buttonImages)
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
        }

        // Highlight Button 1 by default if it exists
        if (buttons.Length > 0)
        {
            ToggleButtonVisibility(0); // Highlight first button (Equipment by default)
        }
    }
}