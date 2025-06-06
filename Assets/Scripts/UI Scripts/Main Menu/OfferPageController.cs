using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfferPageController : MonoBehaviour
{
    public GameObject commonPage;
    public GameObject mainPanel;
    public GameObject offerPage;
    private int selectedPage; // Track the current page

    public GameObject TopUpPage;
    public GameObject DailyOffersPage;
    public GameObject GrowthFundPage;
    public GameObject BattlePassPage;

    // References to the button objects
    public GameObject TopUpButton;
    public GameObject DailyOffersButton;
    public GameObject GrowthFundButton;
    public GameObject BattlePassButton;

    // References to PassItemLoader scripts for pages
    public PassItemLoader battlePassLoader;   // Script for BattlePassPage
    public PassItemLoader growthFundLoader;  // Script for GrowthFundPage

    public void OpenOfferPage()
    {
        offerPage.SetActive(true);
        commonPage.SetActive(false);
        mainPanel.SetActive(false);

        // Update dot statuses when opening the page
        UpdateDotStatuses();
    }

    public void CloseOfferPage()
    {
        offerPage.SetActive(false);
        commonPage.SetActive(true);
        mainPanel.SetActive(true);
    }

    public void ToggleOfferPage(int selectedIndex)
    {
        // Track selected page
        selectedPage = selectedIndex;

        // Handle Button visuals (toggle orange/grey buttons on the button objects)
        if (TopUpButton != null)
        {
            TopUpButton.transform.Find("OrangeButton")?.gameObject.SetActive(selectedIndex == 1);
            TopUpButton.transform.Find("GreyButton")?.gameObject.SetActive(selectedIndex != 1);
        }
        
        if (DailyOffersButton != null)
        {
            DailyOffersButton.transform.Find("OrangeButton")?.gameObject.SetActive(selectedIndex == 2);
            DailyOffersButton.transform.Find("GreyButton")?.gameObject.SetActive(selectedIndex != 2);
        }
        
        if (GrowthFundButton != null)
        {
            GrowthFundButton.transform.Find("OrangeButton")?.gameObject.SetActive(selectedIndex == 3);
            GrowthFundButton.transform.Find("GreyButton")?.gameObject.SetActive(selectedIndex != 3);
        }

        if (BattlePassButton != null)
        {
            BattlePassButton.transform.Find("OrangeButton")?.gameObject.SetActive(selectedIndex == 4);
            BattlePassButton.transform.Find("GreyButton")?.gameObject.SetActive(selectedIndex != 4);
        }

        // Set multipliers for PassItemLoader instances
        if (growthFundLoader != null && selectedIndex == 3)
        {
            growthFundLoader.levelMultiplier = 5; // Use multiplier 5 for GrowthFundPage
        }
        if (battlePassLoader != null && selectedIndex == 4)
        {
            battlePassLoader.levelMultiplier = 1; // Default multiplier 1 for BattlePassPage
        }

        // Toggle page visibility
        if (TopUpPage != null) TopUpPage.SetActive(selectedIndex == 1);
        if (DailyOffersPage != null) DailyOffersPage.SetActive(selectedIndex == 2);
        if (GrowthFundPage != null) GrowthFundPage.SetActive(selectedIndex == 3);
        if (BattlePassPage != null) BattlePassPage.SetActive(selectedIndex == 4);

        // Ensure all buttons remain visible regardless of which page is selected
        if (TopUpButton != null) TopUpButton.SetActive(true);
        if (DailyOffersButton != null) DailyOffersButton.SetActive(true);
        if (GrowthFundButton != null) GrowthFundButton.SetActive(true);
        if (BattlePassButton != null) BattlePassButton.SetActive(true);

        // Update dot statuses
        UpdateDotStatuses();
    }

    // Method to update dot statuses
    public void UpdateDotStatuses()
    {
        Debug.Log($"Dot statuses updated for page {selectedPage}");
    }
}
