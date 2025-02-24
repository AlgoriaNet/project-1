using UnityEngine;
using UnityEngine.UI;

public class DynamicButtonContainer : MonoBehaviour
{
    // // Set this flag in the Inspector. When true, the 6th button (Button_6) will be enabled.
    // public bool isSixthButtonUnlocked = true;

    // Assign Button_6 in the Inspector.
    public GameObject Button_6;

    // For testing purposes, set a mock user level here.
    // In the future, you can replace this with:
    // int userLevel = PlayerPrefs.GetInt("UserLevel", 1);
    private int userLevel; // Local variable for testing.

    void Awake()
    {
        // Declare and set the user level for testing. overriding the value in inspector
        userLevel = 1;
    }

    void Start()
    {
        // Determine if the 6th button should be unlocked (e.g., level >= 20).
        bool isSixthButtonUnlocked = userLevel >= 20;

        RectTransform containerRect = GetComponent<RectTransform>();

        // Get the current screen width (physical resolution in pixels)
        float screenWidth = Screen.width;

        // Initially set the container's width equal to the screen width
        float containerWidth = screenWidth;
        containerRect.sizeDelta = new Vector2(0, containerWidth);

        // Retrieve the actual container width after layout adjustments
        containerWidth = containerRect.rect.width;

        // Determine the number of buttons: 5 if locked, 6 if unlocked.
        int buttonCount = isSixthButtonUnlocked ? 6 : 5;

        // Calculate container height based on the button count.
        float containerHeight = containerWidth / buttonCount;

        // Update the container's height (keeping its width)
        containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, containerHeight);

        // Align the container vertically (for example, set its anchored Y position to half the height)
        containerRect.anchoredPosition = new Vector2(containerRect.anchoredPosition.x, containerHeight / 2);

        // Adjust each child button's height.
        foreach (Transform child in transform)
        {
            RectTransform buttonRect = child.GetComponent<RectTransform>();
            LayoutElement layoutElement = child.GetComponent<LayoutElement>();

            if (buttonRect != null)
            {
                // Set each button's height to match the container height.
                buttonRect.sizeDelta = new Vector2(buttonRect.sizeDelta.x, containerHeight);

                // Add or update a LayoutElement to enforce the preferred height.
                if (layoutElement == null)
                {
                    layoutElement = child.gameObject.AddComponent<LayoutElement>();
                }
                layoutElement.preferredHeight = containerHeight;
            }
        }

        // If the 6th button is unlocked, simply enable it.
        if (isSixthButtonUnlocked)
        {
            Debug.Log("isSixthButtonUnlocked is true: enabling Button_6");
            Button_6.SetActive(true);
            // Force the layout group to rebuild to reflect the change
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
        }
    }
}
