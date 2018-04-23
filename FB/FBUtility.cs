using System.Text;

public static class FBUtility
{

    private static StringBuilder s_QueryBuilder = new StringBuilder();

    public static string QueryBuilder(string userId, params object[] fieldNames)
    {
        s_QueryBuilder.Remove(0, s_QueryBuilder.Length);
        s_QueryBuilder.Append(userId);
        if (fieldNames != null && fieldNames.Length > 0)
        {
            s_QueryBuilder.Append("?fields=");
            for (var i = 0; i < fieldNames.Length; i++)
                s_QueryBuilder.AppendFormat("{0},", fieldNames[i]);
            s_QueryBuilder.Remove(s_QueryBuilder.Length - 1, 1); // Remove the last comma.
        }

        return s_QueryBuilder.ToString();
    }

    public static string QueryBuilder(object edgeName, int? limit = null, params object[] fieldNames)
    {
        s_QueryBuilder.Remove(0, s_QueryBuilder.Length);
        s_QueryBuilder.Append(edgeName);
        if (limit.HasValue)
            s_QueryBuilder.AppendFormat(".limit({0})", limit);
        if (fieldNames != null && fieldNames.Length > 0)
        {
            s_QueryBuilder.Append("{");
            for (var i = 0; i < fieldNames.Length; i++)
                s_QueryBuilder.AppendFormat("{0},", fieldNames[i]);
            s_QueryBuilder.Remove(s_QueryBuilder.Length - 1, 1); // Remove the last comma.
            s_QueryBuilder.Append("}");
        }

        return s_QueryBuilder.ToString();
    }
}
