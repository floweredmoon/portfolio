using System;
using System.Collections.Generic;

public abstract class FBPagination<TPagination, TFBNode> : IFBEdge
    where TPagination : FBPagination<TPagination, TFBNode>
    where TFBNode : IFBNode
{

    public List<TFBNode> data;
    public FBPaging paging;

    public int count
    {
        get;
        set;
    }

    public void RequestNextAsync(Action<bool> callback)
    {
        if (paging != null && paging.hasNext)
            paging.RequestNextAsync<TPagination, TFBNode>((bool successOrFailure, TPagination pagination) =>
            {
                if (successOrFailure)
                    Merge(pagination);

                callback.InvokeNullOk(successOrFailure);
            });
        else
            callback.InvokeNullOk(false);
    }

    protected void Merge(TPagination pagination)
    {
        if (pagination != null)
        {
            if (pagination.data != null && pagination.data.Count > 0)
                data.AddRange(pagination.data);

            if (pagination.paging != null)
                paging = pagination.paging;
        }
    }
}
