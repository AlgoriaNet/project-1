using UnityEngine;
using UnityEngine.UI;
using System;
using model;
using Newtonsoft.Json.Linq;
using WebSocket;

public class GoldPurchase : MonoBehaviour
{
    public GameObject claimButton;
    public GameObject adTV;
    public GameObject buyGold1Button; // 10 diamonds -> 300 gold
    public GameObject buyGold2Button; // 90 diamonds -> 1000 gold
    public GameObject buyGold3Button; // 200 diamonds -> 5000 gold
    private static PurchaseWebSocketApi _wsSocketApi;


    private void Start()
    {
        // PlayerPrefs.SetInt("Diamond", 500); // 🔧 TEMP: give 500 diamonds for testing

        _wsSocketApi = PurchaseWebSocketApi.Instance;
        string today = DateTime.Now.ToString("yyyy-MM-dd");

        // Hide claim button if already claimed today
        string lastClaimDate = PlayerPrefs.GetString("GoldClaimDate", "");
        claimButton?.SetActive(lastClaimDate != today);

        // Hide buygold1 button if already bought today
        string lastPurchaseDate = PlayerPrefs.GetString("Gold10_LastPurchase", "");
        buyGold1Button?.SetActive(lastPurchaseDate != today);
    }

    public void ClaimFreeGold()
    {
        // string expiryString = PlayerPrefs.GetString("MonthlyCardExpiry", "");
        // bool isMonthlyCardActive = false;
        // if (DateTime.TryParse(expiryString, out DateTime expiryDate))
        // {
        //     isMonthlyCardActive = DateTime.Now < expiryDate;
        // }

        string expiryString = PlayerProfile.Data?.Player?.MonthlyCardExpiry 
                            ?? PlayerPrefs.GetString("MonthlyCardExpiry", "");

        bool isMonthlyCardActive = DateTime.TryParse(expiryString, out DateTime expiryDate) 
                                && DateTime.Now < expiryDate;

        Debug.Log("Monthly card active: " + isMonthlyCardActive);

        if (isMonthlyCardActive)
        {
            adTV?.SetActive(false); // Monthly card active
            GrantGoldReward();
        }
        else
        {
            adTV?.SetActive(true); // Monthly card expired or not bought
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

    private void GrantGoldReward()
    {
        _wsSocketApi.Action("add_gold", new { type = "500" }, AfterByPurchase);
        claimButton?.SetActive(false);
    
        // ADD THIS LINE:
        PlayerPrefs.SetString("GoldClaimDate", DateTime.Now.ToString("yyyy-MM-dd"));
    
        Debug.Log("Granted 500 gold successfully.");
    }

    public void BuyGold_300()
    {
        // Debug checks to find the issue
        if (PlayerProfile.Data == null)
        {
            Debug.LogError("PlayerProfile.Data is null!");
            return;
        }
        if (PlayerProfile.Data.Player == null)
        {
            Debug.LogError("PlayerProfile.Data.Player is null!");
            return;
        }

        int diamonds = PlayerProfile.Data.Player.Diamond;
        if (diamonds >= 10)
        {
            _wsSocketApi.Action("add_gold", new { type = "300" }, AfterByPurchase);
            
            buyGold1Button.SetActive(false);
            // Update the date for next session
            PlayerPrefs.SetString("Gold10_LastPurchase", DateTime.Now.ToString("yyyy-MM-dd"));      

            Debug.Log("Bought 300 gold with 10 diamonds.");
        }
        else
        {
            Debug.Log("Not enough diamonds.");
        }
    }

    public void BuyGold_1200()
    {
        int diamonds = PlayerProfile.Data.Player.Diamond;
        if (diamonds >= 90)
        {
            _wsSocketApi.Action("add_gold", new { type = "1100" }, AfterByPurchase);
            Debug.Log("Bought 1000 gold with 90 diamonds.");
        }
        else
        {
            Debug.Log("Not enough diamonds.");
        }
    }

    public void BuyGold_4000()
    {
        int diamonds = PlayerProfile.Data.Player.Diamond; 
        if (diamonds >= 200)
        {
            _wsSocketApi.Action("add_gold", new { type = "4000" }, AfterByPurchase);
            Debug.Log("Bought 5000 gold with 200 diamonds.");
        }
        else
        {
            Debug.Log("Not enough diamonds.");
        }
    }

    public void AfterByPurchase(JObject _object)
    {
        // 处理购买后的响应
        int diamond = _object.GetValue("diamond").Value<int>();
        int gold = _object.GetValue("gold").Value<int>();

        // Update Player model only - with protection
        try
        {
            PlayerProfile.Data.Player.Diamond = diamond;
            PlayerProfile.Data.Player.GoldCoin = gold;
            PlayerProfile.Data.NotifyListeners("Player");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to update player profile: {e.Message}");
            // Continue execution anyway
        }
    }
}
