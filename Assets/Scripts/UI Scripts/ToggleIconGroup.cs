using UnityEngine;

public class ToggleIconGroup : MonoBehaviour
{
    public GameObject buttonGroup; // Assign Icon Group in the Inspector

    private bool isVisible = true;

    public void ToggleVisibility()
    {
        isVisible = !isVisible;

        // Show or hide the Icon Group
        buttonGroup.SetActive(isVisible);
    }
}