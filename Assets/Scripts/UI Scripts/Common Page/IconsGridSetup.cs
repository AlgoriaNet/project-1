using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class IconsGridSetup : MonoBehaviour
{
    public GridLayoutGroup grid; // Assign the GridLayoutGroup in the Inspector
    public RectTransform contentPanel; // Assign the ContentPanel RectTransform
    public string defaultIconPath = "UILoading/CharacterImages/UserIcons/WhiteBorderIcons/00"; // Default path for the user icon
    private Sprite selectedIconSprite;
    public Image tickImage; // Assign the tick/checkmark image in the Inspector
    private GameObject currentSelectedIcon; // Store the currently selected icon GameObject
    private string currentUserIconPath; // Store the current user icon path

    public int totalIcons = 21; // Maximum number of icons

    // List of unlocked and locked icons
    private List<string> unlockedIcons;
    private List<string> lockedIcons;

    void Start()
    {
        // Retrieve the current user icon path from PlayerPrefs
        string currentUserIconPath = PlayerPrefs.GetString("UserIconPath", defaultIconPath);

        // Load unlocked icons from PlayerPrefs
        string unlockedIconsStr = PlayerPrefs.GetString("UnlockedIcons", "00"); // Default is only hero unlocked
        unlockedIcons = new List<string>(unlockedIconsStr.Split(','));
        // unlockedIcons = new List<string> { "00", "03", "19" }; // Simulate unlocked items for testing only

        // Load all icons from the correct Resources path
        Sprite[] allIcons = Resources.LoadAll<Sprite>("UILoading/CharacterImages/UserIcons/WhiteBorderIcons");
        if (allIcons == null || allIcons.Length == 0)
        {
            Debug.LogError("No icons found in the path: Resources/UILoading/CharacterImages/UserIcons/WhiteBorderIcons. Ensure the icons are correctly placed.");
            return;
        }

        // Determine locked icons (allIcons minus unlockedIcons)
        lockedIcons = new List<string>();
        foreach (Sprite icon in allIcons)
        {
            if (!unlockedIcons.Contains(icon.name))
            {
                lockedIcons.Add(icon.name);
            }
        }

        // Reorder icons and set up the grid
        Sprite[] orderedIcons = ReorderIcons(currentUserIconPath, allIcons);
        SetupGrid(orderedIcons);
        SetInitialTickPosition(currentUserIconPath);
    }


    public void SetInitialTickPosition(string currentUserIconPath)
    {
        // Store the path in the class-level field
        this.currentUserIconPath = currentUserIconPath;

        // Use a small delay to allow GridLayoutGroup to finalize sizes
        Invoke(nameof(ApplyInitialTickPosition), 0.1f);
    }

    private void ApplyInitialTickPosition()
    {
        foreach (Transform child in grid.transform)
        {
            if (child == null) continue; // Skip if the child has been destroyed

            Image imgComponent = child.GetComponent<Image>();
            if (imgComponent != null && imgComponent.sprite.name == GetIconNameFromPath(currentUserIconPath))
            {
                // Set this icon as the current selected icon
                currentSelectedIcon = child.gameObject;

                // Ensure the tick image is active and attached correctly
                if (tickImage != null)
                {
                    tickImage.gameObject.SetActive(true);
                    ResizeAndPositionTick(child.gameObject);
                    Debug.Log($"[Initial Placement After Refresh] Icon Size: {currentSelectedIcon.GetComponent<RectTransform>().rect.size}, Tick Size: {tickImage.GetComponent<RectTransform>().sizeDelta}");
                }
                else
                {
                    Debug.LogError("Tick image is missing or destroyed.");
                }

                return; // Exit once the tick is placed
            }
        }

        Debug.LogWarning("No matching icon found for the current user icon path.");
    }

    public Sprite[] ReorderIcons(string currentUserIconPath, Sprite[] allIcons)
    {
        // Ensure currentUserIconPath is not null
        if (string.IsNullOrEmpty(currentUserIconPath))
        {
            Debug.LogWarning("currentUserIconPath is null or empty. Using default icon path.");
            currentUserIconPath = defaultIconPath; // Fallback to default icon path
        }

        List<Sprite> unlockedIconsList = new List<Sprite>();
        List<Sprite> lockedIconsList = new List<Sprite>();

        // Separate unlocked and locked icons with null checks
        foreach (Sprite icon in allIcons)
        {
            if (icon == null)
            {
                Debug.LogWarning("Null icon encountered in allIcons. Skipping.");
                continue; // Skip null entries
            }

            if (unlockedIcons != null && unlockedIcons.Contains(icon.name))
            {
                unlockedIconsList.Add(icon);
            }
            else
            {
                lockedIconsList.Add(icon);
            }
        }

        // Sort each list by icon name (ascending)
        unlockedIconsList.Sort((a, b) => a.name.CompareTo(b.name));
        lockedIconsList.Sort((a, b) => a.name.CompareTo(b.name));

        // Combine the lists: unlocked first, then locked
        List<Sprite> orderedIcons = new List<Sprite>(unlockedIconsList);
        orderedIcons.AddRange(lockedIconsList);

        // Move the current user icon to the top of the list
        for (int i = 0; i < orderedIcons.Count; i++)
        {
            if (orderedIcons[i] != null && orderedIcons[i].name == GetIconNameFromPath(currentUserIconPath))
            {
                // Move the current user icon to the top
                Sprite currentIcon = orderedIcons[i];
                orderedIcons.RemoveAt(i);
                orderedIcons.Insert(0, currentIcon);
                break;
            }
        }

        return orderedIcons.ToArray();
    }

    private string GetIconNameFromPath(string path)
    {
        // Extract the icon name from the path (e.g., "UILoading/CharacterImages/UserIcons/WhiteBorderIcons/00" -> "00")
        return System.IO.Path.GetFileNameWithoutExtension(path);
    }

    public void SetupGrid(Sprite[] icons)
    {
        // Get the content panel width
        float panelWidth = contentPanel.rect.width;

        // Get the number of columns from the GridLayoutGroup constraint
        int columns = grid.constraintCount;

        // Calculate icon width dynamically
        float iconWidth = panelWidth / (columns + (columns - 1) * 1f / 6 + 1);

        // Calculate padding and spacing based on icon width
        float padding = iconWidth / 2;
        float spacing = iconWidth / 6;

        // Apply padding and spacing to the GridLayoutGroup
        grid.padding.left = Mathf.RoundToInt(padding);
        grid.padding.right = Mathf.RoundToInt(padding);
        grid.spacing = new Vector2(spacing, spacing);

        // Apply calculated icon size
        grid.cellSize = new Vector2(iconWidth, iconWidth);

        // Temporarily detach the shared tickImage to avoid destroying it
        if (tickImage != null)
        {
            tickImage.transform.SetParent(null); // Detach from grid
        }

        // Clear existing children
        foreach (Transform child in grid.transform)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }

        // Reattach the shared tickImage after clearing
        if (tickImage != null)
        {
            tickImage.transform.SetParent(grid.transform); // Reattach to grid
            tickImage.gameObject.SetActive(false); // Hide initially
        }

        // Dynamically create icons
        foreach (Sprite icon in icons)
        {
            if (icon == null) continue; // Ensure the icon is not null

            GameObject newIcon = new GameObject($"Icon_{icon.name}", typeof(RectTransform), typeof(Image));
            newIcon.transform.SetParent(grid.transform, false);

            // Set the icon's image
            Image imgComponent = newIcon.GetComponent<Image>();
            imgComponent.sprite = icon;
            imgComponent.preserveAspect = true;

            // Check if the icon is locked
            if (lockedIcons != null && lockedIcons.Contains(icon.name))
            {
                // Apply grayscale effect to indicate locked state
                imgComponent.color = Color.gray; // Example: use grayscale for locked icons

                // Disable interaction for locked icons
                Button button = newIcon.AddComponent<Button>();
                button.interactable = false; // Disable interaction
            }
            else
            {
                // Add a button to allow icon selection for unlocked icons
                Button button = newIcon.AddComponent<Button>();
                button.onClick.AddListener(() => OnIconSelected(newIcon)); // Pass the GameObject reference

                // Add a tick/checkmark image as a child (only for unlocked icons)
                GameObject tick = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
                tick.transform.SetParent(newIcon.transform, false);
                RectTransform tickRect = tick.GetComponent<RectTransform>();
                tickRect.anchorMin = new Vector2(0.7f, 0.0f); // Bottom-right corner
                tickRect.anchorMax = new Vector2(1.0f, 0.3f);
                tickRect.offsetMin = Vector2.zero;
                tickRect.offsetMax = Vector2.zero;

                Image tickImage = tick.GetComponent<Image>();
                tickImage.sprite = Resources.Load<Sprite>("Ui/TickImage"); // Replace with your actual checkmark image path
                tickImage.preserveAspect = true;
                tick.SetActive(false); // Hide by default
            }
        }
    }

    private void OnIconSelected(GameObject newSelectedIcon)
    {
        // Move the tick image to the new icon's position
        if (currentSelectedIcon != null)
        {
            Debug.Log($"Tick moving from {currentSelectedIcon.name} to {newSelectedIcon.name}");
        }

        // Update the current selected icon
        currentSelectedIcon = newSelectedIcon;

        // Ensure the tick image is active
        if (!tickImage.gameObject.activeSelf)
        {
            tickImage.gameObject.SetActive(true);
        }

        // Use the helper method to resize and position the tick
        ResizeAndPositionTick(newSelectedIcon);

        // Update the selected icon sprite
        Image selectedImage = newSelectedIcon.GetComponent<Image>();
        if (selectedImage != null)
        {
            selectedIconSprite = selectedImage.sprite;
        }

        Debug.Log($"Selected Icon: {selectedIconSprite.name}");
    }

    private void ResizeAndPositionTick(GameObject targetIcon)
    {
        // Get the RectTransform of the target icon
        RectTransform iconRect = targetIcon.GetComponent<RectTransform>();
        RectTransform tickRect = tickImage.GetComponent<RectTransform>();

        // Calculate tick size as 40% of the icon width
        float tickWidth = iconRect.rect.width * 0.4f;
        tickRect.sizeDelta = new Vector2(tickWidth, tickWidth);

        // Reset the tick's local scale to avoid compounding scaling issues
        tickRect.localScale = Vector3.one;

        // Position the tick at the lower-right corner of the target icon
        tickRect.SetParent(targetIcon.transform, false); // Set tick as a child of the target icon
        tickRect.anchorMin = new Vector2(1f, 0f); // Lower-right corner
        tickRect.anchorMax = new Vector2(1f, 0f); // Lower-right corner
        tickRect.anchoredPosition = new Vector2(-tickWidth / 2, tickWidth / 2); // Adjust position slightly inward
        tickRect.pivot = new Vector2(0.5f, 0.5f); // Adjust pivot for proper placement
    }

    public Sprite GetSelectedIcon()
    {
        return selectedIconSprite; // Return the selected icon's Sprite
    }
}