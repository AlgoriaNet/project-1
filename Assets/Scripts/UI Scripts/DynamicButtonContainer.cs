using UnityEngine;
using UnityEngine.UI;

public class DynamicButtonContainer : MonoBehaviour
{
    void Start()
    {
        RectTransform containerRect = GetComponent<RectTransform>();

        // Get the current screen width (physical resolution of the device in pixels)
        float screenWidth = Screen.width;

        // Initially set the container's width equal to the screen width
        float containerWidth = screenWidth;
        containerRect.sizeDelta = new Vector2(0, containerWidth);  // Temporarily set the container's width in the sizeDelta

        // Retrieve the actual container width after Unity's layout adjustments
        containerWidth = containerRect.rect.width;

        // Calculate the container height as 1/5 of the container width
        float containerHeight = containerWidth / 5;

        // Update the sizeDelta to apply the calculated height while keeping the width
        containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, containerHeight);

        // Set the Y position of the container to half of its height to properly align it at the bottom
        containerRect.anchoredPosition = new Vector2(containerRect.anchoredPosition.x, containerHeight / 2);

        // Adjust the height of each button dynamically and set the preferred height
        foreach (Transform child in transform)
        {
            RectTransform buttonRect = child.GetComponent<RectTransform>();
            LayoutElement layoutElement = child.GetComponent<LayoutElement>();
            if (buttonRect != null)
            {
                // Ensure the button height matches the container height
                buttonRect.sizeDelta = new Vector2(buttonRect.sizeDelta.x, containerHeight);

                // Add or update the LayoutElement to respect preferred height
                if (layoutElement == null)
                {
                    layoutElement = child.gameObject.AddComponent<LayoutElement>();
                }
                layoutElement.preferredHeight = containerHeight;
            }
        }
    }
}