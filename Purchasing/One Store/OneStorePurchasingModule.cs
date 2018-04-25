using UnityEngine.Purchasing.Extension;

public class OneStorePurchasingModule : AbstractPurchasingModule, IStoreConfiguration
{

    #region AbstractPurchasingModule

    public override void Configure()
    {
        RegisterStore(typeof(OneStore).ToString(), OneStore.instance);
    }

    #endregion
}
