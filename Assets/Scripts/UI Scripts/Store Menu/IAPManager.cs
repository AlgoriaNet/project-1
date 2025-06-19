using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using UnityEngine;
using System;
using System.Collections.Generic;
using WebSocket;

public class IAPManager : MonoBehaviour, IStoreListener
{
    private static IStoreController storeController;
    private static IExtensionProvider extensionProvider;

    public static IAPManager Instance;

    private static PurchaseWebSocketApi _wsSocketApi;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _wsSocketApi = PurchaseWebSocketApi.Instance;
        InitializePurchasing();
    }

    public void InitializePurchasing()
    {
        if (IsInitialized()) return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct("hero_99", ProductType.Consumable);
        builder.AddProduct("hero_499", ProductType.Consumable);
        builder.AddProduct("hero_999", ProductType.Consumable);
        builder.AddProduct("hero_1999", ProductType.Consumable);
        builder.AddProduct("hero_4999", ProductType.Consumable);
        builder.AddProduct("hero_9999", ProductType.Consumable);
        builder.AddProduct("card_999", ProductType.Consumable);
        builder.AddProduct("card_2999", ProductType.Consumable);

        UnityPurchasing.Initialize(this as IDetailedStoreListener, builder);
    }

    private bool IsInitialized()
    {
        return storeController != null && extensionProvider != null;
    }

    public void BuyProduct(string productId)
    {
        if (!IsInitialized()) return;

        Product product = storeController.products.WithID(productId);
        if (product != null && product.availableToPurchase)
        {
            storeController.InitiatePurchase(product);
        }
        else
        {
            Debug.LogError("BuyProduct: Product not found or not available.");
        }
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        var product = args.purchasedProduct;
        var productId = product.definition.id;
        var receipt = product.receipt;
        var platform = Application.platform.ToString();
        var isSandbox = Debug.isDebugBuild;
        var payTime = DateTime.UtcNow.ToString("o");

        // Fetch real store price and currency
        var money = product.metadata.localizedPrice.ToString(); // e.g., "0.99"
        var currency = product.metadata.isoCurrencyCode;         // e.g., "USD"

        Debug.Log($"Purchase successful: {productId} - {money} {currency}");

        // Step 1: payment
        PurchaseWebSocketApi.Instance.Action("payment", new
        {
            product_id = productId,
            receipt = receipt,
            platform = "google", // or "apple" based on store
            is_sandbox = isSandbox,
            money = money,
            currency = currency
        }, (response) =>
        {
            var orderId = response["order_id"]?.ToString();
            if (string.IsNullOrEmpty(orderId))
            {
                Debug.LogError("Missing order_id from backend.");
                return;
            }

            // Step 2: payment_callback
            PurchaseWebSocketApi.Instance.Action("payment_callback", new
            {
                product_id = productId,
                money = money,
                currency = currency,
                platform = "google",
                is_sandbox = isSandbox,
                order_id = orderId,
                platform_order_id = product.transactionID ?? Guid.NewGuid().ToString(),
                receipt_data = receipt,
                pay_time = payTime
            }, (callbackResponse) =>
            {
                Debug.Log("payment_callback response: " + callbackResponse.ToString());

                // You can parse rewards or player info here
            });
        });

        return PurchaseProcessingResult.Complete;
    }
  
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        extensionProvider = extensions;
        Debug.Log("IAP initialized.");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError("IAP Init Failed: " + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"IAP Init Failed: {error} - {message}");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError("Purchase failed: " + failureReason);
    }
}