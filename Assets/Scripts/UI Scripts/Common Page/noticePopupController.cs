using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class noticePopupController : MonoBehaviour
{
    public GameObject noticePopup; // Assign the notice Popup object
    public RectTransform board; // Assign the Board object
    public GameObject panel; // The panel serving as the close button
    public RectTransform bowField; // Assign the BowField RectTransform
    private Vector2 originalBoardSize; // Store the original board size
    private RectTransform noticePopupRect; // Reference to the notice Popup RectTransform

    void Start()
    {
        // Ensure references are assigned
        if (noticePopup == null || board == null || panel == null)
        {
            Debug.LogError("Please assign all required references in the Inspector.");
            return;
        }

        noticePopupRect = noticePopup.GetComponent<RectTransform>();
        originalBoardSize = board.sizeDelta;
    }

    void Update()
    {
        // Ensure the board adjusts dynamically if the popup is active
        if (noticePopup.activeSelf)
        {
            AdjustBoardSize();
        }
    }

    public void OpennoticePopup()
    {
        noticePopup.SetActive(true);
        AdjustBoardSize();
    }

    public void ClosenoticePopup()
    {
        noticePopup.SetActive(false);
    }

    private void AdjustBoardSize()
    {
        if (noticePopupRect != null)
        {
            float popupWidth = noticePopupRect.rect.width;

            // Adjust the board width if the popup is smaller than the original board
            if (popupWidth < originalBoardSize.x + 60)
            {
                board.sizeDelta = new Vector2(popupWidth -60, originalBoardSize.y);
                board.GetComponent<Image>().preserveAspect = false; // Uncheck Preserve Aspect
            }
            else
            {
                board.sizeDelta = originalBoardSize;
                board.GetComponent<Image>().preserveAspect = true; // Check Preserve Aspect
            }

            // Adjust the BowField width to 80% of the board width
            if (bowField != null)
            {
                float bowFieldWidth = board.rect.width * 0.8f; // Calculate BowField width as 70% of Board width
                bowField.sizeDelta = new Vector2(bowFieldWidth, bowField.sizeDelta.y); // Adjust BowField width while keeping its height unchanged
                bowField.anchoredPosition = new Vector2(0, bowField.anchoredPosition.y); // Center BowField horizontally if needed
            }
        }
    }
}