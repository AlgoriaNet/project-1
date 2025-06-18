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
        builder.AddProduct("hero_2999", ProductType.Consumable);
        builder.AddProduct("hero_4999", ProductType.Consumable);
        builder.AddProduct("hero_9999", ProductType.Consumable);

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
        Debug.Log("Purchase successful: " + args.purchasedProduct.definition.id);

        string receipt = args.purchasedProduct.receipt;
        _wsSocketApi.Action("payment", new
        {
            product_id = args.purchasedProduct.definition.id,
            receipt = receipt,
            platform = Application.platform.ToString()
        }, (response) =>
        {
            Debug.Log("Payment verified by backend: " + response.ToString());
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