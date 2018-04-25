using System;

public abstract class Node : IDisposable
{
    bool m_Disposed = false;

    public Entry entry
    {
        get;
        set;
    }

    public bool isDirty
    {
        get;
        private set;
    }

    ~Node()
    {
        Dispose(false);
    }

    public virtual Node OnCreate()
    {
        return this;
    }

    //public virtual void OnDestroy() { }

    protected virtual void SetDirty(bool value)
    {
        if (isDirty != value)
        {
            isDirty = value;

            if (onDirtyChanged != null)
            {
                onDirtyChanged(isDirty);
            }
        }
    }

    public virtual void Update() { }

    public delegate void OnDirtyChanged(bool isDirty);
    public OnDirtyChanged onDirtyChanged;

    #region IDisposable

    void Dispose(bool dispose)
    {
        if (m_Disposed)
        {
            return;
        }

        if (dispose)
        {
            entry = null;
        }

        m_Disposed = true;
    }

    public void Dispose()
    {
        if (m_Disposed)
        {
            return;
        }

        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region ILogger

    protected void Log(string format, params object[] args)
    {
        if (entry != null && entry.logger != null)
        {
            entry.logger.Log(format, args);
        }
    }

    protected void LogWarning(string format, params object[] args)
    {
        if (entry != null && entry.logger != null)
        {
            entry.logger.LogWarning(format, args);
        }
    }

    protected void LogError(string format, params object[] args)
    {
        if (entry != null && entry.logger != null)
        {
            entry.logger.LogError(format, args);
        }
    }

    #endregion
}
