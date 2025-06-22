using UnityEngine;
using UnityEngine.UI;

public class DiamondPurchase : MonoBehaviour
{
    public Button[] purchaseButtons;

    private readonly string[] productIds = new[]
    {
        "hero_99", "hero_499", "hero_999", "hero_1999",
        "hero_4999", "hero_9999", "card_999", "card_2999"
    };

    private void Start()
    {
        for (int i = 0; i < purchaseButtons.Length && i < productIds.Length; i++)
        {
            int index = i;
            purchaseButtons[i].onClick.AddListener(() =>
            {
                IAPManager.Instance.PurchaseProduct(productIds[index], null);
            });
        }
    }
}