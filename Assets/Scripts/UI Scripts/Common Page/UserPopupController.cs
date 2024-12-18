using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UserPopupController : MonoBehaviour
{
    public GameObject userPopup; // Assign the User Popup object
    public RectTransform board; // Assign the Board object
    public Button closeButton; // Assign the Close Button on the board
    public Button editNameButton; // Assign the Pencil Icon Button
    public TMP_InputField nameInputField; // Assign the Input Field for editing username

    public Image topPanelUserIcon; // Assign the User Icon from the Top Panel
    public TextMeshProUGUI topPanelUsername; // Assign the Username from the Top Panel
    public Image popupUserIcon; // Assign the User Icon in the User Popup

    private Vector2 originalBoardSize; // Store the original board size
    private RectTransform userPopupRect; // Reference to the User Popup RectTransform

    public GameObject step1Panel; // Assign Step 1 panel in the Inspector
    public GameObject step2Panel; // Assign Step 2 panel in the Inspector

    public Button saveButton; // Assign the Save button in the Inspector
    public IconsGridSetup iconsGridSetup; // Reference to IconsGridSetup script


    void Start()
    {
        // Ensure references are assigned
        if (userPopup == null || board == null || closeButton == null)
        {
            Debug.LogError("Please assign all required references in the Inspector.");
            return;
        }

        userPopupRect = userPopup.GetComponent<RectTransform>();
        originalBoardSize = board.sizeDelta;

        // Load the username from PlayerPrefs
        string savedUsername = PlayerPrefs.GetString("Username", "DefaultName"); // Default name
        topPanelUsername.text = savedUsername;

        // Hook up button listeners
        closeButton.onClick.AddListener(CloseUserPopup);
        editNameButton.onClick.AddListener(OpenNameEdit);
        nameInputField.onEndEdit.AddListener(SaveEditedName);

        popupUserIcon.GetComponent<Button>().onClick.AddListener(SwitchToStep2);
        saveButton.onClick.AddListener(SaveAndClosePanels); 
    }

    void Update()
    {
        // Ensure the board adjusts dynamically if the popup is active
        if (userPopup.activeSelf)
        {
            AdjustBoardSize();
        }
    }

    public void OpenUserPopup()
    {
        // Sync the user icon from the top panel to the popup
        if (popupUserIcon != null && topPanelUserIcon != null)
        {
            popupUserIcon.sprite = topPanelUserIcon.sprite; // Copy the user icon
        }

        // Always populate the InputField with the latest username
        if (nameInputField != null)
        {
            string savedUsername = PlayerPrefs.GetString("Username", "DefaultName"); // Retrieve username from PlayerPrefs
            nameInputField.text = savedUsername; // Ensure InputField has the latest value
            nameInputField.gameObject.SetActive(true); // Ensure it's visible
        }

        userPopup.SetActive(true);
        AdjustBoardSize();
    }

    public void CloseUserPopup()
    {
        userPopup.SetActive(false);
    }

    private void AdjustBoardSize()
    {
        if (userPopupRect != null)
        {
            float popupWidth = userPopupRect.rect.width;

            // Adjust the board width if the popup is smaller than the original board
            if (popupWidth < originalBoardSize.x)
            {
                board.sizeDelta = new Vector2(popupWidth, originalBoardSize.y);
                board.GetComponent<Image>().preserveAspect = false; // Uncheck Preserve Aspect
            }
            else
            {
                board.sizeDelta = originalBoardSize;
                board.GetComponent<Image>().preserveAspect = true; // Check Preserve Aspect
            }
        }
    }

    private void OpenNameEdit()
    {
        // Show the input field with the current username
        if (nameInputField != null)
        {
            nameInputField.gameObject.SetActive(true);
            nameInputField.ActivateInputField();
        }
    }

    private void SaveEditedName(string newName)
    {
        if (!string.IsNullOrEmpty(newName))
        {
            // Save the new username to PlayerPrefs
            PlayerPrefs.SetString("Username", newName);

            // Update the username in the top panel
            topPanelUsername.text = newName;

            Debug.Log($"Username updated to: {newName}");
        }

        // Hide the input field
        nameInputField.gameObject.SetActive(false);
    }

    private void SwitchToStep2()
    {
        if (step1Panel != null)
        {
            step1Panel.SetActive(false); // Disable Step 1 panel
        }
        
        if (step2Panel != null)
        {
            step2Panel.SetActive(true); // Enable Step 2 panel
        }
        
        // Add the reshuffle and grid setup logic here
        IconsGridSetup iconsGridSetup = step2Panel.GetComponentInChildren<IconsGridSetup>();
        if (iconsGridSetup != null)
        {
            // Retrieve the current user icon path from PlayerPrefs
            string currentUserIconPath = PlayerPrefs.GetString("UserIconPath", iconsGridSetup.defaultIconPath);

            // Load all icons from the correct Resources path
            Sprite[] allIcons = Resources.LoadAll<Sprite>("UILoading/CharacterImages/UserIcons/WhiteBorderIcons");
            if (allIcons == null || allIcons.Length == 0)
            {
                Debug.LogError("No icons found in the path: Resources/UILoading/CharacterImages/UserIcons/WhiteBorderIcons. Ensure the icons are correctly placed.");
                return;
            }

            // Sort icons by name to ensure ascending order
            System.Array.Sort(allIcons, (a, b) => a.name.CompareTo(b.name));

            // Reorder icons to place the current user icon first
            Sprite[] orderedIcons = iconsGridSetup.ReorderIcons(currentUserIconPath, allIcons);

            // Set up the grid
            iconsGridSetup.SetupGrid(orderedIcons);

            // Set the initial tick position and size
            iconsGridSetup.SetInitialTickPosition(currentUserIconPath);
        }

        Debug.Log("Switched from Step 1 to Step 2.");
    }

    private void SaveAndClosePanels()
    {
        // Retrieve the selected icon sprite from IconsGridSetup
        Sprite selectedIcon = iconsGridSetup.GetSelectedIcon();

        // Update the user icon in the top panel
        if (selectedIcon != null)
        {
            topPanelUserIcon.sprite = selectedIcon;
            popupUserIcon.sprite = selectedIcon; 
            PlayerPrefs.SetString("UserIconPath", $"UILoading/CharacterImages/UserIcons/WhiteBorderIcons/{selectedIcon.name}");
            PlayerPrefs.Save();
            Debug.Log($"Saved Icon: {selectedIcon.name}");
        }

        // Close Step 2 and reopen Step 1
        step2Panel.SetActive(false);
        step1Panel.SetActive(true);
    }
}
