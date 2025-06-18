using UnityEngine;
using UnityEngine.UI;
using System;
using model;
using Newtonsoft.Json.Linq;
using WebSocket;

public class DiamondPurchase : MonoBehaviour
{
    public Button[] purchaseButtons;

    private readonly (string productId, int amount)[] diamondOptions = new[]
    {
        ("hero_99", 60),
        ("hero_499", 330),
        ("hero_999", 680),
        ("hero_1999", 1480),
        ("hero_4999", 3780),
        ("hero_9999", 7980),
        ("card_999", 680),
        ("card_2999", 2040),
    };

    private static PurchaseWebSocketApi _wsSocketApi;

    private void Start()
    {
        _wsSocketApi = PurchaseWebSocketApi.Instance;

        for (int i = 0; i < purchaseButtons.Length && i < diamondOptions.Length; i++)
        {
            int index = i;
            purchaseButtons[i].onClick.AddListener(() =>
                BuyDiamondsInstance(diamondOptions[index].productId));
        }
    }

    private void BuyDiamondsInstance(string productId)
    {
        _wsSocketApi.Action("add_diamond", new { type = productId }, AfterByPurchase);
    }

    public static void BuyDiamonds(string productId, int diamondAmount)
    {
        Debug.Log($"Static diamond purchase: {productId}, {diamondAmount}");
        PurchaseWebSocketApi.Instance.Action("add_diamond", new { type = productId }, StaticAfterByPurchase);
    }

    private void AfterByPurchase(JObject _object)
    {
        UpdateDiamond(_object);
    }

    private static void StaticAfterByPurchase(JObject _object)
    {
        UpdateDiamond(_object);
    }

    private static void UpdateDiamond(JObject _object)
    {
        try
        {
            int diamond = _object.GetValue("diamond").Value<int>();
            PlayerProfile.Data.Player.Diamond = diamond;
            PlayerProfile.Data.NotifyListeners("Player");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update diamond: {e.Message}");
        }
    }
}
