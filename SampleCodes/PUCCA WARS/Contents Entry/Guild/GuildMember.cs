using Common.Packet;
using System.Collections.Generic;

public partial class Guild
{
    #region Variables
    List<CGuildMember> m_GuildMemberList;
    List<CGuildUserBase> m_ApprovalUserList;
    CGuildMember m_OwnGuildMember;
    #endregion

    #region Properties
    public List<CGuildMember> guildMemberList
    {
        get
        {
            return m_GuildMemberList;
        }

        private set
        {
            m_GuildMemberList = value;
            memberCount = (m_GuildMemberList != null) ? m_GuildMemberList.Count : 0; // 임시

            if (onGuildMemberListUpdate != null)
            {
                onGuildMemberListUpdate(m_GuildMemberList);
            }
        }
    }

    public List<CGuildUserBase> approvalUserList
    {
        get
        {
            return m_ApprovalUserList;
        }

        private set
        {
            m_ApprovalUserList = value;

            if (onApprovalUserListUpdate != null)
            {
                onApprovalUserListUpdate(m_ApprovalUserList);
            }
        }
    }

    CGuildMember ownGuildMember
    {
        get
        {
            if (Equals(m_OwnGuildMember, null))
            {
                m_OwnGuildMember = FindGuildMember(entry.account.userNo);
            }

            return m_OwnGuildMember;
        }
    }
    #endregion

    #region Delegates
    public delegate void OnGuildMemberListUpdate(List<CGuildMember> guildMemberList);
    public OnGuildMemberListUpdate onGuildMemberListUpdate;

    public delegate void OnApprovalUserListUpdate(List<CGuildUserBase> approvalUserList);
    public OnApprovalUserListUpdate onApprovalUserListUpdate;
    /*
    public delegate void OnGuildMemberInfoResult(long aid,
                                                 CGuildBase guildBase,
                                                 CGuildMember guildMember,
                                                 CFranchiseRankingInfo franchiseRankingInfo,
                                                 List<CCardInfo> cardInfoList);
    public OnGuildMemberInfoResult onGuildMemberInfoResult;
    */
    public delegate void OnKickResult(long aid);
    public OnKickResult onKickResult;
    #endregion

    bool RemoveApprovalUser(long aid)
    {
        if (m_ApprovalUserList != null && m_ApprovalUserList.Count > 0)
        {
            return (m_ApprovalUserList.RemoveAll(item => item.m_AID.Equals(aid)) > 0);
        }

        return false;
    }

    public CGuildUserBase FindApprovalUser(long aid)
    {
        if (m_ApprovalUserList != null && m_ApprovalUserList.Count > 0)
        {
            return m_ApprovalUserList.Find(item => item.m_AID.Equals(aid));
        }

        return null;
    }

    public CGuildMember FindGuildMember(long aid)
    {
        if (m_GuildMemberList != null && m_GuildMemberList.Count > 0)
        {
            return m_GuildMemberList.Find(item => long.Equals(item.m_AID, aid));
        }

        return null;
    }

    bool RemoveGuildMember(long aid)
    {
        if (m_GuildMemberList != null && m_GuildMemberList.Count > 0)
        {
            return (m_GuildMemberList.RemoveAll(item => long.Equals(item.m_AID, aid)) > 0);
        }

        return false;
    }

    #region REQ
    public void REQ_PACKET_CG_GUILD_MEMBER_LIST_SYN()
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_MEMBER_LIST_SYN());
    }
    /*
    public void REQ_PACKET_CG_GUILD_DETAIL_MEMBER_INFO_SYN(long aid)
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_DETAIL_MEMBER_INFO_SYN()
        {
            m_MemberAID = aid,
        });
    }
    */
    public void REQ_PACKET_CG_GUILD_KICK_OUT_SYN(long aid)
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_KICK_OUT_SYN()
        {
            m_KickOutAid = aid,
        });
    }
    #endregion

    #region RCV
    void RCV_PACKET_CG_GUILD_MEMBER_LIST_ACK(PACKET_CG_GUILD_MEMBER_LIST_ACK packet)
    {
        guildMemberList = packet.m_GuildMemberList;
        approvalUserList = packet.m_ApprovalUserList;
    }
    /*
    void RCV_PACKET_CG_GUILD_DETAIL_MEMBER_INFO_ACK(PACKET_CG_GUILD_DETAIL_MEMBER_INFO_ACK packet)
    {
        long aid = packet.m_MemberAid;
        CGuildMember guildMember = FindGuildMember(aid);
        if (guildMember != null)
        {
            if (onGuildMemberInfoResult != null)
            {
                onGuildMemberInfoResult(aid, guildBase, guildMember, packet.m_MemberRankInfo, packet.m_MainDeckCardInfoList);
            }
        }
    }
    */
    void RCV_PACKET_CG_GUILD_KICK_OUT_ACK(PACKET_CG_GUILD_KICK_OUT_ACK packet)
    {
        if (RemoveGuildMember(packet.m_KickOutAid))
        {
            memberCount = (m_GuildMemberList != null) ? m_GuildMemberList.Count : 0; // 임시

            if (onKickResult != null)
            {
                onKickResult(packet.m_KickOutAid);
            }
        }
        else
        {
            LogError("m_KickOutAid : {0}", packet.m_KickOutAid);
        }
    }
    #endregion
}
