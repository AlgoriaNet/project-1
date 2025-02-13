using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public Button[] buttons; // Assign buttons in the inspector
    public GameObject[] menus; // Assign corresponding menus in the inspector
    public ChestManager chestManager; 

    // Special UI elements for Menu 1 case (AlliesMenue)
    public GameObject step1Panel;
    public GameObject step2Panel;
    public GameObject lowerGroup;
    private int currentMenuIndex = -1; // Default to -1 (no menu active)

    private void Start()
    {
        // Add listeners to buttons
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // Capture the index for the lambda   
            buttons[i].onClick.AddListener(() => ToggleMenu(index));
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