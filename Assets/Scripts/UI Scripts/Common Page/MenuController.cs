using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuController : MonoBehaviour
{
    public Button[] buttons; // Assign buttons in the inspector
    public GameObject[] menus; // Assign corresponding menus in the inspector
    public ChestManager chestManager;

    // Special UI elements for Menu 1 case (AlliesMenu)
    public GameObject step1Panel;
    public GameObject step2Panel;
    public GameObject lowerGroup;
    public GameObject topPanel; // This is the topPanel in Common Page

    private int currentMenuIndex = -1; // Default to -1 (no menu active)

    private Button activeButton; // Track the currently active button

    public RectTransform leftArrow;
    public RectTransform rightArrow;

    public Image backgroundImage; // Assign background image in the inspector

    private void Start()
    {
        // Add listeners to buttons
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // Capture the index for the lambda   
            buttons[i].onClick.AddListener(() => ToggleMenu(index));
        }

        // Force the layout system to update all canvases
        Canvas.ForceUpdateCanvases();

        // ✅ Set Button_3 as default active
        if (buttons.Length > 2) // Ensure Button_3 exists in the array
        {
            ToggleMenu(2); // This will also call UpdateButtonVisuals internally
        }
    }

    private void ToggleMenu(int activeMenuIndex)
    {
        currentMenuIndex = activeMenuIndex; // Update the active menu index

        // Loop through all menus
        for (int i = 0; i < menus.Length; i++)
        {
            menus[i].SetActive(i == activeMenuIndex); // Enable only the selected menu
        }

        // 🔹 Change button visuals
        UpdateButtonVisuals(buttons[activeMenuIndex]);

        // 🔹 Special Case: If Menu 1 is activated (index 0)
        if (activeMenuIndex == 0)
        {
            if (step1Panel != null) step1Panel.SetActive(true);
            if (step2Panel != null) step2Panel.SetActive(false);
            if (lowerGroup != null) lowerGroup.SetActive(false);
        }

        // 🔹 Menu 3 (Main Menu) - Handle animation trigger based on countdown status
        if (activeMenuIndex == 2) // Menu 3
        {
            if (chestManager.isCountdownActive) // If the countdown is still active
            {
                chestManager.chestAnimator?.ResetTrigger("ChestReady");
            }
            else
            {
                chestManager.chestAnimator?.SetTrigger("ChestReady");
            }
        }

        // 🔹 Disable the background image when Button_4 is clicked
        if (activeMenuIndex == 3) // Assuming Button_4 corresponds to index 3
        {
            if (backgroundImage != null)
            {
                backgroundImage.enabled = false; // Disable the background
            }
        }
        else
        {
            // Re-enable the background when any other button is clicked
            if (backgroundImage != null)
            {
                backgroundImage.enabled = true; // Enable the background
            }
        }

        // Disable topPanel when Menu_5 is opened (assuming index 4 is Menu_5)
        if (activeMenuIndex == 4)
        {
            if (topPanel != null) topPanel.SetActive(false);
        }
        else
        {
            if (topPanel != null) topPanel.SetActive(true);
        }
    }

    private void UpdateButtonVisuals(Button selectedButton)
    {
        // Restore previous button (if any)
        if (activeButton != null)
        {
            Transform prevButtonTransform = activeButton.transform;
            Image prevButtonImage = prevButtonTransform.GetComponent<Image>();
            TextMeshProUGUI prevButtonText = prevButtonTransform.Find("Text")?.GetComponent<TextMeshProUGUI>();

            if (prevButtonImage != null) prevButtonImage.color = Color.white; // Restore white color
            if (prevButtonText != null) prevButtonText.gameObject.SetActive(false); // Hide text
        }

        // Set new active button
        activeButton = selectedButton;
        UpdateArrowPositions(activeButton);

        // Change current button visuals
        Transform activeButtonTransform = activeButton.transform;
        Image newButtonImage = activeButtonTransform.GetComponent<Image>();
        TextMeshProUGUI newButtonText = activeButtonTransform.Find("Text")?.GetComponent<TextMeshProUGUI>();

        if (newButtonImage != null) newButtonImage.color = Color.black; // Change to black
        if (newButtonText != null) newButtonText.gameObject.SetActive(true); // Enable text
    }

    private void UpdateArrowPositions(Button activeButton)
    {
        RectTransform buttonRect = activeButton.GetComponent<RectTransform>();
        if (buttonRect == null) return;

        // Calculate arrow height as 1/3 of the button height.
        float buttonHeight = buttonRect.rect.height;
        float arrowHeight = buttonHeight / 3f;

        // Resize left arrow while preserving its aspect ratio.
        float leftAspect = leftArrow.rect.width / leftArrow.rect.height;
        float newLeftWidth = arrowHeight * leftAspect;
        leftArrow.sizeDelta = new Vector2(newLeftWidth, arrowHeight);

        // Resize right arrow while preserving its aspect ratio.
        float rightAspect = rightArrow.rect.width / rightArrow.rect.height;
        float newRightWidth = arrowHeight * rightAspect;
        rightArrow.sizeDelta = new Vector2(newRightWidth, arrowHeight);

        // Retrieve the button's world corners.
        Vector3[] corners = new Vector3[4];
        buttonRect.GetWorldCorners(corners);
        // corners[0] is bottom-left, corners[1] is top-left,
        // corners[2] is top-right, corners[3] is bottom-right.

        // Get the exact left and right edges (x coordinates) and the vertical center (y).
        float leftEdge = corners[0].x; // left edge x
        float rightEdge = corners[2].x; // right edge x
        float midY = (corners[0].y + corners[1].y) / 2f;

        // Position left arrow so its center is exactly left of the button.
        leftArrow.position = new Vector3(
            leftEdge - newLeftWidth / 2f,
            midY,
            leftArrow.position.z
        );

        // Position right arrow so its center is exactly right of the button.
        rightArrow.position = new Vector3(
            rightEdge + newRightWidth / 2f,
            midY,
            rightArrow.position.z
        );
    }   

    public bool IsMenuActive(int menuIndex)
    {
        return currentMenuIndex == menuIndex;
    }

    public int GetCurrentMenuIndex()
    {
        return currentMenuIndex;
    }
}