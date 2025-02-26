using UnityEngine;
using UnityEngine.UI;

public class SwitchPanels : MonoBehaviour
{
    public Button button;   // Assign the button in Inspector
    public GameObject packPanel;  // Assign the Pack panel in Inspector
    public GameObject boardPanel; // Assign the Board panel in Inspector

    private void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(SwitchToPack);
        }
    }

    private void SwitchToPack()
    {
        if (packPanel != null) packPanel.SetActive(true);
        if (boardPanel != null) boardPanel.SetActive(false);
    }
}