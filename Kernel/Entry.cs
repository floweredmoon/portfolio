using System;

public sealed class Entry
{
    #region Properties

    public ILogger logger
    {
        get;
        private set;
    }

    public PacketBroadcaster packetBroadcaster
    {
        get;
        private set;
    }

    public Data data
    {
        get;
        private set;
    }

    public Account account
    {
        get;
        private set;
    }

    public Character character
    {
        get;
        private set;
    }

    public Chest chest
    {
        get;
        private set;
    }

    public Battle battle
    {
        get;
        private set;
    }

    public Administrator administrator
    {
        get;
        private set;
    }

    public Guild guild
    {
        get;
        private set;
    }

    public Adventure adventure
    {
        get;
        private set;
    }

    public Treasure treasure
    {
        get;
        private set;
    }

    public Ranking ranking
    {
        get;
        private set;
    }

    public Post post
    {

        get;
        private set;
    }

    public Achieve achieve
    {
        get;
        private set;
    }

    public NormalShop normalShop
    {
        get;
        private set;
    }

    public StrangeShop strangeShop
    {
        get;
        private set;
    }

    public SecretBusiness secretBusiness
    {
        get;
        private set;
    }

    public RevengeBattle revengeBattle
    {
        get;
        private set;
    }

    public Franchise franchise
    {
        get;
        private set;
    }


    public Detect detect
    {
        get;
        private set;
    }

    public Tutorial tutorial
    {
        get;
        private set;
    }

    public Billing billing
    {
        get;
        private set;
    }

    public Notice notice
    {
        get;
        private set;
    }

    public EventTime eventTime
    {
        get;
        private set;
    }

    #endregion

    public Entry(ILogger logger)
    {
        if (logger != null)
        {
            this.logger = logger;
        }

        packetBroadcaster   = CreateNode<PacketBroadcaster>();
        data                = CreateNode<Data>();
        account             = CreateNode<Account>();
        character           = CreateNode<Character>();
        chest               = CreateNode<Chest>();
        battle              = CreateNode<Battle>();
        administrator       = CreateNode<Administrator>();
        guild               = CreateNode<Guild>();
        adventure           = CreateNode<Adventure>();
        treasure            = CreateNode<Treasure>();
        ranking             = CreateNode<Ranking>();
        post                = CreateNode<Post>();
        achieve             = CreateNode<Achieve>();
        normalShop          = CreateNode<NormalShop>();
        strangeShop         = CreateNode<StrangeShop>();
        secretBusiness      = CreateNode<SecretBusiness>();
        revengeBattle       = CreateNode<RevengeBattle>();
        franchise           = CreateNode<Franchise>();
        detect              = CreateNode<Detect>();
        tutorial            = CreateNode<Tutorial>();
        billing             = CreateNode<Billing>();
        notice              = CreateNode<Notice>();
        eventTime           = CreateNode<EventTime>();
    }

    public T CreateNode<T>() where T : Node, new()
    {
        return new T()
        {
            entry = this,
        }.OnCreate() as T;
    }

    #region ILogger

    void Log(string format, params object[] args)
    {
        if (logger != null)
        {
            logger.Log(format, args);
        }
    }

    void LogWarning(string format, params object[] args)
    {
        if (logger != null)
        {
            logger.LogWarning(format, args);
        }
    }

    void LogError(string format, params object[] args)
    {
        if (logger != null)
        {
            logger.LogError(format, args);
        }
    }

    #endregion
}
