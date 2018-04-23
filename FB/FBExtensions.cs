using Facebook.Unity;

public static class FBExtensions
{

    public static bool SuccessOrFailure(this IResult result)
    {
        if (result == null)
        {
            return false;
        }
        else if (!string.IsNullOrEmpty(result.Error))
        {
            // Error Response
            return false;
        }
        else if (result.Cancelled)
        {
            // Cancelled Response
            return false;
        }
        else if (!string.IsNullOrEmpty(result.RawResult))
        {
            // Success Response
            return true;
        }
        else
        {
            // Empty Response
            return false;
        }
    }
}
