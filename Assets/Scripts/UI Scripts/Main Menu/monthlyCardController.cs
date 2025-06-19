using UnityEngine;
using UnityEngine.UI;
using System;
using model;

public class MonthlyCardController : MonoBehaviour
{
    public GameObject monthlyCardPage;
    public GameObject commonPage;
    public GameObject mainPanel;

    public Button buyWeeklyCardButton;
    public Button buyMonthlyCardButton;

    private void Start()
    {
        buyWeeklyCardButton.onClick.AddListener(BuyWeeklyCard);
        buyMonthlyCardButton.onClick.AddListener(BuyMonthlyCard);
    }

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

    private void BuyWeeklyCard()
    {
        // Use the new IAP system instead of the old DiamondPurchase.BuyDiamonds
        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.BuyProduct("card_999");
            
            // Update expiry date - this should ideally be moved to happen after successful purchase
            // but keeping the same logic as before for now
            UpdateWeeklyCardExpiry();
        }
        else
        {
            Debug.LogError("IAPManager instance not found!");
        }
    }

    private void BuyMonthlyCard()
    {
        // Use the new IAP system instead of the old DiamondPurchase.BuyDiamonds
        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.BuyProduct("card_2999");
            
            // Update expiry date - this should ideally be moved to happen after successful purchase
            // but keeping the same logic as before for now
            UpdateMonthlyCardExpiry();
        }
        else
        {
            Debug.LogError("IAPManager instance not found!");
        }
    }

    private void UpdateWeeklyCardExpiry()
    {
        var now = DateTime.Now.Date;
        var expiry = DateTime.TryParse(PlayerProfile.Data.Player.WeeklyCardExpiry, out var old) && old >= now
            ? old.AddDays(7)
            : now.AddDays(6); // inclusive

        PlayerProfile.Data.Player.WeeklyCardExpiry = expiry.ToString("yyyy-MM-dd");
        PlayerProfile.Data.NotifyListeners("Player");
    }

    private void UpdateMonthlyCardExpiry()
    {
        var now = DateTime.Now.Date;
        var expiry = DateTime.TryParse(PlayerProfile.Data.Player.MonthlyCardExpiry, out var old) && old >= now
            ? old.AddDays(30)
            : now.AddDays(29); // inclusive

        PlayerProfile.Data.Player.MonthlyCardExpiry = expiry.ToString("yyyy-MM-dd");
        PlayerProfile.Data.NotifyListeners("Player");
    }
}