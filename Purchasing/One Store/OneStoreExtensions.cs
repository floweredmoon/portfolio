using IapResponse;
using UnityEngine.Purchasing;

public static class OneStoreExtensions
{

    public static bool SuccessOrFailure(this Result commandResult)
    {
        return commandResult != null && string.Equals(commandResult.code, "0000");
    }

    public static ProductType? GetProductType(this IapResponse.Product product)
    {
        if (product != null)
        {
            switch (product.kind)
            {
                // "one-day-pass"
                // "one-week-pass"
                // "one-month-pass"
                case "non-consumable":
                    return ProductType.NonConsumable;
                case "consumable":
                    return ProductType.Consumable;
            }
        }

        return null;
    }
}
