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
        DiamondPurchase.BuyDiamonds("card_999", 680);
        PlayerProfile.Data.Player.WeeklyCardExpiry = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");
        PlayerProfile.Data.NotifyListeners("Player");
    }

    private void BuyMonthlyCard()
    {
        DiamondPurchase.BuyDiamonds("card_2999", 2040);
        PlayerProfile.Data.Player.MonthlyCardExpiry = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd");
        PlayerProfile.Data.NotifyListeners("Player");
    }
}