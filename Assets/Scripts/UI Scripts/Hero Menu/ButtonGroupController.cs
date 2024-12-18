using UnityEngine;
using UnityEngine.UI;

public class ButtonVisibilityController : MonoBehaviour
{
    public Button[] buttons; // Assign the buttons
    public Image[] buttonImages; // Assign the Image components of the button backgrounds

    private int activeButtonIndex = -1; // No button is active initially

    public void ToggleButtonVisibility(int buttonIndex)
    {
        for (int i = 0; i < buttonImages.Length; i++)
        {
            if (i == buttonIndex)
            {
                // Show the clicked button's image
                // buttonImages[i].enabled = true;
                buttonImages[i].color = new Color(buttonImages[i].color.r, buttonImages[i].color.g, buttonImages[i].color.b, 1f);
                activeButtonIndex = i;
            }
            else
            {
                // Hide other button images
                // buttonImages[i].enabled = false;
                buttonImages[i].color = new Color(buttonImages[i].color.r, buttonImages[i].color.g, buttonImages[i].color.b, 0f);
            }
        }
    }

    private void Start()
    {
        // Set all images to hidden at start
        foreach (var img in buttonImages)
        {
            // img.enabled = false;
            img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
        }
    }
}