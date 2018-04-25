using Common.Packet;
using System.Collections.Generic;
using System.Linq;

public partial class Guild
{
    #region Variables
    Dictionary<long, CGuildBase> m_RecommendGuildDictionary = new Dictionary<long, CGuildBase>();
    Dictionary<long, CGuildBase> m_WaitingApprovalDictionary = new Dictionary<long, CGuildBase>();
    #endregion

    #region Properties
    public int recommendGuildCount
    {
        get
        {
            return (m_RecommendGuildDictionary != null) ? m_RecommendGuildDictionary.Count : 0;
        }
    }

    public List<CGuildBase> recommendGuildList
    {
        get
        {
            return (m_RecommendGuildDictionary != null) ? m_RecommendGuildDictionary.Values.ToList<CGuildBase>() : null;
        }
    }

    public int waitingApprovalCount
    {
        get
        {
            return (m_WaitingApprovalDictionary != null) ? m_WaitingApprovalDictionary.Count : 0;
        }
    }

    public List<CGuildBase> waitingApprovalList
    {
        get
        {
            return (m_WaitingApprovalDictionary != null) ? m_WaitingApprovalDictionary.Values.ToList<CGuildBase>() : null;
        }
    }
    #endregion

    #region Delegates
    public delegate void OnGuildListUpdate(List<CGuildBase> recommendGuildList, List<CGuildBase> waitingApprovalList);
    public OnGuildListUpdate onGuildListUpdate;

    public delegate void OnSearchResult(CGuildBase guildBase);
    public OnSearchResult onSearchResult;

    public delegate void OnJoinResult(long gid, string guildName, bool isJoin);
    public OnJoinResult onJoinResult;

    public delegate void OnJoinRequestCancelResult(long gid);
    public OnJoinRequestCancelResult onJoinRequestCancelResult;
    #endregion

    void ClearWaitingApprovalGuildList()
    {
        m_WaitingApprovalDictionary.Clear();
    }

    CGuildBase FindWaitingApprovalGuild(long gid)
    {
        return m_WaitingApprovalDictionary.ContainsKey(gid) ? m_WaitingApprovalDictionary[gid] : null;
    }

    bool RemoveWaitingApprovalGuild(long gid)
    {
        return m_WaitingApprovalDictionary.Remove(gid);
    }

    void AddWaitingApprovalGuild(CGuildBase guildBase)
    {
        if (guildBase != null)
        {
            if (!m_WaitingApprovalDictionary.ContainsKey(guildBase.m_Gid))
            {
                m_WaitingApprovalDictionary.Add(guildBase.m_Gid, guildBase);
            }
            else LogError("Duplicated CGuildBase.m_Gid({0}) in waitingApprovalList.", guildBase.m_Gid);
        }
    }

    void ClearRecommendGuildList()
    {
        m_RecommendGuildDictionary.Clear();
    }

    public CGuildBase FindRecommendGuild(long gid)
    {
        return m_RecommendGuildDictionary.ContainsKey(gid) ? m_RecommendGuildDictionary[gid] : null;
    }

    bool RemoveRecommendGuild(long gid)
    {
        return m_RecommendGuildDictionary.Remove(gid);
    }

    void AddRecommendGuild(CGuildBase guildBase)
    {
        if (guildBase != null)
        {
            if (!m_RecommendGuildDictionary.ContainsKey(guildBase.m_Gid))
            {
                m_RecommendGuildDictionary.Add(guildBase.m_Gid, guildBase);
            }
            else LogError("Duplicated CGuildBase.m_Gid({0}) in recommendGuildList.", guildBase.m_Gid);
        }
    }

    #region REQ
    public void REQ_PACKET_CG_GUILD_RECOMMEND_LIST_SYN()
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_RECOMMEND_LIST_SYN());
    }

    public void REQ_PACKET_CG_GUILD_JOIN_GUILD_SYN(long gid)
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_JOIN_GUILD_SYN()
        {
            m_Gid = gid,
        });
    }

    public void REQ_PACKET_CG_GUILD_SEARCH_GUILD_SYN(string guildName)
    {
        if (string.IsNullOrEmpty(guildName))
        {
            return;
        }

        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_SEARCH_GUILD_SYN()
        {
            m_sGuildName = guildName,
        });
    }

    public void REQ_PACKET_CG_GUILD_CANCEL_JOIN_REQUEST_SYN(long gid)
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_CANCEL_JOIN_REQUEST_SYN()
        {
            m_Gid = gid,
        });
    }
    #endregion

    #region RCV
    void RCV_PACKET_CG_GUILD_RECOMMEND_LIST_ACK(PACKET_CG_GUILD_RECOMMEND_LIST_ACK packet)
    {
        m_RecommendGuildDictionary.Clear();
        if (packet.m_RecommendGuildList != null && packet.m_RecommendGuildList.Count > 0)
        {
            for (int i = 0; i < packet.m_RecommendGuildList.Count; i++)
            {
                AddRecommendGuild(packet.m_RecommendGuildList[i]);
            }
        }

        m_WaitingApprovalDictionary.Clear();
        if (packet.m_WaitingApprovalList != null && packet.m_WaitingApprovalList.Count > 0)
        {
            for (int i = 0; i < packet.m_WaitingApprovalList.Count; i++)
            {
                AddWaitingApprovalGuild(packet.m_WaitingApprovalList[i]);
            }
        }

        if (onGuildListUpdate != null)
        {
            onGuildListUpdate(recommendGuildList, waitingApprovalList);
        }
    }

    void RCV_PACKET_CG_GUILD_JOIN_GUILD_ACK(PACKET_CG_GUILD_JOIN_GUILD_ACK packet)
    {
        string guildName = string.Empty;
        CGuildBase guildBase = FindRecommendGuild(packet.m_GuildInfo.m_Gid);
        if (guildBase != null)
        {
            guildName = guildBase.m_sGuildName;
        }
        else
        {
            LogError("{0}", packet.m_GuildInfo.m_Gid);
        }

        if (packet.m_bJoined)
        {
            ClearRecommendGuildList();
            ClearWaitingApprovalGuildList();
        }
        else
        {
            RemoveRecommendGuild(packet.m_GuildInfo.m_Gid);
            AddWaitingApprovalGuild(packet.m_GuildInfo);
        }

        if (onJoinResult != null)
        {
            onJoinResult(packet.m_GuildInfo.m_Gid, guildName, packet.m_bJoined);
        }
    }

    void RCV_PACKET_CG_GUILD_SEARCH_GUILD_ACK(PACKET_CG_GUILD_SEARCH_GUILD_ACK packet)
    {
        AddRecommendGuild(packet.m_GuildInfo);

        if (onSearchResult != null)
        {
            onSearchResult(packet.m_GuildInfo);
        }
    }

    void RCV_PACKET_CG_GUILD_CANCEL_JOIN_REQUEST_ACK(PACKET_CG_GUILD_CANCEL_JOIN_REQUEST_ACK packet)
    {
        if (RemoveWaitingApprovalGuild(packet.m_Gid))
        {
            if (onJoinRequestCancelResult != null)
            {
                onJoinRequestCancelResult(packet.m_Gid);
            }
        }
        else
        {
            LogError("Failed to remove CGuildBase(m_Gid : {0}) in waitingApprovalList.", packet.m_Gid);
        }
    }
    #endregion
}
