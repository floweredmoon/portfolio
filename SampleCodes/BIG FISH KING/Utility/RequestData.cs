using System;

public sealed class RequestData<TKey, TValue>
{

    public RequestData(int? expirationMinutes = null)
    {
        cachedTime = DateTime.MinValue;
        this.expirationMinutes = expirationMinutes;
    }

    public TValue value
    {
        get;
        /*private*/
        set;
    }

    public DateTime cachedTime
    {
        get;
        set;
    }

    private int? expirationMinutes
    {
        get;
        set;
    }

    public bool isExpired
    {
        get
        {
            return expirationMinutes.HasValue ? (DateTime.Now - cachedTime).TotalMinutes > expirationMinutes : false;
        }
    }

    public bool isInvalid
    {
        get;
        set;
    }
}
