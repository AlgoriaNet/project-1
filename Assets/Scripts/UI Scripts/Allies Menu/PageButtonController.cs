using UnityEngine;
using UnityEngine.UI;

public class PageButtonController : MonoBehaviour
{
    public Button[] buttons; // Assign the buttons
    public Image[] buttonImages; // Assign the Image components of the button backgrounds
    private int activeButtonIndex = -1; // No button is active initially

    public GameObject[] pages; // Assign the corresponding pages in Inspector


    public void ToggleButtonVisibility(int buttonIndex)
    {
        for (int i = 0; i < buttonImages.Length; i++)
        {
            if (i == buttonIndex)
            {
                // Show the clicked button's image
                buttonImages[i].color = new Color(buttonImages[i].color.r, buttonImages[i].color.g, buttonImages[i].color.b, 1f);
                activeButtonIndex = i;
            }
            else
            {
                // Hide other button images
                buttonImages[i].color = new Color(buttonImages[i].color.r, buttonImages[i].color.g, buttonImages[i].color.b, 0f);
            }

            // 🔹 Toggle corresponding pages
            if (i < pages.Length)
            {
                pages[i].SetActive(i == buttonIndex);
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

        // 🔹 Highlight Button 1 by default if it exists
        if (buttons.Length > 0)
        {
            ToggleButtonVisibility(0); // Highlight first button
        }
    }
}