using UnityEngine;

public class BaseMenuController : MonoBehaviour
{
    public GameObject commonPage;
    public GameObject rewardsPage; // Assign RewardsPage in Inspector

    public void OpenRewardsPage()
    {
        rewardsPage.SetActive(true);
        commonPage.SetActive(false); // Hide common page
    }

    public void CloseRewardsPage()
    {
        rewardsPage.SetActive(false);
        commonPage.SetActive(true); 
    }
}