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
    public GameObject buyGold2Button; // 90 diamonds -> 1200 gold
    public GameObject buyGold3Button; // 200 diamonds -> 4000 gold
    private static PurchaseWebSocketApi _wsSocketApi;
    [SerializeField] private DiamondStore diamondStore;


    private void Start()
    {
        // PlayerPrefs.SetInt("Diamond", 500); // 🔧 TEMP: give 500 diamonds for testing

        _wsSocketApi = PurchaseWebSocketApi.Instance;
        string today = DateTime.Now.ToString("yyyy-MM-dd");

        // Hide claim button if already claimed today
        string lastClaimDate = PlayerPrefs.GetString("GoldClaimDate", "");
        // claimButton?.SetActive(lastClaimDate != today);
        bool canClaimGold = lastClaimDate != today;
        SetClaimButtonState(claimButton, canClaimGold);

        // Hide buygold1 button if already bought today
        string lastPurchaseDate = PlayerPrefs.GetString("Gold10_LastPurchase", "");
        // buyGold1Button?.SetActive(lastPurchaseDate != today);
        bool canBuyGold1 = lastPurchaseDate != today;
        SetClaimButtonState(buyGold1Button, canBuyGold1);
    }

    private void SetClaimButtonState(GameObject buttonObj, bool isEnabled)
    {
        var btn = buttonObj.GetComponent<Button>();
        if (btn != null) btn.interactable = isEnabled;

        var canvasGroup = buttonObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = buttonObj.AddComponent<CanvasGroup>();

        canvasGroup.alpha = isEnabled ? 1f : 0.7f;
    }

    public void ClaimFreeGold()
    {
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
            diamondStore.Open();
            Debug.Log("Not enough diamonds.");
        }
    }

    public void BuyGold_1200()
    {
        int diamonds = PlayerProfile.Data.Player.Diamond;
        if (diamonds >= 90)
        {
            _wsSocketApi.Action("add_gold", new { type = "1200" }, AfterByPurchase);
            Debug.Log("Bought 1200 gold with 90 diamonds.");
        }
        else
        {
            diamondStore.Open();
            Debug.Log("Not enough diamonds.");
        }
    }

    public void BuyGold_4000()
    {
        int diamonds = PlayerProfile.Data.Player.Diamond;
        if (diamonds >= 200)
        {
            _wsSocketApi.Action("add_gold", new { type = "4000" }, AfterByPurchase);
            Debug.Log("Bought 4000 gold with 200 diamonds.");
        }
        else
        {
            diamondStore.Open();
            Debug.Log("Not enough diamonds.");
        }
    }

    public void AfterByPurchase(JObject _object)
    {
        // 处理购买后的响应
        int diamond = _object.GetValue("diamond").Value<int>();
        int gold = _object.GetValue("gold").Value<int>();

        Debug.Log($"AfterByPurchase: diamond={diamond}, gold={gold}");
        Debug.Log(
            $"PlayerProfile.Data.Player.Diamond={PlayerProfile.Data.Player.Diamond}, PlayerProfile.Data.Player.GoldCoin={PlayerProfile.Data.Player.GoldCoin}");
        // Update Player model only - with protection
        PlayerProfile.Data.Player.Diamond = diamond;
        PlayerProfile.Data.Player.GoldCoin = gold;
        PlayerProfile.Data.NotifyListeners("Player");
    }
}