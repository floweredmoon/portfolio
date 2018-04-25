using Common.Packet;
using Common.Util;
using System.Collections.Generic;

public partial class Character : Node
{
    #region Variables
    List<CCardInfo> m_CardInfoList = new List<CCardInfo>();
    List<CSoulInfo> m_SoulInfoList = new List<CSoulInfo>();
    #endregion

    #region Properties
    public List<CCardInfo> cardInfoList
    {
        get
        {
            return m_CardInfoList;
        }

        set
        {
            if (value != null && value.Count > 0)
            {
                for (int i = 0; i < value.Count; i++)
                {
                    UpdateCardInfo(value[i]);
                }
            }
        }
    }

    public List<CSoulInfo> soulInfoList
    {
        get
        {
            return m_SoulInfoList;
        }

        set
        {
            //m_SoulInfoList = value;
            if (value != null && value.Count > 0)
            {
                for (int i = 0; i < value.Count; i++)
                {
                    UpdateSoulInfo(value[i]);
                }
            }
        }
    }

    public List<CDeckData> deckDataList
    {
        get;
        private set;
    }

    public bool isCardInfoListInitialized
    {
        get;
        private set;
    }

    public bool isSoulInfoListInitialized
    {
        get;
        private set;
    }
    #endregion

    #region Delegates
    public delegate void OnDeckLeaderUpdateCallback(int deckNo, long leaderCID);
    public OnDeckLeaderUpdateCallback onDeckLeaderUpdateCallback;

    public delegate void OnDeckDataUpdateCallback(int deckNo, int slotNo, long cid);
    public OnDeckDataUpdateCallback onDeckDataUpdateCallback;

    public delegate void OnCardInfoUpdate(long cid, int cardIndex, bool isNew);
    public OnCardInfoUpdate onCardInfoUpdate;

    public delegate void OnSoulInfoUpdate(long sequence, int soulIndex, int soulCount, int updateCount);
    public OnSoulInfoUpdate onSoulInfoUpdate;

    public delegate void OnCardLevelUp(long cid);
    public OnCardLevelUp onCardLevelUp;

    public delegate void OnEquipmentLevelUpCallback(long cid, eGoodsType goodsType);
    public OnEquipmentLevelUpCallback onEquipmentLevelUpCallback;

    public delegate void OnSkillLevelUpCallback(long cid);
    public OnSkillLevelUpCallback onSkillLevelUpCallback;

    public delegate void OnLevelUpCallback();
    public OnLevelUpCallback onLevelUpCallback;
    #endregion

    public override Node OnCreate()
    {
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_READ_CARD_LIST_ACK>(RCV_PACKET_CG_READ_CARD_LIST_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_READ_SOUL_LIST_ACK>(RCV_PACKET_CG_READ_SOUL_LIST_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_READ_DECK_LIST_ACK>(RCV_PACKET_CG_READ_DECK_LIST_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_CARD_EDIT_DECK_INFO_ACK>(RCV_PACKET_CG_CARD_EDIT_DECK_INFO_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_CARD_LEVEL_UP_ACK>(RCV_PACKET_CG_CARD_LEVEL_UP_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_CARD_ITEM_LEVEL_UP_ACK>(RCV_PACKET_CG_CARD_ITEM_LEVEL_UP_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_CARD_SKILL_LEVEL_UP_ACK>(RCV_PACKET_CG_CARD_SKILL_LEVEL_UP_ACK);

        return base.OnCreate();
    }

    public bool CardLevelUpAvailable(int cardIndex, byte level) // ref bool
    {
        if (level < entry.data.GetValue<byte>(Const_IndexID.Const_Card_Level_Limit))
        {
            CSoulInfo soulInfo = FindSoulInfo(cardIndex);
            if (soulInfo != null)
            {
                DB_Card.Schema card = DB_Card.Query(DB_Card.Field.Index, cardIndex);
                if (card != null)
                {
                    DB_CardLevelUp.Schema cardLevelUp = DB_CardLevelUp.Query(DB_CardLevelUp.Field.Grade_Type, card.Grade_Type,
                                                                             DB_CardLevelUp.Field.CardLevel, level);
                    if (cardLevelUp != null)
                    {
                        return (soulInfo.m_iSoulCount >= cardLevelUp.Count) &&
                               (entry.account.gold >= cardLevelUp.Need_Gold);
                    }
                }
            }
        }

        return false;
    }

    public bool SkillLevelUpAvailable(int cardIndex, byte skillLevel)
    {
        if (skillLevel < entry.data.GetValue<byte>(Const_IndexID.Const_Skill_Level_Limit))
        {
            DB_Card.Schema card = DB_Card.Query(DB_Card.Field.Index, cardIndex);
            if (card != null)
            {
                DB_SkillLevelUp.Schema skillLevelUp = DB_SkillLevelUp.Query(DB_SkillLevelUp.Field.Grade_Type, card.Grade_Type,
                                                                            DB_SkillLevelUp.Field.Skill_Level, skillLevel);
                if (skillLevelUp != null)
                {
                    return (entry.account.GetValue(card.ClassType) >= skillLevelUp.Count) &&
                           (entry.account.gold >= skillLevelUp.Need_Gold);
                }
            }
        }

        return false;
    }

    public bool EquipmentLevelUpAvailable(int cardIndex, Goods_Type equipment)
    {
        DB_Card.Schema card = DB_Card.Query(DB_Card.Field.Index, cardIndex);
        if (card != null)
        {
            byte level;
            if (TryGetEquipmentLevel(cardIndex, equipment, out level))
            {
                if (level < entry.data.GetValue<byte>(Const_IndexID.Const_Equipment_Level_Limit))
                {
                    int requiredTicket, requiredGold;
                    if (Settings.Equipment.TryGetLevelUpCondition(card.Grade_Type, equipment, level, out requiredTicket, out requiredGold))
                    {
                        return (Kernel.entry.account.GetValue(equipment) >= requiredTicket) &&
                               (Kernel.entry.account.gold >= requiredGold);
                    }
                }
            }
        }

        return false;
    }

    public bool TryGetEquipmentLevel(int cardIndex, Goods_Type equipment, out byte level)
    {
        level = 0;

        CCardInfo cardInfo = FindCardInfo(cardIndex);
        if (cardInfo != null)
        {
            switch (equipment)
            {
                case Goods_Type.EquipUpAccessory:
                    level = cardInfo.m_byAccessoryLV;
                    break;
                case Goods_Type.EquipUpArmor:
                    level = cardInfo.m_byArmorLV;
                    break;
                case Goods_Type.EquipUpWeapon:
                    level = cardInfo.m_byWeaponLV;
                    break;
            }
        }

        return true;
    }

    public void UpdateSoulInfo(CSoulInfo value)
    {
        if (value != null)
        {
            CSoulInfo soulInfo = FindSoulInfo(value.m_Sequence);
            int updateCount = 0;
            if (soulInfo != null)
            {
                updateCount = value.m_iSoulCount - soulInfo.m_iSoulCount;
                // DEEP COPY!
                soulInfo.m_iSoulCount = value.m_iSoulCount;
                soulInfo.m_iSoulIndex = value.m_iSoulIndex;
                soulInfo.m_Sequence = value.m_Sequence;
            }
            else
            {
                updateCount = value.m_iSoulCount;
                m_SoulInfoList.Add(soulInfo = value);
            }

            if (onSoulInfoUpdate != null)
            {
                onSoulInfoUpdate(soulInfo.m_Sequence, soulInfo.m_iSoulIndex, soulInfo.m_iSoulCount, isSoulInfoListInitialized ? updateCount : 0);
            }
        }
    }

    public void UpdateSoulInfo(long sequence, int soulCount)
    {
        CSoulInfo soulInfo = FindSoulInfo(sequence);
        if (soulInfo != null)
        {
            int updateCount = soulCount - soulInfo.m_iSoulCount;

            soulInfo.m_iSoulCount = soulCount;

            if (onSoulInfoUpdate != null)
            {
                onSoulInfoUpdate(soulInfo.m_Sequence, soulInfo.m_iSoulIndex, soulInfo.m_iSoulCount, isSoulInfoListInitialized ? updateCount : 0);
            }
        }
    }

    public void UpdateCardInfo(CCardInfo value)
    {
        if (value != null)
        {
            bool isNew;
            CCardInfo cardInfo = FindCardInfo(value.m_Cid);
            if (cardInfo != null)
            {
                // DEEP COPY!
                cardInfo.m_byAccessoryLV = value.m_byAccessoryLV;
                cardInfo.m_byArmorLV = value.m_byArmorLV;
                cardInfo.m_byLevel = value.m_byLevel;
                cardInfo.m_bySkill = value.m_bySkill;
                cardInfo.m_byWeaponLV = value.m_byWeaponLV;
                cardInfo.m_Cid = value.m_Cid;
                cardInfo.m_iCardIndex = value.m_iCardIndex;
                cardInfo.m_bIsNew = value.m_bIsNew;

                isNew = false;
            }
            else
            {
                m_CardInfoList.Add(cardInfo = value);

                isNew = true;
            }

            // ref. PUC-736
            // 임시 처리
            //if (isNew)
            {
                int newCardCount = 0;
                for (int i = 0; i < m_CardInfoList.Count; i++)
                {
                    if (m_CardInfoList[i].m_bIsNew)
                    {
                        newCardCount++;
                    }
                }

                if (m_NewCardCount != newCardCount)
                {
                    this.newCardCount = newCardCount;
                }
            }

            if (onCardInfoUpdate != null)
            {
                onCardInfoUpdate(cardInfo.m_Cid, cardInfo.m_iCardIndex, isCardInfoListInitialized && isNew);
            }
        }
    }

    public void SetMainDeck(int deckNo)
    {
        bool dirty = false;
        bool isMainDeck = false;
        for (int i = 0; i < deckDataList.Count; i++)
        {
            isMainDeck = int.Equals(deckDataList[i].m_iDeckNum, deckNo);
            if (deckDataList[i].m_bIsMainDeck != isMainDeck)
            {
                deckDataList[i].m_bIsMainDeck = isMainDeck;
                dirty = true;
            }
        }

        if (dirty)
        {
            SetDirty(true);
        }
    }

    public bool UpdateDeckData(int deckNo, int slotNo, long cid)
    {
        // deckNo, slotNo, cid가 유효한 값인지 확인해야 합니다.
        CDeckData deckData = FindDeckData(deckNo);
        if (deckData != null)
        {
            // 해당 cid가 이미 존재하는 경우, 해당 slotNo의 cid 값을 비웁니다.
            int tempSlotNo = deckData.m_CardCidList.IndexOf(cid);
            if (tempSlotNo > -1)
            {
                deckData.m_CardCidList[tempSlotNo] = 0;
            }

            // Warning, ArgOutOfRngExcpt!
            long tempCID = deckData.m_CardCidList[slotNo];
            deckData.m_CardCidList[slotNo] = cid;

            SetDirty(true);

            if (onDeckDataUpdateCallback != null)
            {
                if (tempSlotNo > -1)
                {
                    onDeckDataUpdateCallback(deckNo, tempSlotNo, 0);
                }

                onDeckDataUpdateCallback(deckNo, slotNo, cid);

                // 무효한 deckNo, slotNo로 이벤트 처리
                if (tempCID > 0)
                {
                    onDeckDataUpdateCallback(0, 0, tempCID);
                }
            }

            //
            if (tempSlotNo > -1)
            {
                UpdateDeckLeader(deckData, tempCID);
            }
            else if (long.Equals(tempCID, deckData.m_LeaderCid))
            {
                UpdateDeckLeader(deckData, cid);
            }

            return true;
        }

        return false;
    }

    public bool IsDeckLeader(int deckNo, long cid)
    {
        CDeckData deckData = FindDeckData(deckNo);
        if (deckData != null)
        {
            return long.Equals(deckData.m_LeaderCid, cid);
        }

        return false;
    }

    public bool UpdateDeckLeader(int deckNo, long cid)
    {
        CDeckData deckData = FindDeckData(deckNo);
        if (deckData != null)
        {
            return UpdateDeckLeader(deckData, cid);
        }

        return false;
    }

    public bool UpdateDeckLeader(CDeckData deckData, long cid)
    {
        if (deckData != null &&
            deckData.m_CardCidList.Contains(cid) &&
            deckData.m_LeaderCid != cid)
        {
            deckData.m_LeaderCid = cid;
            SetDirty(true);

            if (onDeckLeaderUpdateCallback != null)
            {
                onDeckLeaderUpdateCallback(deckData.m_iDeckNum, deckData.m_LeaderCid);
            }

            return true;
        }

        return false;
    }

    public CSoulInfo FindSoulInfo(int cardIndex)
    {
        if (m_SoulInfoList != null && m_SoulInfoList.Count > 0)
        {
            DB_Soul.Schema soul = DB_Soul.Query(DB_Soul.Field.Card_List_Link, cardIndex);
            if (soul != null)
            {
                return m_SoulInfoList.Find(item => Equals(item.m_iSoulIndex, soul.Index));
            }
        }

        return null;
    }

    public CSoulInfo FindSoulInfo(long sequence)
    {
        if (m_SoulInfoList != null && m_SoulInfoList.Count > 0)
        {
            return m_SoulInfoList.Find(item => Equals(item.m_Sequence, sequence));
        }

        return null;
    }

    public CDeckData FindMainDeckData()
    {
        return (deckDataList != null && deckDataList.Count > 0) ? deckDataList.Find(item => item.m_bIsMainDeck) : null;
    }

    public CDeckData FindDeckData(int deckNo)
    {
        return (deckDataList != null && deckDataList.Count > 0) ? deckDataList.Find(item => Equals(item.m_iDeckNum, deckNo)) : null;
    }

    public CDeckData FindDeckData(long cid)
    {
        if (deckDataList != null && deckDataList.Count > 0)
        {
            for (int i = 0; i < deckDataList.Count; i++)
            {
                CDeckData deckData = deckDataList[i];
                if (deckData.m_CardCidList.Contains(cid))
                {
                    return deckData;
                }
            }
        }

        return null;
    }

    public CCardInfo FindCardInfo(int cardIndex)
    {
        return (m_CardInfoList != null && m_CardInfoList.Count > 0) ? m_CardInfoList.Find(item => Equals(item.m_iCardIndex, cardIndex)) : null;
    }

    public CCardInfo FindCardInfo(long cid)
    {
        return (m_CardInfoList != null && m_CardInfoList.Count > 0) ? m_CardInfoList.Find(item => Equals(item.m_Cid, cid)) : null;
    }

    #region REQ
    public void REQ_PACKET_CG_READ_CARD_LIST_SYN()
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_READ_CARD_LIST_SYN()
            {
                m_AID = entry.account.userNo,
                m_iDBIndex = entry.account.dbNo,
                m_iAuthKey = entry.account.AuthKey,
            });
    }

    public void REQ_PACKET_CG_READ_SOUL_LIST_SYN()
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_READ_SOUL_LIST_SYN()
            {
                m_AID = entry.account.userNo,
                m_iDBIndex = entry.account.dbNo,
                m_iAuthKey = entry.account.AuthKey,
            });
    }

    public void REQ_PACKET_CG_READ_DECK_LIST_SYN()
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_READ_DECK_LIST_SYN()
            {
                m_AID = entry.account.userNo,
                m_iDBIndex = entry.account.dbNo,
                m_iAuthKey = entry.account.AuthKey,
            });
    }

    public void REQ_PACKET_CG_CARD_EDIT_DECK_INFO_SYN()
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_CARD_EDIT_DECK_INFO_SYN()
            {
                m_DeckList = deckDataList,
            });
    }

    public void REQ_PACKET_CG_CARD_LEVEL_UP_SYN(long cid)
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_CARD_LEVEL_UP_SYN()
        {
            m_Cid = cid,
        });
    }

    public void REQ_PACKET_CG_CARD_ITEM_LEVEL_UP_SYN(long cid, eItemType itemType)
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_CARD_ITEM_LEVEL_UP_SYN()
            {
                m_Cid = cid,
                m_eItemType = itemType,
            });
    }

    public void REQ_PACKET_CG_CARD_SKILL_LEVEL_UP_SYN(long cid)
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_CARD_SKILL_LEVEL_UP_SYN()
            {
                m_Cid = cid,
            });
    }
    #endregion

    #region RCV
    void RCV_PACKET_CG_READ_CARD_LIST_ACK(PACKET_CG_READ_CARD_LIST_ACK packet)
    {
        cardInfoList = packet.m_CardList;

        // 임시 처리
        if (m_CardInfoList.Count > 0)
        {
            for (int i = 0; i < m_CardInfoList.Count; i++)
            {
                Directed(m_CardInfoList[i].m_iCardIndex);
            }
        }

        isCardInfoListInitialized = true;
    }

    void RCV_PACKET_CG_READ_SOUL_LIST_ACK(PACKET_CG_READ_SOUL_LIST_ACK packet)
    {
        soulInfoList = packet.m_SoulList;
        isSoulInfoListInitialized = true;
    }

    void RCV_PACKET_CG_READ_DECK_LIST_ACK(PACKET_CG_READ_DECK_LIST_ACK packet)
    {
        deckDataList = packet.m_DeckList;
    }

    void RCV_PACKET_CG_CARD_EDIT_DECK_INFO_ACK(PACKET_CG_CARD_EDIT_DECK_INFO_ACK packet)
    {
        //deckDataList = packet.m_DeckList;
        SetDirty(false);
    }

    void RCV_PACKET_CG_CARD_LEVEL_UP_ACK(PACKET_CG_CARD_LEVEL_UP_ACK packet)
    {
        entry.account.gold = packet.m_iRemainGold;
        UpdateSoulInfo(packet.m_SoulSeq, packet.m_iSoulRemainCount);
        UpdateCardInfo(packet.m_CardInfo);

        if (onCardLevelUp != null)
        {
            onCardLevelUp(packet.m_CardInfo.m_Cid);
        }

        //if (onLevelUpCallback != null)
        //    onLevelUpCallback();
    }

    void RCV_PACKET_CG_CARD_ITEM_LEVEL_UP_ACK(PACKET_CG_CARD_ITEM_LEVEL_UP_ACK packet)
    {
        UpdateCardInfo(packet.m_CardInfo);
        entry.account.gold = packet.m_iRemainGold;

        switch (packet.m_eTicketType)
        {
            case eGoodsType.EquipUpAccessory:
                entry.account.accessoryTicket = packet.m_iRemainTicket;
                break;
            case eGoodsType.EquipUpArmor:
                entry.account.armorTicket = packet.m_iRemainTicket;
                break;
            case eGoodsType.EquipUpWeapon:
                entry.account.weaponTicket = packet.m_iRemainTicket;
                break;
        }

        if (onEquipmentLevelUpCallback != null)
        {
            onEquipmentLevelUpCallback(packet.m_CardInfo.m_Cid, packet.m_eTicketType);
        }

        //if (onLevelUpCallback != null)
        //    onLevelUpCallback();
    }

    void RCV_PACKET_CG_CARD_SKILL_LEVEL_UP_ACK(PACKET_CG_CARD_SKILL_LEVEL_UP_ACK packet)
    {
        UpdateCardInfo(packet.m_CardInfo);
        entry.account.gold = packet.m_iRemainGold;

        switch (packet.m_eTicketType)
        {
            case eGoodsType.SkillUpHealer:
                entry.account.bufferTicket = packet.m_iRemainTicket;
                break;
            case eGoodsType.SkillUpHitter:
                entry.account.attackerTicket = packet.m_iRemainTicket;
                break;
            case eGoodsType.SkillUpKeeper:
                entry.account.defenderTicket = packet.m_iRemainTicket;
                break;
            case eGoodsType.SkillUpRanger:
                entry.account.rangerTicket = packet.m_iRemainTicket;
                break;
            case eGoodsType.SkillUpWizard:
                entry.account.debufferTicket = packet.m_iRemainTicket;
                break;
        }

        if (onSkillLevelUpCallback != null)
        {
            onSkillLevelUpCallback(packet.m_CardInfo.m_Cid);
        }

        //if (onLevelUpCallback != null)
        //    onLevelUpCallback();
    }
    #endregion
}
