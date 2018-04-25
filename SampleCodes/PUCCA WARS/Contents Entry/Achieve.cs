using Common.Packet;
using System.Collections.Generic;

public class Achieve : Node
{
    // Key is achieveGroup.
    Dictionary<int, CAchieve> m_AchieveDictionary = new Dictionary<int, CAchieve>();
    // Key is achieveIndex.
    Dictionary<int, CDailyAchieve> m_DailyAchieveDictionary = new Dictionary<int, CDailyAchieve>();
    int m_AchieveLastStep;

    #region Properties
    public Dictionary<int, CAchieve> achieveDictionary
    {
        get
        {
            return m_AchieveDictionary;
        }
    }

    public Dictionary<int, CDailyAchieve> dailyAchieveDictionary
    {
        get
        {
            return m_DailyAchieveDictionary;
        }
    }

    // 마지막 일일 업적 완료 시간
    public int lastDailyAchieveCompleteTime
    {
        get;
        private set;
    }

    public bool isCompleteAllDailyAchieve
    {
        get;
        private set;
    }

    public int achieveLastStep
    {
        get
        {
            if (m_AchieveLastStep == 0)
            {
                m_AchieveLastStep = entry.data.GetValue<int>(Const_IndexID.Const_Achieve_Last_Step);
            }

            return m_AchieveLastStep;
        }
    }
    #endregion

    #region Delegates
    public delegate void OnUpdateAchieveList(Dictionary<int, CAchieve> achieveDictionary);
    public OnUpdateAchieveList onUpdateAchieveList;

    public delegate void OnCompleteAchieveResult(int achieveGroup, byte achieveCompleteStep, int achieveAccumulate, CReceivedGoods receivedGoods, bool isLevelUp);
    public OnCompleteAchieveResult onCompleteAchieveResult;

    public delegate void OnUpdateDailyAchieveList(Dictionary<int, CDailyAchieve> dailyAchieveDictionary);
    public OnUpdateDailyAchieveList onUpdateDailyAchieveList;

    public delegate void OnCompleteDailyAchieveResult(CDailyAchieve dailyAchieve, CReceivedGoods receivedGoods, bool isLevelUp);
    public OnCompleteDailyAchieveResult onCompleteDailyAchieveResult;

    public delegate void OnUpdateAchieve(CAchieve achieve);
    public OnUpdateAchieve onUpdateAchieve;

    public delegate void OnUpdateDailyAchieve(CDailyAchieve dailyAchieve);
    public OnUpdateDailyAchieve onUpdateDailyAchieve;
    #endregion

    public override Node OnCreate()
    {
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_READ_COMPLETE_ACHIEVE_LIST_ACK>(RCV_PACKET_CG_READ_COMPLETE_ACHIEVE_LIST_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GAME_COMPLETE_ACHIEVE_ACK>(RCV_PACKET_CG_GAME_COMPLETE_ACHIEVE_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_READ_DAILY_ACHIEVE_LIST_ACK>(RCV_PACKET_CG_READ_DAILY_ACHIEVE_LIST_ACK);
        entry.packetBroadcaster.AddPacketListener<PACKET_CG_GAME_COMPLETE_DAILY_ACHIEVE_ACK>(RCV_PACKET_CG_GAME_COMPLETE_DAILY_ACHIEVE_ACK);

        return base.OnCreate();
    }

    #region Daily Achieve
    public void UpdateDailyAchieve(CDailyAchieve dailyAchieve)
    {
        if (dailyAchieve != null)
        {
            CDailyAchieve temp = FindDailyAchieve(dailyAchieve.m_iAchieveIndex);
            if (temp != null)
            {
                // DEEP COPY!
                temp.m_bIsComplete = dailyAchieve.m_bIsComplete;
                temp.m_iAchieveAccumulatedAmount = dailyAchieve.m_iAchieveAccumulatedAmount;
                temp.m_iCompleteTime = dailyAchieve.m_iCompleteTime;
                temp.m_Sequence = dailyAchieve.m_Sequence;

                // lastDailyAchieveCompleteTime, isCompleteAllDailyAchieve 갱신
                bool isCompleteAllDailyAchieve = true;
                foreach (var item in m_DailyAchieveDictionary.Values)
                {
                    if (lastDailyAchieveCompleteTime < item.m_iCompleteTime)
                    {
                        lastDailyAchieveCompleteTime = item.m_iCompleteTime;
                    }

                    isCompleteAllDailyAchieve = (isCompleteAllDailyAchieve && item.m_bIsComplete);
                }
                this.isCompleteAllDailyAchieve = isCompleteAllDailyAchieve;

                if (onUpdateDailyAchieve != null)
                {
                    onUpdateDailyAchieve(temp);
                }
            }
            else LogError("CDailyAchieve could not be found. (achieveIndex : {0})", dailyAchieve.m_iAchieveIndex);
        }
    }

    public void UpdateDailyAchieve(int achieveIndex, int achieveAccumulate)
    {
        CDailyAchieve dailyAchieve = FindDailyAchieve(achieveIndex);
        if (dailyAchieve != null)
        {
            dailyAchieve.m_iAchieveAccumulatedAmount = achieveAccumulate;

            if (onUpdateDailyAchieve != null)
            {
                onUpdateDailyAchieve(dailyAchieve);
            }
        }
        else LogError("CDailyAchieve could not be found. (achieveIndex : {0})", achieveIndex);
    }

    public CDailyAchieve FindDailyAchieve(int achieveIndex)
    {
        return (m_DailyAchieveDictionary != null) && (m_DailyAchieveDictionary.ContainsKey(achieveIndex)) ? m_DailyAchieveDictionary[achieveIndex] : null;
    }
    #endregion

    #region Achieve
    public void UpdateAchieve(int achieveGroup, int achieveAccumulate, byte achieveCompleteStep)
    {
        CAchieve achieve = FindAchieve(achieveGroup);
        if (achieve != null)
        {
            // DEEP COPY!
            achieve.m_byCompleteStep = achieveCompleteStep;
            achieve.m_iAchieveAccumulatedAmount = achieveAccumulate;

            if (onUpdateAchieve != null)
            {
                onUpdateAchieve(achieve);
            }
        }
        else LogError("CAchieve could not be found. (achieveGroup : {0})", achieveGroup);
    }

    public CAchieve FindAchieve(int achieveGroup)
    {
        return (m_AchieveDictionary != null) && (m_AchieveDictionary.ContainsKey(achieveGroup)) ? m_AchieveDictionary[achieveGroup] : null;
    }
    #endregion

    #region REQ
    public void REQ_PACKET_CG_READ_COMPLETE_ACHIEVE_LIST_SYN()
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_READ_COMPLETE_ACHIEVE_LIST_SYN());
    }

    public void REQ_PACKET_CG_GAME_COMPLETE_ACHIEVE_SYN(int achieveIndex)
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GAME_COMPLETE_ACHIEVE_SYN()
        {
            m_iAchieveIndex = achieveIndex,
        },
        false);
    }

    public void REQ_PACKET_CG_READ_DAILY_ACHIEVE_LIST_SYN()
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_READ_DAILY_ACHIEVE_LIST_SYN());
    }

    public void REQ_PACKET_CG_GAME_COMPLETE_DAILY_ACHIEVE_SYN(int achieveIndex)
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GAME_COMPLETE_DAILY_ACHIEVE_SYN()
            {
                m_iAchieveIndex = achieveIndex,
            });
    }
    #endregion

    #region RCV
    void RCV_PACKET_CG_READ_COMPLETE_ACHIEVE_LIST_ACK(PACKET_CG_READ_COMPLETE_ACHIEVE_LIST_ACK packet)
    {
        m_AchieveDictionary.Clear();
        foreach (var schema in DB_AchieveList.instance.schemaList)
        {
            int achieveGroup = schema.Achieve_Group;

            if (!m_AchieveDictionary.ContainsKey(schema.Achieve_Group))
            {
                CAchieve achieve = null;
                if (packet.m_CompleteAchieveList != null && packet.m_CompleteAchieveList.Count > 0)
                {
                    achieve = packet.m_CompleteAchieveList.Find(item => item.m_iAchieveGroup == achieveGroup);
                }
                // 한 번도 수행하지 않은 업적은 null, 클라이언트에서 임의로 생성합니다.
                if (achieve == null)
                {
                    achieve = new CAchieve()
                    {
                        m_iAchieveGroup = achieveGroup,
                    };
                }

                m_AchieveDictionary.Add(achieveGroup, achieve);
            }
        }

        if (onUpdateAchieveList != null)
        {
            onUpdateAchieveList(m_AchieveDictionary);
        }
    }

    void RCV_PACKET_CG_GAME_COMPLETE_ACHIEVE_ACK(PACKET_CG_GAME_COMPLETE_ACHIEVE_ACK packet)
    {
        if (packet.m_AchieveInfo != null)
        {
            UpdateAchieve(packet.m_AchieveInfo.m_iAchieveGroup,
                                  packet.m_AchieveInfo.m_iAchieveAccumulatedAmount,
                                  packet.m_AchieveInfo.m_byCompleteStep);

        }
        else LogError("PACKET_CG_GAME_COMPLETE_ACHIEVE_ACK.m_AchieveInfo is null");

        if (packet.m_AchieveCompleteGoodsReward != null)
        {
            entry.account.SetValue(packet.m_AchieveCompleteGoodsReward.m_eGoodsType, packet.m_AchieveCompleteGoodsReward.m_iTotalAmount);
        }
        else LogError("PACKET_CG_GAME_COMPLETE_ACHIEVE_ACK.m_AchieveCompleteGoodsReward is null.");

        entry.account.exp = packet.m_iExp;

        if (packet.m_bIsLevelUp)
        {
            // 업적 보상 획득 연출, 계정 레벨 업 연출 순서에 문제가 있어
            // onLevelUpdate 대리자를 호출하지 않기 위해 SetLevelWithoutUpdateCallback 함수를 사용합니다.
            //entry.account.level++;
            entry.account.SetLevelWithoutUpdateCallback((byte)(entry.account.level + 1));

            if (packet.m_LevelUpReward != null)
            {
                entry.account.heart = packet.m_LevelUpReward.m_iTotalHeart;
                entry.account.ruby = packet.m_LevelUpReward.m_iTotalRuby;
                entry.account.gold = packet.m_LevelUpReward.m_iTotalTrainingPoint;
            }
            else LogError("PACKET_CG_GAME_COMPLETE_ACHIEVE_ACK.m_LevelUpReward is null.");
        }

        if (onCompleteAchieveResult != null)
        {
            onCompleteAchieveResult(packet.m_AchieveInfo.m_iAchieveGroup,
                                    packet.m_AchieveInfo.m_byCompleteStep,
                                    packet.m_AchieveInfo.m_iAchieveAccumulatedAmount,
                                    packet.m_AchieveCompleteGoodsReward,
                                    packet.m_bIsLevelUp);
        }
    }

    void RCV_PACKET_CG_READ_DAILY_ACHIEVE_LIST_ACK(PACKET_CG_READ_DAILY_ACHIEVE_LIST_ACK packet)
    {
        bool isCompleteAllDailyAchieve = true;

        m_DailyAchieveDictionary.Clear();
        if (packet.m_DailyAchieveList != null)
        {
            if (packet.m_DailyAchieveList.Count > 0)
            {
                for (int i = 0; i < packet.m_DailyAchieveList.Count; i++)
                {
                    CDailyAchieve dailyAchieve = packet.m_DailyAchieveList[i];
                    if (dailyAchieve != null)
                    {
                        isCompleteAllDailyAchieve = isCompleteAllDailyAchieve && dailyAchieve.m_bIsComplete;

                        if (!m_DailyAchieveDictionary.ContainsKey(dailyAchieve.m_iAchieveIndex))
                        {
                            // 마지막 일일 업적 완료 시간
                            if (lastDailyAchieveCompleteTime < dailyAchieve.m_iCompleteTime)
                            {
                                lastDailyAchieveCompleteTime = dailyAchieve.m_iCompleteTime;
                            }

                            m_DailyAchieveDictionary.Add(dailyAchieve.m_iAchieveIndex, dailyAchieve);
                        }
                        else LogError("CDailyAchieve is duplicated. (achieveIndex : {0})", dailyAchieve.m_iAchieveIndex);
                    }
                }
            }
            else LogError("PACKET_CG_GAME_COMPLETE_ACHIEVE_ACK.m_DailyAchieveList is null.");
        }
        else
        {
            LogError("PACKET_CG_GAME_COMPLETE_ACHIEVE_ACK.m_DailyAchieveList is null.");
        }

        // isCompleteAllDailyAchieve 갱신
        this.isCompleteAllDailyAchieve = isCompleteAllDailyAchieve;

        if (onUpdateDailyAchieveList != null)
        {
            onUpdateDailyAchieveList(m_DailyAchieveDictionary);
        }
    }

    void RCV_PACKET_CG_GAME_COMPLETE_DAILY_ACHIEVE_ACK(PACKET_CG_GAME_COMPLETE_DAILY_ACHIEVE_ACK packet)
    {
        CDailyAchieve dailyAchieve = null;
        if (packet.m_DailyAchieveInfo != null)
        {
            dailyAchieve = FindDailyAchieve(packet.m_DailyAchieveInfo.m_iAchieveIndex);
            if (dailyAchieve != null)
            {
                UpdateDailyAchieve(packet.m_DailyAchieveInfo);
            }
            else LogError("CDailyAchieve (achieveIndex : {0}) could not be found.", packet.m_DailyAchieveInfo.m_iAchieveIndex);
        }
        else LogError("PACKET_CG_GAME_COMPLETE_DAILY_ACHIEVE_ACK.m_DailyAchieveInfo is null.");

        if (packet.m_AchieveCompleteGoodsReward != null)
        {
            entry.account.SetValue(packet.m_AchieveCompleteGoodsReward.m_eGoodsType, packet.m_AchieveCompleteGoodsReward.m_iTotalAmount);
        }
        else LogError("PACKET_CG_GAME_COMPLETE_DAILY_ACHIEVE_ACK.m_AchieveCompleteGoodsReward is null.");

        entry.account.exp = packet.m_iExp;

        if (packet.m_bIsLevelUp)
        {
            // 업적 보상 획득 연출, 계정 레벨 업 연출 순서에 문제가 있어
            // onLevelUpdate 대리자를 호출하지 않기 위해 SetLevelWithoutUpdateCallback 함수를 사용합니다.
            //entry.account.level++;
            entry.account.SetLevelWithoutUpdateCallback((byte)(entry.account.level + 1));

            if (packet.m_LevelUpReward != null)
            {
                entry.account.heart = packet.m_LevelUpReward.m_iTotalHeart;
                entry.account.ruby = packet.m_LevelUpReward.m_iTotalRuby;
                entry.account.gold = packet.m_LevelUpReward.m_iTotalTrainingPoint;
            }
            else LogError("PACKET_CG_GAME_COMPLETE_DAILY_ACHIEVE_ACK.m_LevelUpReward is null.");
        }

        if (onCompleteDailyAchieveResult != null)
        {
            onCompleteDailyAchieveResult(dailyAchieve,
                                         packet.m_AchieveCompleteGoodsReward,
                                         packet.m_bIsLevelUp);
        }
    }
    #endregion
}
