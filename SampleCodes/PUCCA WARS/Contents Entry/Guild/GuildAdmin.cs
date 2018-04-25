using Common.Packet;
using System.Collections.Generic;

public partial class Guild
{
    #region Delegates
    public delegate void OnGuildCreateResult();
    public OnGuildCreateResult onGuildCreateResult;

    public delegate void OnJoinTypeChange(bool isFreeJoin);
    public OnJoinTypeChange onJoinTypeChange;
    #endregion

    #region REQ
    public void REQ_PACKET_CG_GUILD_CREATE_GUILD_SYN(string guildName, string guildIntroduce, string guildEmblem, bool isFreeJoin)
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_CREATE_GUILD_SYN()
        {
            m_sGuildName = guildName,
            m_sGuildIntroduce = guildIntroduce,
            m_sGuildEmblem = guildEmblem,
            m_bIsFreeJoin = isFreeJoin,
        });
    }

    public void REQ_PACKET_CG_GUILD_CHANGE_GUILD_JOIN_TYPE_SYN()
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_CHANGE_GUILD_JOIN_TYPE_SYN());
    }

    public void REQ_PACKET_CG_GUILD_DECISION_JOIN_REQUEST_SYN(long aid, bool approval)
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_DECISION_JOIN_REQUEST_SYN()
        {
            m_TargetAid = aid,
            m_bIsApproval = approval,
        });
    }

    public void REQ_PACKET_CG_GUILD_DISBAND_GUILD_SYN()
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_DISBAND_GUILD_SYN());
    }

    public void REQ_PACKET_CG_GUILD_UPDATE_GUILD_INTRODUCE_SYN(string guildIntroduce)
    {
        if (string.IsNullOrEmpty(guildIntroduce))
        {
            return;
        }

        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_UPDATE_GUILD_INTRODUCE_SYN()
            {
                m_sIntroduce = guildIntroduce,
            });
    }
    #endregion

    #region RCV
    void RCV_PACKET_CG_GUILD_CREATE_GUILD_ACK(PACKET_CG_GUILD_CREATE_GUILD_ACK packet)
    {
        entry.account.gold = packet.m_iRemainGold;

        if (onGuildCreateResult != null)
        {
            onGuildCreateResult();
        }
    }

    void RCV_PACKET_CG_GUILD_CHANGE_GUILD_JOIN_TYPE_ACK(PACKET_CG_GUILD_CHANGE_GUILD_JOIN_TYPE_ACK packet)
    {
        if (guildBase != null)
        {
            guildBase.m_bIsFreeJoin = !guildBase.m_bIsFreeJoin;

            // 가입 조건이 자유로 변경될 경우, 가입 대기 유저 목록을 비웁니다.
            if (guildBase.m_bIsFreeJoin)
            {
                if (m_ApprovalUserList != null && m_ApprovalUserList.Count > 0)
                {
                    approvalUserList = new List<CGuildUserBase>();
                }
            }

            if (onJoinTypeChange != null)
            {
                onJoinTypeChange(guildBase.m_bIsFreeJoin);
            }
        }
    }

    void RCV_PACKET_CG_GUILD_DECISION_JOIN_REQUEST_ACK(PACKET_CG_GUILD_DECISION_JOIN_REQUEST_ACK packet)
    {
        guildMemberList = packet.m_GuildMemberList;
        approvalUserList = packet.m_ApprovalUserList;
    }

    void RCV_PACKET_CG_GUILD_DISBAND_GUILD_ACK(PACKET_CG_GUILD_DISBAND_GUILD_ACK packet)
    {
        Clear();

        if (onGuildDisbandResult != null)
        {
            onGuildDisbandResult();
        }
    }

    void RCV_PACKET_CG_GUILD_UPDATE_GUILD_INTRODUCE_ACK(PACKET_CG_GUILD_UPDATE_GUILD_INTRODUCE_ACK packet)
    {
        guildIntroduce = packet.m_sIntroduce;
    }
    #endregion
}
