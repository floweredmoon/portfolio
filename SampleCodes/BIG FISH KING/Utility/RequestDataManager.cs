using Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RequestDataManager<TKey, TValue, TRequest, TResponse> : Singleton<RequestDataManager<TKey, TValue, TRequest, TResponse>>
    where TRequest : class, IRequestProtocol, new()
    where TResponse : class, IResponseProtocol
{

    private Dictionary<TKey, RequestData<TKey, TValue>> m_RequestDataDict = new Dictionary<TKey, RequestData<TKey, TValue>>();

    public int? expirationMinutes
    {
        get;
        set;
    }

    protected abstract void OnPreprocess(TRequest request, TKey key);

    //protected abstract void OnPostprocess(TResponse response);

    //protected virtual void OnPreprocess(TValue value) { }

    protected virtual void OnPostprocess(TValue value) { }

    protected abstract bool TryGetValue(TResponse response, out TValue value);

    public void RequestAsync(TKey key, Action<TKey, RequestData<TKey, TValue>> callback)
    {
        StartCoroutine(RequestCoroutine(key, callback));
    }

    private IEnumerator RequestCoroutine(TKey key, Action<TKey, RequestData<TKey, TValue>> callback)
    {
        RequestData<TKey, TValue> requestData;
        if (!m_RequestDataDict.TryGetValue(key, out requestData))
            Add(key, requestData = new RequestData<TKey, TValue>(expirationMinutes));

        // isExpired : 만료
        // isInvalid : 실패
        if (requestData.isExpired || requestData.isInvalid)
        {
            var packet = new Packet<TRequest, TResponse>();
            OnPreprocess(packet.request, key);

            yield return packet;

            if (!(requestData.isInvalid = packet.statusCode != StatusCode.OK))
            {
                TValue value;
                if (TryGetValue(packet.response, out value))
                {
                    OnPostprocess(value);
                    requestData.value = value;
                    requestData.cachedTime = DateTime.Now;
                }
            }
            else
                requestData.cachedTime = DateTime.MinValue;
        }

        callback.InvokeNullOk(key, requestData);
    }

    public void Add(TKey key, RequestData<TKey, TValue> value)
    {
        if (m_RequestDataDict.ContainsKey(key))
            Debug.LogWarningFormat("{0}, {1}", key, value);

        m_RequestDataDict[key] = value;
    }

    public bool Remove(TKey key)
    {
        return m_RequestDataDict.Remove(key);
    }

    public void Clear()
    {
        m_RequestDataDict.Clear();
    }
}
