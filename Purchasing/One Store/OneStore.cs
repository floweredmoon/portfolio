using IapError;
using IapResponse;
using Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

//
// [IMPLEMENT]
// https://github.com/ONE-store/inapp-sdk/wiki/IAP-Reference-Result-Code
// 
public class OneStore : Singleton<OneStore>, IStore
{

    // [IMPLEMENT]
    // -> Command Line Argument
    public const string APPLICATION_ID = "OA00716555";

    #region Fields

    private IStoreCallback callback;
    private AndroidJavaObject androidJavaObject;
    // UnityPurchasing.Initialize(ConfigurationBuilder)
    // 
    private ReadOnlyCollection<ProductDefinition> productDefinitions;
    // 
    private Dictionary<string, IapResponse.Product> availableProducts = new Dictionary<string, IapResponse.Product>(StringComparer.OrdinalIgnoreCase);
    // 
    private string purchasingProductId;
    private bool isInitialized;
    // 
    private bool isPurchasing;

    #endregion

    private void OnDestroy()
    {
        if (androidJavaObject != null)
        {
            androidJavaObject.Call("exit");
            androidJavaObject.Dispose();
        }
    }

    private IEnumerator InitializeCoroutine(IStoreCallback callback)
    {
        // Keep a reference to the callback for communicating with Unity IAP.
        this.callback = callback;

        // isRelease
        // false : Development & Review Server
        // true : Release Server
        var packet = new Packet<C2G_ONESTORE_REQ, G2C_ONESTORE_RES>();

        yield return packet;

        if (packet.statusCode == StatusCode.OK)
        {
            var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            if (unityPlayerClass != null)
            {
                var currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
                if (currentActivity != null)
                {
                    // params
                    // (1) Listener
                    // (2) Activity Context
                    // (3) Debug (true : Debug, false : Release)
                    androidJavaObject = new AndroidJavaObject(
                        "com.onestore.iap.unity.RequestAdapter",
                        gameObject.name,
                        currentActivity,
                        !packet.response.isRelease);
                    isInitialized = (androidJavaObject != null);
                    // RetrieveProducts(ReadOnlyCollection<ProductDefinition>) 호출됨
                }
            }
        }

        if (!isInitialized)
            callback.OnSetupFailed(InitializationFailureReason.PurchasingUnavailable);
    }

    public void CommandResponse(string value)
    {
        Response commandResponse = JsonUtility.FromJson<Response>(value);
        switch (commandResponse.method)
        {
            // "request_purchase_history"
            // "check_purchasability"
            // "change_product_properties"
            case "request_product_info":
                StartCoroutine(RetrieveProductsCallbackCoroutine(commandResponse.result));
                break;
        }
    }

    public void CommandError(string value)
    {
        // 2017-06-16
        // request_product_info Command만 사용, Command Error 시 OnSetupFailed() 호출.
        callback.OnSetupFailed(InitializationFailureReason.PurchasingUnavailable);
    }

    private IEnumerator RetrieveProductsCoroutine(ReadOnlyCollection<ProductDefinition> productDefinitions)
    {
        this.productDefinitions = productDefinitions;

        while (!isInitialized)
            yield return null;

        if (androidJavaObject != null)
            // params
            // (1) Display User Interface (필요한 경우)
            // (2) Application ID
            androidJavaObject.Call("requestProductInfo", true, APPLICATION_ID);
    }

    private IEnumerator RetrieveProductsCallbackCoroutine(Result commandResult)
    {
        if (commandResult.SuccessOrFailure())
        {
            var productDescriptions = new List<ProductDescription>();
            if (commandResult.product != null && commandResult.product.Count > 0)
            {
                for (int i = 0; i < commandResult.product.Count; i++)
                {
                    var product = commandResult.product[i];
                    var productDefinition = productDefinitions.Select(item => string.Equals(item.id, product.id));
                    if (productDefinition == null)
                        // 알 수 없는 상품
                        continue;
                    var productType = product.GetProductType();
                    if (!productType.HasValue)
                        // 알 수 없는 상품
                        continue;

                    var metadata = new ProductMetadata(
                        priceString: "₩" + ((int)product.price).ToString("#,#"),
                        title: product.name,
                        description: string.Empty,
                        currencyCode: "KRW",
                        localizedPrice: (decimal)product.price);
                    var productDescription = new ProductDescription(
                        id: product.id,
                        metadata: metadata,
                        receipt: string.Empty,
                        transactionId: string.Empty,
                        type: productType.Value);

                    productDescriptions.Add(productDescription);
                    availableProducts.Add(product.id, product);

                    yield return null;
                }
            }

            // sendCommandPurchaseHistory ...
            // sendCommandCheckPurchasability ...

            yield return RetrieveUnconsumedProductsCoroutine();

            // 
            if (productDefinitions != null)
                productDefinitions = null;

            // sendCommandPurchaseHistory, sendCommandCheckPurchasability 처리 후 호출
            if (productDescriptions.Count > 0)
                callback.OnProductsRetrieved(productDescriptions);
            else
                callback.OnSetupFailed(InitializationFailureReason.NoProductsAvailable);
        }
        else
            callback.OnSetupFailed(InitializationFailureReason.PurchasingUnavailable);

        yield break;
    }

    private IEnumerator RetrieveUnconsumedProductsCoroutine()
    {
        // 
        var packet = new Packet<C2G_PURCHASE_REQ, G2C_PURCHASE_RES>();
        packet.request.isUnconsumed = true;

        yield return packet;

        if (packet.statusCode == StatusCode.OK)
        {
            // 상품 처리
        }
    }

    #region Payment

    private void PaymentAsync(IapResponse.Product product, string developerPayload)
    {
        // 유효하지 않은 상품
        if (product == null || !product.purchasability)
        {
            callback.OnPurchaseFailed(new PurchaseFailureDescription(
                productId: product.id.ToStringNullOk(),
                reason: PurchaseFailureReason.ProductUnavailable,
                message: string.Empty));

            return;
        }
        // 이미 진행 중
        else if (isPurchasing)
        {
            callback.OnPurchaseFailed(new PurchaseFailureDescription(
                productId: product.id,
                reason: PurchaseFailureReason.ExistingPurchasePending,
                message: string.Empty));

            return;
        }

        isPurchasing = true;
        purchasingProductId = product.id;
        // params
        // (1) Application ID
        // (2) Product ID
        // (3) Product Name
        // (4) Developer Payload
        // (5) Tag
        androidJavaObject.Call("requestPayment", APPLICATION_ID, product.id, product.name, developerPayload, string.Empty);
    }

    public void PaymentResponse(string value)
    {
        Response paymentResponse = JsonUtility.FromJson<Response>(value);
        if (paymentResponse.result.SuccessOrFailure())
            callback.OnPurchaseSucceeded(
                storeSpecificId: purchasingProductId,
                receipt: paymentResponse.result.receipt,
                transactionIdentifier: paymentResponse.result.txid);
        else
            // PurchaseFailureReason.Unknown, ResultCode
            callback.OnPurchaseFailed(new PurchaseFailureDescription(
                productId: purchasingProductId,
                reason: PurchaseFailureReason.Unknown,
                message: paymentResponse.result.message));

        // Finish payment, then release.
        purchasingProductId = string.Empty;
        isPurchasing = false;
    }

    public void PaymentError(string value)
    {
        Error paymentError = JsonUtility.FromJson<Error>(value);
        callback.OnPurchaseFailed(new PurchaseFailureDescription(
            productId: purchasingProductId,
            reason: PurchaseFailureReason.Unknown,
            message: paymentError.errorMessage));

        // Finish payment, then release.
        purchasingProductId = string.Empty;
        isPurchasing = false;
    }

    #endregion

    #region IStore

    public void FinishTransaction(ProductDefinition productDefinition, string transactionId)
    {
        // Perform transaction related housekeeping

        // 
    }

    public void Initialize(IStoreCallback callback)
    {
        StartCoroutine(InitializeCoroutine(callback));
    }

    public void Purchase(ProductDefinition productDefinition, string developerPayload)
    {
        // Start the purchase flow and call either callback.OnPurchaseSucceeded() or callback.OnPurchaseFailed()

        IapResponse.Product product;
        if (availableProducts.TryGetValue(productDefinition.id, out product))
            PaymentAsync(product, developerPayload);
        else
            callback.OnPurchaseFailed(new PurchaseFailureDescription(
                productId: productDefinition.id,
                reason: PurchaseFailureReason.ProductUnavailable,
                message: string.Empty));
    }

    public void RetrieveProducts(ReadOnlyCollection<ProductDefinition> productDefinitions)
    {
        // Fetch product information and invoke callback.OnProductsRetrieved();

        if (productDefinitions != null && productDefinitions.Count > 0)
            StartCoroutine(RetrieveProductsCoroutine(productDefinitions));
        else
            callback.OnSetupFailed(InitializationFailureReason.NoProductsAvailable);
    }

    #endregion
}
