using UnityEngine;
using UnityEngine.UI;
using System;
using model;

public class GoldPurchase : MonoBehaviour
{
    public GameObject claimButton;
    public GameObject adTV;
    public GameObject buyGold1Button; // 10 diamonds -> 300 gold
    public GameObject buyGold2Button; // 90 diamonds -> 1000 gold
    public GameObject buyGold3Button; // 200 diamonds -> 5000 gold

    private void Start()
    {
        // PlayerPrefs.SetInt("Diamond", 500); // 🔧 TEMP: give 500 diamonds for testing

        // Hide claim button if already claimed today
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        string lastClaimDate = PlayerPrefs.GetString("GoldClaimDate", "");        

        if (lastClaimDate == today)
        {
            claimButton?.SetActive(false);
        }
        else
        {
            claimButton?.SetActive(true);
        }

        // Hide buygold1 button if already bought today
        string lastPurchaseDate = PlayerPrefs.GetString("BuyGold1Date", "");

        if (lastPurchaseDate == today)
            buyGold1Button.SetActive(false);
        else
            buyGold1Button.SetActive(true);

        // Monthly card status to control ad TV
        string expiryDateStr = PlayerPrefs.GetString("MonthlyCardExpiry", "");
        if (DateTime.TryParse(expiryDateStr, out DateTime expiryDate) && DateTime.Now < expiryDate)
        {
            adTV?.SetActive(false); // Monthly card active
        }
        else
        {
            adTV?.SetActive(true);  // Monthly card expired or not bought
        }
    }

    public void ClaimFreeGold()
    {
        string expiryString = PlayerPrefs.GetString("MonthlyCardExpiry", "");
        bool isMonthlyCardActive = false;
        if (DateTime.TryParse(expiryString, out DateTime expiryDate))
        {
            isMonthlyCardActive = DateTime.Now < expiryDate;
        }

        Debug.Log("Monthly card active: " + isMonthlyCardActive);

        if (isMonthlyCardActive)
        {
            GrantGoldReward();
        }
        else
        {
            Debug.Log("Show reward ad video to claim reward!");
            if (GoogleMobileAdsScript.This.CheckRewardedAd())
            {
                GoogleMobileAdsScript.This.ShowRewardedAd("gold_reward", GrantGoldReward);
            }
            else
            {
                Debug.Log("Rewarded ad not available, showing interstitial ad instead.");
                GoogleMobileAdsScript.This.ShowInterstitialAd(GrantGoldReward);
            }
        }
    }

    public void BuyGold_300()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        string lastPurchaseDate = PlayerPrefs.GetString("Gold10_LastPurchase", "");

        if (today == lastPurchaseDate)
        {
            Debug.Log("You have already bought this deal today.");
            return;
        }

        int diamonds = PlayerPrefs.GetInt("Diamond", 0);
        if (diamonds >= 10)
        {
            int gold = PlayerPrefs.GetInt("GoldCoin", 0);

            PlayerPrefs.SetInt("Diamond", diamonds - 10);
            PlayerPrefs.SetInt("GoldCoin", gold + 300);
            PlayerPrefs.SetString("Gold10_LastPurchase", today);
            PlayerPrefs.Save();

            PlayerProfile.Data.Player.Diamond -= 10;
            PlayerProfile.Data.Player.GoldCoin += 300;
            PlayerProfile.Data.SetPlayer(PlayerProfile.Data.Player); // Triggers listeners

            FindObjectOfType<IndexDynamicSize>()?.RefreshDiamondDisplay();
            FindObjectOfType<Row1GroupDynamicSize>()?.RefreshCurrencyDisplay();

            buyGold1Button.SetActive(false);
            Debug.Log("Bought 300 gold with 10 diamonds.");
        }
        else
        {
            Debug.Log("Not enough diamonds.");
        }
    }

    public void BuyGold_1000()
    {
        int diamonds = PlayerPrefs.GetInt("Diamond", 0);
        if (diamonds >= 90)
        {
            int gold = PlayerPrefs.GetInt("GoldCoin", 0);

            PlayerPrefs.SetInt("Diamond", diamonds - 90);
            PlayerPrefs.SetInt("GoldCoin", gold + 1000);
            PlayerPrefs.Save();

            PlayerProfile.Data.Player.Diamond -= 90;
            PlayerProfile.Data.Player.GoldCoin += 1000;
            PlayerProfile.Data.SetPlayer(PlayerProfile.Data.Player);

            FindObjectOfType<IndexDynamicSize>()?.RefreshDiamondDisplay();
            FindObjectOfType<Row1GroupDynamicSize>()?.RefreshCurrencyDisplay();

            Debug.Log("Bought 1000 gold with 90 diamonds.");
        }
        else
        {
            Debug.Log("Not enough diamonds.");
        }
    }

    public void BuyGold_5000()
    {
        int diamonds = PlayerPrefs.GetInt("Diamond", 0);
        if (diamonds >= 200)
        {
            int gold = PlayerPrefs.GetInt("GoldCoin", 0);

            PlayerPrefs.SetInt("Diamond", diamonds - 200);
            PlayerPrefs.SetInt("GoldCoin", gold + 5000);
            PlayerPrefs.Save();

            PlayerProfile.Data.Player.Diamond -= 200;
            PlayerProfile.Data.Player.GoldCoin += 5000;
            PlayerProfile.Data.SetPlayer(PlayerProfile.Data.Player);

            FindObjectOfType<IndexDynamicSize>()?.RefreshDiamondDisplay();
            FindObjectOfType<Row1GroupDynamicSize>()?.RefreshCurrencyDisplay();

            Debug.Log("Bought 5000 gold with 200 diamonds.");
        }
        else
        {
            Debug.Log("Not enough diamonds.");
        }
    }

    private void GrantGoldReward()
    {
        int currentGold = PlayerPrefs.GetInt("GoldCoin", 0);
        int newGold = currentGold + 500;

        PlayerPrefs.SetInt("GoldCoin", newGold);
        PlayerPrefs.SetString("GoldClaimDate", DateTime.Now.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();

        PlayerProfile.Data.Player.GoldCoin = newGold;
        PlayerProfile.Data.SetPlayer(PlayerProfile.Data.Player);

        Debug.Log("✅ 500 gold granted. Total: " + newGold);
        claimButton?.SetActive(false);
    }
}
