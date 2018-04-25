using Common.Packet;
using System.Collections.Generic;

public partial class Guild : Node
{
    #region Variables
    CGuildBase m_GuildBase;
    #endregion

    #region Properties
    #region CGuildBase Property
    public CGuildBase guildBase
    {
        get
        {
            return m_GuildBase;
        }

        /*private*/
        set
        {
            if (m_GuildBase != value)
            {
                m_GuildBase = value;

                entry.account.gid = (m_GuildBase != null) ? m_GuildBase.m_Gid : 0;
                entry.account.guildName = (m_GuildBase != null) ? m_GuildBase.m_sGuildName : string.Empty;

                if (onGuildBaseUpdate != null)
                {
                    onGuildBaseUpdate(m_GuildBase);
                }
            }
        }
    }

    public bool isFreeJoin
    {
        get
        {
            return (m_GuildBase != null) ? m_GuildBase.m_bIsFreeJoin : false;
        }

        set
        {
            if (m_GuildBase != null && m_GuildBase.m_bIsFreeJoin != value)
            {
                REQ_PACKET_CG_GUILD_CHANGE_GUILD_JOIN_TYPE_SYN();
            }
        }
    }

    public byte guildLevel
    {
        get
        {
            if (m_GuildBase != null)
            {
                return m_GuildBase.m_byGuildLevel;
            }

            return 0;
        }

        set
        {
            if (m_GuildBase != null && m_GuildBase.m_byGuildLevel != value)
            {
                m_GuildBase.m_byGuildLevel = value;

                if (onGuildLevelUpdate != null)
                {
                    onGuildLevelUpdate(m_GuildBase.m_byGuildLevel);
                }

                if (onGuildBaseUpdate != null)
                {
                    onGuildBaseUpdate(m_GuildBase);
                }
            }
        }
    }

    public long gid
    {
        get
        {
            return (m_GuildBase != null) ? m_GuildBase.m_Gid : 0;
        }
    }

    public long guildExp
    {
        get
        {
            return (m_GuildBase != null) ? m_GuildBase.m_GuildExp : 0;
        }

        set
        {
            if (m_GuildBase != null && m_GuildBase.m_GuildExp != value)
            {
                m_GuildBase.m_GuildExp = value;

                if (onGuildBaseUpdate != null)
                {
                    onGuildBaseUpdate(m_GuildBase);
                }
            }
        }
    }

    public int memberCount
    {
        get
        {
            return (m_GuildBase != null) ? m_GuildBase.m_iMemberCount : 0;
        }

        set
        {
            if (m_GuildBase != null)
            {
                m_GuildBase.m_iMemberCount = value;
            }

            if (onGuildBaseUpdate != null)
            {
                onGuildBaseUpdate(m_GuildBase);
            }
        }
    }

    public string guildName
    {
        get
        {
            if (m_GuildBase != null)
            {
                return m_GuildBase.m_sGuildName;
            }
            else
            {
                return Languages.ToString(TEXT_UI.GUILD_NONE);
            }
        }
    }

    public string guildHeadName
    {
        get
        {
            return (m_GuildBase != null) ? m_GuildBase.m_sGuildHeadName : string.Empty;
        }
    }

    public string guildIntroduce
    {
        get
        {
            return (m_GuildBase != null) ? m_GuildBase.m_sGuildIntroduce : string.Empty;
        }

        set
        {
            if (m_GuildBase != null && m_GuildBase.m_sGuildIntroduce != value)
            {
                m_GuildBase.m_sGuildIntroduce = value;

                if (onGuildBaseUpdate != null)
                {
                    onGuildBaseUpdate(m_GuildBase);
                }
            }
        }
    }

    public int totalSupportedCardCount
    {
        get
        {
            return (m_GuildBase != null) ? m_GuildBase.m_iTotalSupportedCardCount : 0;
        }

        set
        {
            if (m_GuildBase != null && m_GuildBase.m_iTotalSupportedCardCount != value)
            {
                m_GuildBase.m_iTotalSupportedCardCount = value;

                if (onGuildBaseUpdate != null)
                {
                    onGuildBaseUpdate(m_GuildBase);
                }
            }
        }
    }

    public string guildEmblem
    {
        get
        {
            return (m_GuildBase != null) ? m_GuildBase.m_sGuildEmblem : string.Empty;
        }
    }

    public int guildRankingPoint
    {
        get
        {
            return (m_GuildBase != null) ? m_GuildBase.m_iGuildRankingPoint : 0;
        }
    }
    #endregion

    public long guildRanking
    {
        get;
        /*private*/
        set;
    }

    public long totalRankedGuildCount
    {
        get;
        /*private*/
        set;
    }

    public bool isLeader
    {
        get
        {
            if (m_GuildBase != null)
            {
                return string.Equals(entry.account.name, m_GuildBase.m_sGuildHeadName);
            }

            return false;
        }
    }

    public int MaxLevel
    {
        get
        {
            List<DB_GuildLevel.Schema> listGuildData = DB_GuildLevel.instance.schemaList;
            for (int i = 0; i < listGuildData.Count; i++)
            {
                DB_GuildLevel.Schema guildLvDt = listGuildData[i];

                if (guildLvDt.Max_Exp <= 0)
                    return guildLvDt.GulidLevel;
            }

            return 0;
        }
    }
    #endregion

    #region Delegates
    public delegate void OnGuildBaseUpdate(CGuildBase guildBase);
    public OnGuildBaseUpdate onGuildBaseUpdate;

    public delegate void OnGuildLevelUpdate(byte guildLevel);
    public OnGuildLevelUpdate onGuildLevelUpdate;

    public delegate void OnGuildJoinStateUpdate(long gid, string guildName);
    public OnGuildJoinStateUpdate onGuildJoinStateUpdate;

    // PACKET_CG_GUILD_LEAVE_GUILD_ACK, PACKET_CG_GUILD_DISBAND_GUILD_ACK
    public delegate void OnGuildDisbandResult();
    public OnGuildDisbandResult onGuildDisbandResult;
    #endregion

    public override Node OnCreate()
    {
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_RECOMMEND_LIST_ACK>(RCV_PACKET_CG_GUILD_RECOMMEND_LIST_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_CREATE_GUILD_ACK>(RCV_PACKET_CG_GUILD_CREATE_GUILD_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_JOIN_GUILD_ACK>(RCV_PACKET_CG_GUILD_JOIN_GUILD_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_ENTER_GUILD_MAIN_ACK>(RCV_PACKET_CG_GUILD_ENTER_GUILD_MAIN_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_MEMBER_LIST_ACK>(RCV_PACKET_CG_GUILD_MEMBER_LIST_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_REFRESH_CHATTING_LIST_ACK>(RCV_PACKET_CG_GUILD_REFRESH_CHATTING_LIST_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_REQUEST_CARD_SUPPORT_ACK>(RCV_PACKET_CG_GUILD_REQUEST_CARD_SUPPORT_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_SUPPORT_CARD_ACK>(RCV_PACKET_CG_GUILD_SUPPORT_CARD_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_SEND_CHAT_ACK>(RCV_PACKET_CG_GUILD_SEND_CHAT_ACK);
        //entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_DETAIL_MEMBER_INFO_ACK>(RCV_PACKET_CG_GUILD_DETAIL_MEMBER_INFO_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_DONATIONS_ACK>(RCV_PACKET_CG_GUILD_DONATIONS_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_SEARCH_GUILD_ACK>(RCV_PACKET_CG_GUILD_SEARCH_GUILD_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_CANCEL_JOIN_REQUEST_ACK>(RCV_PACKET_CG_GUILD_CANCEL_JOIN_REQUEST_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_KICK_OUT_ACK>(RCV_PACKET_CG_GUILD_KICK_OUT_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_LEAVE_GUILD_ACK>(RCV_PACKET_CG_GUILD_LEAVE_GUILD_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_CHANGE_GUILD_JOIN_TYPE_ACK>(RCV_PACKET_CG_GUILD_CHANGE_GUILD_JOIN_TYPE_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_DECISION_JOIN_REQUEST_ACK>(RCV_PACKET_CG_GUILD_DECISION_JOIN_REQUEST_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_BUY_GUILD_SHOP_ITEM_ACK>(RCV_PACKET_CG_GUILD_BUY_GUILD_SHOP_ITEM_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_REFRESH_GUILD_JOINED_ACK>(RCV_PACKET_CG_GUILD_REFRESH_GUILD_JOINED_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_GET_CARD_REQUEST_LIST_ACK>(RCV_PACKET_CG_GUILD_GET_CARD_REQUEST_LIST_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_GET_SHOP_BUY_COUNT_LIST_ACK>(RCV_PACKET_CG_GUILD_GET_SHOP_BUY_COUNT_LIST_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_DISBAND_GUILD_ACK>(RCV_PACKET_CG_GUILD_DISBAND_GUILD_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_RECEIVED_CARD_ACK>(RCV_PACKET_CG_GUILD_RECEIVED_CARD_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GUILD_UPDATE_GUILD_INTRODUCE_ACK>(RCV_PACKET_CG_GUILD_UPDATE_GUILD_INTRODUCE_ACK);

        return base.OnCreate();
    }

    void Clear()
    {
        // Initialize all variables of guild.
        entry.account.gid = 0;
        entry.account.guildName = string.Empty;
        entry.account.guildEmblem = string.Empty;
        // ref. PUC-660
        //entry.account.guildPoint = 0;
        m_ApprovalUserList = null;
        m_BuyCountDictionary.Clear();
        m_ChatSequence = 0;
        m_GuildBase = null;
        m_GuildChatList = null;
        m_GuildMemberList = null;
        m_GuildRequestCardList = null;
        m_OwnGuildMember = null;
        m_RecommendGuildDictionary.Clear();
        m_CardSupportable = false;
        m_WaitingApprovalDictionary.Clear();
        guildRanking = 0;
        totalRankedGuildCount = 0;
    }

    #region REQ
    public void REQ_PACKET_CG_GUILD_ENTER_GUILD_MAIN_SYN()
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_ENTER_GUILD_MAIN_SYN());
    }

    public void REQ_PACKET_CG_GUILD_LEAVE_GUILD_SYN()
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_LEAVE_GUILD_SYN());
    }

    public void REQ_PACKET_CG_GUILD_REFRESH_GUILD_JOINED_SYN()
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_REFRESH_GUILD_JOINED_SYN());
    }
    #endregion

    #region RCV
    void RCV_PACKET_CG_GUILD_ENTER_GUILD_MAIN_ACK(PACKET_CG_GUILD_ENTER_GUILD_MAIN_ACK packet)
    {
        // 호출 순서 정리
        m_GuildShopResetTime = packet.m_iGuildShopResetTime;
        totalRankedGuildCount = packet.m_TotalRankedGuildCount;
        guildRanking = packet.m_GuildRanking;
        Log("totalRankedGuildCount : {0} guildRanking : {1}", totalRankedGuildCount, guildRanking);
        guildBase = packet.m_GuildBase;
    }

    void RCV_PACKET_CG_GUILD_LEAVE_GUILD_ACK(PACKET_CG_GUILD_LEAVE_GUILD_ACK packet)
    {
        Clear();

        if (onGuildDisbandResult != null)
        {
            onGuildDisbandResult();
        }
    }

    void RCV_PACKET_CG_GUILD_REFRESH_GUILD_JOINED_ACK(PACKET_CG_GUILD_REFRESH_GUILD_JOINED_ACK packet)
    {
        // 가입한 길드가 없는 경우 gid = 0, guildName = string.Empty.
        if (entry.account.gid != packet.m_Gid)
        {
            entry.account.gid = packet.m_Gid;
        }

        if (entry.account.guildName != packet.m_sGuildName)
        {
            entry.account.guildName = packet.m_sGuildName;
        }

        if (entry.account.guildEmblem != packet.m_sGuildEmblem)
        {
            entry.account.guildEmblem = packet.m_sGuildEmblem;
        }


        if (onGuildJoinStateUpdate != null)
        {
            onGuildJoinStateUpdate(entry.account.gid, entry.account.guildName);
        }
    }
    #endregion
}
