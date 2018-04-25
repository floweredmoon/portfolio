using Common.Packet;
using System.Collections.Generic;

public partial class Guild
{
    #region Variables
    List<CGuildChatting> m_GuildChatList = new List<CGuildChatting>();
    long m_ChatSequence;
    #endregion

    #region Properties
    public List<CGuildChatting> guildChatList
    {
        get
        {
            return m_GuildChatList;
        }
    }

    public long chatSequence
    {
        get
        {
            return m_ChatSequence;
        }

        private set
        {
            if (m_ChatSequence != value)
            {
                m_ChatSequence = value;
            }
        }
    }

    public bool isNewChat
    {
        get;
        set;
    }
    #endregion

    #region Delegates
    public delegate void OnGuildChatListUpdate(List<CGuildChatting> guildChatList, int startIndex, int length, bool focusing);
    public OnGuildChatListUpdate onGuildChatListUpdate;
    #endregion

    public void UpdateGuildChatList(List<CGuildChatting> guildChatList, bool focusing, bool isNewChat)
    {
        // 길드 탈퇴 시 null 할당.
        if (m_GuildChatList == null)
        {
            m_GuildChatList = new List<CGuildChatting>();
        }

        int startIndex = (m_GuildChatList != null && m_GuildChatList.Count > 0) ? m_GuildChatList.Count : 0;
        int length = 0;
        if (guildChatList != null && guildChatList.Count > 0)
        {
            guildChatList.Sort(delegate(CGuildChatting lhs, CGuildChatting rhs)
                {
                    if (lhs != null && rhs != null)
                    {
                        return lhs.m_Sequence.CompareTo(rhs.m_Sequence);
                    }
                    else
                    {
                        return -1;
                    }
                });

            length = guildChatList.Count;
            m_GuildChatList.AddRange(guildChatList);
            chatSequence = m_GuildChatList[m_GuildChatList.Count - 1].m_Sequence;
        }

        this.isNewChat = isNewChat ? (guildChatList != null && guildChatList.Count > 0) : isNewChat;

        if (onGuildChatListUpdate != null)
        {
            onGuildChatListUpdate(m_GuildChatList, startIndex, length, focusing ? (length > 0) : focusing);
        }
    }

    public PACKET_CG_GUILD_REFRESH_CHATTING_LIST_SYN PACKET_CG_GUILD_REFRESH_CHATTING_LIST_SYN()
    {
        return new PACKET_CG_GUILD_REFRESH_CHATTING_LIST_SYN()
        {
            m_ChattingSequence = m_ChatSequence,
        };
    }

    #region REQ
    public void REQ_PACKET_CG_GUILD_REFRESH_CHATTING_LIST_SYN()
    {
        Kernel.networkManager.WebRequest(PACKET_CG_GUILD_REFRESH_CHATTING_LIST_SYN(), false);
    }

    public void REQ_PACKET_CG_GUILD_SEND_CHAT_SYN(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_SEND_CHAT_SYN()
        {
            m_ChattingSequence = m_ChatSequence,
            m_sChatString = value,
        });
    }
    #endregion

    #region RCV
    void RCV_PACKET_CG_GUILD_SEND_CHAT_ACK(PACKET_CG_GUILD_SEND_CHAT_ACK packet)
    {
        UpdateGuildChatList(packet.m_GuildChattingList, true, false);
    }

    void RCV_PACKET_CG_GUILD_REFRESH_CHATTING_LIST_ACK(PACKET_CG_GUILD_REFRESH_CHATTING_LIST_ACK packet)
    {
        UpdateGuildChatList(packet.m_GuildChattingList, false, true);
    }
    #endregion
}
