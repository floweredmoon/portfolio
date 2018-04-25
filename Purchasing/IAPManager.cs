using Protocol;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : Singleton<IAPManager>, IStoreListener
{

    #region Fields

    private IStoreController storeController;
    private IExtensionProvider storeExtensionProvider;
#if UNITY_IOS
    private IAppleExtensions appleExtensions;
#endif
    private bool isInitializing;
    private bool isPurchasing;
    private Action<InitializationFailureReason?> initializeCallback;
    private Action<StatusCode?, PurchaseFailureReason?, G2C_PURCHASE_RES> purchaseCallback;

    #endregion

    #region Properties

    public bool isInitialized
    {
        get
        {
            return storeController != null && storeExtensionProvider != null;
        }
    }

    #endregion

    #region IStoreListener

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        storeExtensionProvider = extensions;
#if UNITY_IOS
        appleExtensions = storeExtensionProvider.GetExtension<IAppleExtensions>();
#endif

        Debug.Log(storeController.products.all.Length);
        foreach (var product in storeController.products.all)
            Debug.Log(product.ToString());

        isInitializing = false;
        initializeCallback.InvokeNullOk(null);
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        isInitializing = false;
        initializeCallback.InvokeNullOk(error);
    }

    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        purchaseCallback.InvokeNullOk(null, p, null);
        isPurchasing = false;
    }

    // 
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        StartCoroutine(Verification(e.purchasedProduct, (StatusCode statusCode, G2C_PURCHASE_RES response) =>
        {
            if (statusCode == StatusCode.OK)
                ConfirmPendingPurchase(e.purchasedProduct);

            purchaseCallback.InvokeNullOk(statusCode, null, response);
        }));

        return PurchaseProcessingResult.Pending;
    }

    #endregion

    public void InitializePurchasingAsync(Action<InitializationFailureReason?> initializeCallback)
    {
        if (!isInitialized)
            StartCoroutine(InitializePurchasingCoroutine(initializeCallback));
        else
            // 완료 처리
            initializeCallback.InvokeNullOk(null);
    }

    private IEnumerator InitializePurchasingCoroutine(Action<InitializationFailureReason?> initializeCallback)
    {
        if (isInitialized)
        {
            // 완료 처리
            initializeCallback.InvokeNullOk(null);
            yield break;
        }

        isInitializing = true;
        // Invoked from OnInitialized, OnInitializeFailed.
        this.initializeCallback = initializeCallback;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        UnityPurchasing.Initialize(this, builder);

        //

        while (isInitializing)
            yield return null;
    }

    public void InitiatePurchaseAsync(string productId, Action<StatusCode?, PurchaseFailureReason?, G2C_PURCHASE_RES> purchaseCallback)
    {
        StartCoroutine(InitiatePurchaseCoroutine(productId, purchaseCallback));
    }

    private IEnumerator InitiatePurchaseCoroutine(string productId, Action<StatusCode?, PurchaseFailureReason?, G2C_PURCHASE_RES> purchaseCallback)
    {
        // 
        while (isInitializing || isPurchasing)
            yield return null;

        isPurchasing = true;
        // Invoked from OnPurchaseFailed, ProcessPurchase.
        this.purchaseCallback = purchaseCallback;

        var product = storeController.products.WithStoreSpecificID(productId);
        if (product != null && product.availableToPurchase)
        {
            yield return StartCoroutine(DeveloperPayload((StatusCode statusCode, string developerPayload) =>
            {
                if (statusCode == StatusCode.OK)
                    storeController.InitiatePurchase(product, developerPayload);
                else
                    purchaseCallback.InvokeNullOk(statusCode, null, null);
            }));
        }
        else
            OnPurchaseFailed(product, PurchaseFailureReason.ProductUnavailable);

        while (isPurchasing)
            yield return null;
    }

    // Development Payload 요청
    private IEnumerator DeveloperPayload(Action<StatusCode, string> requestCallback)
    {
        var packet = new Packet<C2G_DEVELOPER_PAYLOAD_REQ, G2C_DEVELOPER_PAYLOAD_RES>();

        yield return packet;

        var developerPayload = string.Empty;
        if (packet.statusCode == StatusCode.OK)
            developerPayload = packet.response.developerPayload;

        requestCallback.InvokeNullOk(packet.statusCode.Value, developerPayload);
    }

    // 영수증 검증
    private IEnumerator Verification(Product purchasedProduct, Action<StatusCode, G2C_PURCHASE_RES> verificationCallback)
    {
        // 
        if (purchasedProduct == null || !purchasedProduct.availableToPurchase)
        {
            verificationCallback.InvokeNullOk(StatusCode.ProductUnavailable, null);
            yield break;
        }

        // 
        if (!purchasedProduct.hasReceipt)
        {
            verificationCallback.InvokeNullOk(StatusCode.SignatureInvalid, null);
            yield break;
        }

        var packet = new Packet<C2G_PURCHASE_REQ, G2C_PURCHASE_RES>();
        packet.request.purchasedProduct = purchasedProduct;

        yield return packet;

        verificationCallback.InvokeNullOk(packet.statusCode.Value, packet.response);
    }

    // 완료
    private void ConfirmPendingPurchase(Product purchasedProduct)
    {
        if (isInitialized)
            storeController.ConfirmPendingPurchase(purchasedProduct);
    }
}
