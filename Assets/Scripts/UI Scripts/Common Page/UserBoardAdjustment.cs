using UnityEngine;

public class UserBoardAdjustment : MonoBehaviour
{
    public RectTransform userPopup; // Assign the "User Popup" RectTransform in the Inspector
    private RectTransform userBoard; // The board being adjusted

    private Vector2 originalSize = new Vector2(960, 880); // Original size of the board
    public bool preserveAspect = true; // Default to preserving aspect ratio when possible

    void Start()
    {
        userBoard = GetComponent<RectTransform>();

        if (userPopup == null)
        {
            Debug.LogError("User Popup is not assigned!");
            return;
        }

        AdjustBoardSize();
    }

    private void AdjustBoardSize()
    {
        float popupWidth = userPopup.rect.width;

        if (popupWidth < originalSize.x)
        {
            // Popup is narrower than the board: Adjust only width, keep original height
            preserveAspect = false; // Disable aspect ratio preservation
            userBoard.sizeDelta = new Vector2(popupWidth, originalSize.y);
            Debug.Log("Board adjusted to fit width only, keeping original height.");
        }
        else
        {
            // Popup is wider or equal: Keep original size, preserve aspect ratio
            preserveAspect = true; // Enable aspect ratio preservation
            userBoard.sizeDelta = originalSize;
            Debug.Log("Board retains original size with preserved aspect ratio.");
        }

        ApplyAspectRatioSettings();
    }

    private void ApplyAspectRatioSettings()
    {
        // Apply preserve aspect ratio settings dynamically if using a CanvasScaler or similar components
        var imageComponent = userBoard.GetComponent<UnityEngine.UI.Image>();
        if (imageComponent != null)
        {
            imageComponent.preserveAspect = preserveAspect;
        }
    }
}

