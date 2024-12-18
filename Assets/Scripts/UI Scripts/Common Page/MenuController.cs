using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public Button[] buttons; // Assign buttons in the inspector
    public GameObject[] menus; // Assign corresponding menus in the inspector

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
        // Loop through all menus
        for (int i = 0; i < menus.Length; i++)
        {
            menus[i].SetActive(i == activeMenuIndex); // Enable only the selected menu
        }
    }
}