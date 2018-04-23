using System;
using System.Collections.Generic;

public interface IFBNodeCollection<TFBNode> : IFBEdge
    where TFBNode : IFBNode
{

    List<TFBNode> data { get; }
    bool hasNext { get; }
    int count { get; }

    void RequestNextAsync(Action<bool> callback);
}
