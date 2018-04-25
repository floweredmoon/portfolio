using System;

public sealed class FBPaging
{

    [Serializable]
    public sealed class edge_cursors : IFBEdge
    {

        public string before;
        public string after;
    }

    public string next;
    public string previous;
    public edge_cursors cursors;

    public bool hasNext
    {
        get
        {
            return !string.IsNullOrEmpty(next);
        }
    }

    public void RequestNextAsync<TPagination, TFBNode>(Action<bool, TPagination> callback)
        where TPagination : FBPagination<TPagination, TFBNode>
        where TFBNode : IFBNode
    {
        if (hasNext)
            FBPlatform.instance.RequestGraphApiAsync(
                query: next,
                method: Facebook.Unity.HttpMethod.GET,
                callback: (bool successOrFailure, TPagination pagination) =>
                {
                    callback.InvokeNullOk(successOrFailure, pagination);
                });
        else
            callback.InvokeNullOk(false, null);
    }
}
