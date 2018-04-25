using Common.Packet;
using System;
using System.Collections.Generic;

public partial class Guild
{
    #region Variables
    List<CGuildRequestCard> m_GuildRequestCardList;
    List<CGuildSupportCard> m_GuildSupportCardList;
    /// <summary>
    /// Deprecated variable.
    /// </summary>
    bool m_CardSupportable;
    int m_LastCardRequestedTime;
    int m_GuildCardRequestCycleSec;
    #endregion

    #region Properties
    public List<CGuildRequestCard> guildRequestCardList
    {
        get
        {
            return m_GuildRequestCardList;
        }

        set
        {
            m_GuildRequestCardList = value;

            #region Deprecated
            /*
            m_CardSupportable = false;
            if (m_GuildRequestCardList != null && m_GuildRequestCardList.Count > 0)
            {
                for (int i = 0; i < m_GuildRequestCardList.Count; i++)
                {
                    CGuildRequestCard item = m_GuildRequestCardList[i];
                    if (item.m_bIsReceiveComplete)
                    {
                        continue;
                    }

                    // Using IsSupportableRequest().
                    CGuildSupportCard guildSupportCard = FindGuildSupportCard(item.m_Sequence);
                    if (guildSupportCard != null)
                    {
                        if (guildSupportCard.m_iSendCardCount >= 3)
                        {
                            continue;
                        }
                    }

                    CSoulInfo soulInfo = Kernel.entry.character.FindSoulInfo(item.m_iRequestCardIndex);
                    if (soulInfo != null && soulInfo.m_iSoulCount > 0)
                    {
                        m_CardSupportable = true;

                        break;
                    }
                }
            }
            */
            #endregion

            if (onGuildRequestCardListUpdate != null)
            {
                onGuildRequestCardListUpdate(m_GuildRequestCardList);
            }
        }
    }

    public List<CGuildSupportCard> guildSupportCardList
    {
        get
        {
            return m_GuildSupportCardList;
        }
        /*
        private set
        {
            m_GuildSupportCardList = value;
        }
        */
    }

    /// <summary>
    /// Deprecated property.
    /// </summary>
    public bool cardSupportable
    {
        get
        {
            return m_CardSupportable;
        }
    }

    public bool cardRequestable
    {
        get
        {
            if (m_LastCardRequestedTime > 0)
            {
                TimeSpan ts = TimeUtility.currentServerTime - TimeUtility.ToDateTime(m_LastCardRequestedTime);
                if (ts.TotalSeconds < guildCardRequestCycleSec)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public int lastCardRequestedTime
    {
        get
        {
            return m_LastCardRequestedTime;

            /*
            if (m_GuildRequestCardList != null && m_GuildRequestCardList.Count > 0)
            {
                CGuildRequestCard guildRequestCard = m_GuildRequestCardList.Find(item => item.m_RequesterAid.Equals(entry.account.userNo));
                if (guildRequestCard != null)
                {
                    return guildRequestCard.m_iReqeustedTime;
                }
            }
            
            return 0;
            */
        }
        /*
        private set
        {
            if (m_LastCardRequestedTime != value)
            {
                m_LastCardRequestedTime = value;
            }
        }
        */
    }

    public int guildCardRequestCycleSec
    {
        get
        {
            if (m_GuildCardRequestCycleSec == 0)
            {
                m_GuildCardRequestCycleSec = entry.data.GetValue<int>(Const_IndexID.Const_Guild_Card_Request_Cycle_Sec);
            }

            return m_GuildCardRequestCycleSec;
        }
    }
    #endregion

    #region Delegates
    public delegate void OnSupportResult(List<CGuildRequestCard> guildRequestCardList, long sequence);
    public OnSupportResult onSupportResult;

    public delegate void OnSupportResultForAnim(List<Goods_Type> revGoodsType);
    public OnSupportResultForAnim onSupportResultForAnim;

    public delegate void OnSupportRequestResult(List<CGuildRequestCard> guildRequestCardList);
    public OnSupportRequestResult onSupportRequestResult;

    public delegate void OnGuildRequestCardListUpdate(List<CGuildRequestCard> guildRequestCardList);
    public OnGuildRequestCardListUpdate onGuildRequestCardListUpdate;

    public delegate void OnGuildReceiveCardResult(int cardIndex, List<CReceicedCard> receivedCardList);
    public OnGuildReceiveCardResult onGuildReceiveCardResult;
    #endregion

    public bool IsSupportableRequest(CGuildRequestCard guildRequestCard, out Result_Define.eResult result)
    {
        result = Result_Define.eResult.DB_ERROR;

        if (guildRequestCard != null)
        {
            if (guildRequestCard.m_RequesterAid != entry.account.userNo)
            {
                DB_Card.Schema card = DB_Card.Query(DB_Card.Field.Index, guildRequestCard.m_iRequestCardIndex);
                if (card != null)
                {
                    int maxCount = 0;
                    switch (card.Grade_Type)
                    {
                        case Grade_Type.Grade_A:
                            maxCount = entry.data.GetValue<int>(Const_IndexID.Const_Guild_Card_A_Support_Limit);
                            break;
                        case Grade_Type.Grade_B:
                            maxCount = entry.data.GetValue<int>(Const_IndexID.Const_Guild_Card_B_Support_Limit);
                            break;
                        case Grade_Type.Grade_C:
                            maxCount = entry.data.GetValue<int>(Const_IndexID.Const_Guild_Card_C_Support_Limit);
                            break;
                        default:
                            LogError("{0}", card.Grade_Type);
                            break;
                    }
                    int supportCount = 0;
                    CGuildSupportCard guildSupportCard = FindGuildSupportCard(guildRequestCard.m_Sequence);
                    if (guildSupportCard != null)
                    {
                        supportCount = guildSupportCard.m_iSendCardCount;
                    }

                    if (supportCount < maxCount)
                    {
                        CSoulInfo soulInfo = entry.character.FindSoulInfo(guildRequestCard.m_iRequestCardIndex);
                        if (soulInfo != null && soulInfo.m_iSoulCount > 0)
                        {
                            result = Result_Define.eResult.SUCCESS;
                        }
                        else
                        {
                            result = Result_Define.eResult.NOT_ENOUGH_SOUL;
                        }
                    }
                    else
                    {
                        result = Result_Define.eResult.SUPPORT_CARD_COUNT_MAX;
                    }
                }
            }
            else
            {
                result = Result_Define.eResult.CAN_NOT_SUPPORT_CARD_YOURSELF;
            }
        }

        return (result == Result_Define.eResult.SUCCESS);
    }

    public CGuildSupportCard FindGuildSupportCard(long sequence)
    {
        if (m_GuildSupportCardList != null && m_GuildSupportCardList.Count > 0)
        {
            return m_GuildSupportCardList.Find(item => item.m_RequestSequence.Equals(sequence));
        }

        return null;
    }

    public CGuildRequestCard FindGuildRequestCard(long sequence)
    {
        if (m_GuildRequestCardList != null && m_GuildRequestCardList.Count > 0)
        {
            return m_GuildRequestCardList.Find(item => item.m_Sequence.Equals(sequence));
        }

        return null;
    }

    #region REQ
    public void REQ_PACKET_CG_GUILD_REQUEST_CARD_SUPPORT_SYN(int cardIndex)
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_REQUEST_CARD_SUPPORT_SYN()
        {
            m_iReqestCardIndex = cardIndex,
        });
    }

    public void REQ_PACKET_CG_GUILD_SUPPORT_CARD_SYN(long sequence)
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_SUPPORT_CARD_SYN()
        {
            m_SupportSequence = sequence,
        });
    }

    public void REQ_PACKET_CG_GUILD_GET_CARD_REQUEST_LIST_SYN()
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_GET_CARD_REQUEST_LIST_SYN(), false);
    }

    public void REQ_PACKET_CG_GUILD_RECEIVED_CARD_SYN()
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_RECEIVED_CARD_SYN());
    }
    #endregion

    #region RCV
    void RCV_PACKET_CG_GUILD_REQUEST_CARD_SUPPORT_ACK(PACKET_CG_GUILD_REQUEST_CARD_SUPPORT_ACK packet)
    {
        guildRequestCardList = packet.m_GuildCardRequestList;

        if (onSupportRequestResult != null)
        {
            onSupportRequestResult(guildRequestCardList);
        }
    }

    void RCV_PACKET_CG_GUILD_SUPPORT_CARD_ACK(PACKET_CG_GUILD_SUPPORT_CARD_ACK packet)
    {
        // ref. PUC-823 (https://mseedgames.atlassian.net/browse/PUC-823)
        CGuildSupportCard guildSupportCard = FindGuildSupportCard(packet.m_SupportSequence);
        if (guildSupportCard != null)
        {
            guildSupportCard.m_iSendCardCount++;
        }
        else LogError("CGuildSupportCard could not be found. (sequence : {0})", packet.m_SupportSequence);

        int TotalGoldCount = packet.m_iTotalGold;
        if (packet.m_bIsAccountLevelUp)
        {
            entry.account.level = packet.m_byCurrentAccountLevel;
            entry.account.heart = packet.m_LevelReward.m_iTotalHeart;
            entry.account.ruby = packet.m_LevelReward.m_iTotalRuby;

            TotalGoldCount = packet.m_LevelReward.m_iTotalTrainingPoint;
        }

        entry.account.exp = (int)packet.m_iExp;
        entry.account.gold = TotalGoldCount;
        entry.account.supportCardCount++;
        entry.character.UpdateSoulInfo(packet.m_RemainSoulInfo);

        if (packet.m_bIsGuildLevelUp)
        {
            entry.guild.guildLevel++;
        }
        entry.guild.guildExp = packet.m_GuildExp;
        entry.account.guildPoint = packet.m_iTotalGuildPoint;
        guildRequestCardList = packet.m_GuildCardRequestList;

        if (onSupportResult != null)
        {
            onSupportResult(guildRequestCardList, packet.m_SupportSequence);
        }

        //** 보상 연출을 위해
        List<Goods_Type> revGoods = new List<Goods_Type>();
        if (packet.m_iReceivedExp > 0)
            revGoods.Add(Goods_Type.AccountExp);
        if (packet.m_iReceivedGold > 0)
            revGoods.Add(Goods_Type.Gold);
        if (packet.m_iReceivedGuildExp > 0)
            revGoods.Add(Goods_Type.GuildExp);
        if (packet.m_iReceivedGuildPoint > 0)
            revGoods.Add(Goods_Type.GuildPoint);

        if (onSupportResultForAnim != null)
            onSupportResultForAnim(revGoods);

    }

    void RCV_PACKET_CG_GUILD_GET_CARD_REQUEST_LIST_ACK(PACKET_CG_GUILD_GET_CARD_REQUEST_LIST_ACK packet)
    {
        totalSupportedCardCount = packet.m_iTotalSupportCount;
        m_LastCardRequestedTime = packet.m_iLastRequestTime;
        // guildRequestCardList 프로퍼티 내부 처리를 위해, guildSupportCardList 프로퍼티를 먼저 호출합니다.
        // (guildRequestCardList 프로퍼티 내부 처리 : m_CardSupportable 값 변경, onGuildRequestCardListUpdate 이벤트 호출 등)
        //guildSupportCardList = packet.m_SupportCardList;
        m_GuildSupportCardList = packet.m_SupportCardList;
        guildRequestCardList = packet.m_RequestCardList;
    }

    void RCV_PACKET_CG_GUILD_RECEIVED_CARD_ACK(PACKET_CG_GUILD_RECEIVED_CARD_ACK packet)
    {
        entry.character.UpdateSoulInfo(packet.m_ReceivedSoulInfo);

        // 서버에서 처리하지 않는 부분 임시 처리
        List<CReceicedCard> receivedCardList = new List<CReceicedCard>();
        if (packet.m_ReceivedCardList != null && packet.m_ReceivedCardList.Count > 0)
        {
            for (int i = 0; i < packet.m_ReceivedCardList.Count; i++)
            {
                CReceicedCard receivedCard = packet.m_ReceivedCardList[i];
                if (receivedCard != null && receivedCard.m_iReceivedCardCount > 0)
                {
                    receivedCardList.Add(receivedCard);
                }
            }
        }

        if (onGuildReceiveCardResult != null)
        {
            onGuildReceiveCardResult(packet.m_ReceivedSoulInfo.m_iSoulIndex, receivedCardList);
        }
    }
    #endregion
}
