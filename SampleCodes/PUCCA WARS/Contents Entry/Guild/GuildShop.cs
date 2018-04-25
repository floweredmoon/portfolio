using Common.Packet;
using System.Collections.Generic;

public partial class Guild
{
    #region Variables
    int m_GuildShopResetTime;
    Dictionary<int, byte> m_BuyCountDictionary = new Dictionary<int, byte>();
    #endregion

    public int guildShopResetTime
    {
        get
        {
            return m_GuildShopResetTime;
        }
    }

    #region Delegates
    public delegate void OnBuyResult(int itemIndex, byte buyCount);
    public OnBuyResult onBuyResult;

    public delegate void OnBuyCountListUpdate();
    public OnBuyCountListUpdate onBuyCountListUpdate;

    public delegate void OnBuyCountUpdate(int itemIndex, byte buyCount);
    public OnBuyCountUpdate onBuyCountUpdate;
    #endregion

    public byte GetBuyCount(int itemIndex)
    {
        if (m_BuyCountDictionary.ContainsKey(itemIndex))
        {
            return m_BuyCountDictionary[itemIndex];
        }

        return 0;
    }

    void UpdateBuyCount(int itemIndex, byte buyCount)
    {
        m_BuyCountDictionary[itemIndex] = buyCount;

        if (onBuyCountUpdate != null)
        {
            onBuyCountUpdate(itemIndex, buyCount);
        }
    }

    #region REQ
    public void REQ_PACKET_CG_GUILD_BUY_GUILD_SHOP_ITEM_SYN(int itemIndex)
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_BUY_GUILD_SHOP_ITEM_SYN()
        {
            m_iShopItemIndex = itemIndex,
        });
    }

    public void REQ_PACKET_CG_GUILD_GET_SHOP_BUY_COUNT_LIST_SYN()
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_GET_SHOP_BUY_COUNT_LIST_SYN());
    }
    #endregion

    #region RCV
    void RCV_PACKET_CG_GUILD_BUY_GUILD_SHOP_ITEM_ACK(PACKET_CG_GUILD_BUY_GUILD_SHOP_ITEM_ACK packet)
    {
        UpdateBuyCount(packet.m_iShopItemIndex, packet.m_byBuyCount);

        entry.account.guildPoint = packet.m_iRemainGuildPoint;
        entry.account.SetValue(packet.m_eGoodsType, packet.m_iGoodsCount);

        if (packet.m_CardList != null && packet.m_CardList.Count > 0)
        {
            entry.character.cardInfoList = packet.m_CardList;
        }

        if (packet.m_SoulList != null && packet.m_SoulList.Count > 0)
        {
            entry.character.soulInfoList = packet.m_SoulList;
        }

        if (onBuyResult != null)
        {
            onBuyResult(packet.m_iShopItemIndex, packet.m_byBuyCount);
        }
    }

    void RCV_PACKET_CG_GUILD_GET_SHOP_BUY_COUNT_LIST_ACK(PACKET_CG_GUILD_GET_SHOP_BUY_COUNT_LIST_ACK packet)
    {
        m_BuyCountDictionary.Clear();

        for (int i = 0; i < packet.m_GuildShopBuyCountList.Count; i++)
        {
            CGuildShopBuyCount item = packet.m_GuildShopBuyCountList[i];
            if (item != null)
            {
                if (m_BuyCountDictionary.ContainsKey(item.m_iShopIndex))
                {
                    LogError("{0}", item.m_iShopIndex);
                }
                else
                {
                    m_BuyCountDictionary.Add(item.m_iShopIndex, item.m_byBuyCount);
                }
            }
        }

        if (onBuyCountListUpdate != null)
        {
            onBuyCountListUpdate();
        }
    }
    #endregion
}
