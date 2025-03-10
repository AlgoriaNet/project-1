using UnityEngine;

public class monthlyCardController : MonoBehaviour
{
    public GameObject monthlyCardPage;  // Assign monthlyCardPage in Inspector
    public GameObject commonPage;
    public GameObject mainPanel;

    public void OpenmonthlyCardPage()
    {
        monthlyCardPage.SetActive(true);
        commonPage.SetActive(false);
        mainPanel.SetActive(false);
    }

    public void ClosemonthlyCardPage()
    {
        monthlyCardPage.SetActive(false);
        commonPage.SetActive(true);
        mainPanel.SetActive(true);
    }
}