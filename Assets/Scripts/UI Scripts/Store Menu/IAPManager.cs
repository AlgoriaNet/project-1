// using UnityEngine;
// using UnityEngine.Purchasing;
// using UnityEngine.Purchasing.Security;
// using System;
// using model;
// using UnityEngine.Purchasing.Extension;
// using WebSocket;

// public class IAPManager : MonoBehaviour, IDetailedStoreListener
// {
//     private static IStoreController m_StoreController;
//     private static IExtensionProvider m_StoreExtensionProvider;
//     public static IAPManager Instance { get; private set; }

//     // 商品ID常量定义
//     public const string Hero99 = "hero_99";
//     public const string Hero499 = "hero_499";
//     public const string Hero999 = "hero_999";
//     // 添加其他商品ID...

//     // 当前正在处理的支付回调（用于支付成功后处理）
//     private Action<bool> m_CurrentPurchaseCallback;
//     private String m_CurrentOrderId;

//     void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }

//         Instance = this;
//         DontDestroyOnLoad(gameObject);
//         InitializePurchasing();
//     }

//     void InitializePurchasing()
//     {
//         if (IsInitialized()) return;

//         var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

//         builder.AddProduct("hero_99", ProductType.Consumable);
//         builder.AddProduct("hero_499", ProductType.Consumable);
//         builder.AddProduct("hero_999", ProductType.Consumable);
//         builder.AddProduct("hero_1999", ProductType.Consumable);
//         builder.AddProduct("hero_4999", ProductType.Consumable);
//         builder.AddProduct("hero_9999", ProductType.Consumable);
//         builder.AddProduct("card_999", ProductType.Consumable);
//         builder.AddProduct("card_2999", ProductType.Consumable);

// #if UNITY_EDITOR
//         // 在编辑器中使用FakeStore进行测试
//         StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
// #endif

//         UnityPurchasing.Initialize(this, builder);
//     }

//     private bool IsInitialized()
//     {
//         return m_StoreController != null && m_StoreExtensionProvider != null;
//     }

//     /// <summary>
//     /// 发起购买（完整流程）
//     /// </summary>
//     public void PurchaseProduct(string productId, Action<bool> callback)
//     {
//         if (!IsInitialized())
//         {
//             Debug.LogError("IAP not initialized!");
//             callback?.Invoke(false);
//             return;
//         }

//         // 1. 支付前调用服务器预校验
//         Debug.Log("Starting payment pre-check with server...");

//         // 这里替换为你的实际WebSocket API调用
//         PurchaseWebSocketApi.Instance.Action("payment", new
//         {
//             product_id = productId,
//             platform = Application.platform.ToString(),
//             is_sandbox = Application.isEditor // 在编辑器中模拟沙盒环境
//         }, (result) =>
//         {
//             Debug.Log("Server pre-check passed, starting purchase...");

//             // 保存回调用于支付完成处理
//             m_CurrentPurchaseCallback = callback;
//             m_CurrentOrderId = (string)result.GetValue("order_id"); // 假设服务器返回了一个订单ID
//             // 2. 发起实际支付
//             Product product = m_StoreController.products.WithID(productId);
//             if (product != null && product.availableToPurchase)
//             {
//                 m_StoreController.InitiatePurchase(product);
//             }
//             else
//             {
//                 Debug.LogError($"Product {productId} not available");
//                 callback?.Invoke(false);
//                 m_CurrentPurchaseCallback = null;
//             }
//         });
//     }

//     /// <summary>
//     /// 支付成功回调
//     /// </summary>
//     public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
//     {
//         Debug.Log($"Purchase successful: {args.purchasedProduct.definition.id}");

//         // 3. 支付成功后验证收据
//         try
//         {
//             // 在真实设备上验证收据
//             if (!Application.isEditor)
//             {
//                 // var validator = new CrossPlatformValidator(
//                 //     GooglePlayTangle.Data(), 
//                 //     AppleTangle.Data(), 
//                 //     Application.identifier);
//                 //
//                 // // 这会验证收据的真实性，如果无效会抛出异常
//                 // IPurchaseReceipt[] result = validator.Validate(args.purchasedProduct.receipt);
//                 // Debug.Log("Receipt validated successfully");
//             }

//             // 4. 调用服务器完成支付
//             Debug.Log("Sending receipt to server for verification...");

//             // 这里替换为你的实际WebSocket API调用
//             PurchaseWebSocketApi.Instance.Action("callback", new
//             {
//                 product_id = args.purchasedProduct.definition.id,
//                 receipt_data = args.purchasedProduct.receipt,
//                 money = args.purchasedProduct.metadata.localizedPrice,
//                 currency = args.purchasedProduct.metadata.isoCurrencyCode,
//                 order_id = m_CurrentOrderId,
//                 platform = Application.platform.ToString(),
//                 is_sandbox = Application.isEditor, 
//                 platform_order_id = args.purchasedProduct.transactionID // 交易ID
//             }, (result) =>
//             {
//                 Debug.Log("Payment fully completed on server!");
//                 // 发放游戏内物品...
//                 m_CurrentPurchaseCallback?.Invoke(true);
//                 m_CurrentPurchaseCallback = null;
//                 m_CurrentOrderId = null;
//                 PlayerProfile.Data.SetPlayer(result["Player"].ToObject<Player>());
//             });

//             return PurchaseProcessingResult.Complete;
//         }
//         catch (IAPSecurityException ex)
//         {
//             Debug.LogError($"Invalid receipt: {ex}");
//             m_CurrentPurchaseCallback?.Invoke(false);
//             m_CurrentPurchaseCallback = null;
//             return PurchaseProcessingResult.Complete;
//         }
//     }

//     // ========== IAP回调接口 ==========
//     public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
//     {
//         Debug.Log("IAP initialized successfully");
//         m_StoreController = controller;
//         m_StoreExtensionProvider = extensions;
//     }

//     public void OnInitializeFailed(InitializationFailureReason error)
//     {
//         Debug.LogError($"IAP Initialization Failed: {error}");
//     }

//     public void OnInitializeFailed(InitializationFailureReason error, string message)
//     {
//         Debug.LogError($"IAP Initialization Failed: {error} - {message}");
//     }

//     public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
//     {
//         Debug.LogError($"Purchase failed - Product: {product.definition.id}, Reason: {failureReason}");
//         m_CurrentPurchaseCallback?.Invoke(false);
//         m_CurrentPurchaseCallback = null;
//     }

//     public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
//     {
//         Debug.LogError(
//             $"Purchase failed - Product: {product.definition.id}, Reason: {failureDescription.reason}, Message: {failureDescription.message}");
//         m_CurrentPurchaseCallback?.Invoke(false);
//         m_CurrentPurchaseCallback = null;
//     }

//     // ========== 辅助方法 ==========
//     public string GetLocalizedPrice(string productId)
//     {
//         if (!IsInitialized()) return string.Empty;
//         Product product = m_StoreController.products.WithID(productId);
//         return product?.metadata.localizedPriceString ?? string.Empty;
//     }

//     public string GetLocalizedTitle(string productId)
//     {
//         if (!IsInitialized()) return string.Empty;
//         Product product = m_StoreController.products.WithID(productId);
//         return product?.metadata.localizedTitle ?? string.Empty;
//     }
// }


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