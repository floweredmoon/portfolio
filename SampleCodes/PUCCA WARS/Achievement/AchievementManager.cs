using Common.Packet;
using System;
using System.Collections.Generic;
using UnityEngine;

public partial class AchievementManager : Singleton<AchievementManager>
{

    #region Network Events

    private void OnUpdateAchievement(CAchieve achieve)
    {
        if (achieve == null)
            return;

        var achvComp = FindNormalArchievementComponent(achieve.m_iAchieveIndex);
        if (achieve != null)
            achvComp.currentScore = achieve.m_iAchieveAccumulatedAmount;
    }

    private void OnCompleteAchievement(int achvGroup, int completedStep, int currentScore)
    {
        var achvComp = FindNormalArchievementComponent(achvGroup);
        if (achvComp == null)
            return;

        var isFinal = completedStep >= Configuration.GetValue<int>(ConstType.AchievementFinal);
        if (isFinal)
            RemoveNormalAchievementComponent(achvGroup);
        else
            achvComp.Update(completedStep + 1, currentScore);
    }

    private void OnUpdateAchievements(Dictionary<int, CAchieve> achievements)
    {
        // [IMPLEMENT]
        // Remove All Achievement Components!

        foreach (var achieveList in DB_AchieveList.instance.schemaList)
        {
            var achvGroup = achieveList.Achieve_Group;
            var achvComp = FindNormalArchievementComponent(achvGroup);
            if (achvComp != null)
                continue;
        }
    }

    #endregion

    private AchievementComponent CreateAchievementInstance(Type achvCompType)
    {
        AchievementComponent achvComp = null;
        // 디버그 편의를 위해 게임 오브젝트를 생성합니다.
        var gameObject = new GameObject(achvCompType.ToString());
        achvComp = gameObject.AddComponent(achvCompType) as AchievementComponent;
        gameObject.transform.SetParent(transform);

        return achvComp;
    }
}

public partial class AchievementManager
{

    // [IMPLEMENT]
    // int -> enum

    #region Fields

    // 진행 중인 일반 업적
    // Key : DB_AchieveList.Achieve_Group
    private Dictionary<int, AchievementComponent> m_NormalAchievements = new Dictionary<int, AchievementComponent>();
    // 목표를 달성한 일반 업적의 DB_AchieveList.Achieve_Group 리스트
    // [WARNING]
    // 완료 (보상 획득)가 아닙니다.
    private List<int> m_FinishedNormalAchievements = new List<int>();

    #endregion

    #region Delegates

    public delegate void OnUpdateNormalAchievement(int achvIndex, int achvGroup, int currentStep, int currentScore, bool isFinished);
    public OnUpdateNormalAchievement onUpdateNormalAchievement = delegate { };

    public delegate void OnFinishedNormalArchievement(int achvIndex, int achvGroup, int currentStep);
    public OnFinishedNormalArchievement onFinishedNormalArchievement = delegate { };

    #endregion

    public void UpdateNormalArchievement(int achvIndex, int achvGroup, int currentStep, int currentScore, bool isFinished)
    {
        onUpdateNormalAchievement(achvIndex, achvGroup, currentStep, currentScore, isFinished);
    }

    public void FinishNormalArchievement(int achvIndex, int achvGroup, int currentStep)
    {
        var achievement = FindNormalArchievementComponent(achvGroup);
        if (AddFinishedNormalArchievement(achievement))
            onFinishedNormalArchievement(achvIndex, achvGroup, currentStep);
    }

    private bool AddFinishedNormalArchievement(AchievementComponent achvComp)
    {
        var isAdded = false;
        if (achvComp != null && achvComp.isFinished)
            m_FinishedNormalAchievements.Add(achvComp.achvGroup);
        // [IMPLEMENT]
        // 

        return isAdded;
    }

    public AchievementComponent FindNormalArchievementComponent(int achvGroup)
    {
        return m_NormalAchievements.ContainsKey(achvGroup) ? m_NormalAchievements[achvGroup] : null;
    }

    private bool RemoveNormalAchievementComponent(int achvGroup)
    {
        var isRemoved = false;
        AchievementComponent achvComp = null;
        if (isRemoved = m_NormalAchievements.TryGetValue(achvGroup, out achvComp))
        {
            m_NormalAchievements.Remove(achvGroup);
            Destroy(achvComp.gameObject);
        }

        return isRemoved;
    }

    private AchievementComponent AddNormalAchievementComponent(int achvGroup, int currentStep, int currentScore)
    {
        AchievementComponent achvComp = null;
        var achieveList = DB_AchieveList.Query(
            DB_AchieveList.Field.Achieve_Group, achvGroup,
            DB_AchieveList.Field.Achieve_Step, currentStep);
        if (achieveList != null &&
            // 현재 단계와 마지막 단계가 같더라도 완료 (보상 획득)하지 않으면 생성되어야 합니다.
            currentStep <= Configuration.GetValue<int>(ConstType.AchievementFinal))
        {
            achvComp = FindNormalArchievementComponent(achvGroup);
            // 중복 확인
            if (achvComp == null)
            {
                Type achvCompType;
                if (NormalAchievementDictionary.TryGetValue(achvGroup, out achvCompType))
                    achvComp = CreateAchievementInstance(achvCompType);

                if (achvComp != null)
                {
                    achvComp.Initialize(achieveList, currentStep, currentScore);

                    if (!AddFinishedNormalArchievement(achvComp))
                        m_NormalAchievements.Add(achvGroup, achvComp);
                }
            }
        }

        return achvComp;
    }
}

public partial class AchievementManager
{

    // [IMPLEMENT]
    // int -> enum

    #region Fields

    // 진행 중인 일일 업적
    // Key : DB_DailyAchieveList.Index
    private Dictionary<int, AchievementComponent> m_DailyAchievements = new Dictionary<int, AchievementComponent>();
    // 목표를 달성한 일일 업적의 DB_DailyAchieveList.Index 리스트
    // [WARNING]
    // 완료 (보상 획득)가 아닙니다.
    private List<int> m_FinishedDailyAchievements = new List<int>();

    #endregion

    #region Delegates

    public delegate void OnUpdateDailyAchievement(int achvIndex, int currentScore, bool isFinished);
    public OnUpdateDailyAchievement onUpdateDailyAchievement = delegate { };

    public delegate void OnFinishDailyAchievement(int achvIndex);
    public OnFinishDailyAchievement onFinishDailyAchievement = delegate { };

    #endregion

    public void UpdateDailyAchievement(int achvIndex, int currentScore, bool isFinished)
    {
        onUpdateDailyAchievement(achvIndex, currentScore, isFinished);
    }

    public void FinishDailyAchievement(int achvIndex)
    {
        var achvComp = FindDailyAchievementComponent(achvIndex);
        if (AddFinishedDailyAchievement(achvComp))
            onFinishDailyAchievement(achvIndex);
    }

    public bool AddFinishedDailyAchievement(AchievementComponent achvComp)
    {
        var isAdded = false;
        if (achvComp != null && achvComp.isFinished)
            m_FinishedDailyAchievements.Add(achvComp.achvIndex);
        // [IMPLEMENT]
        // 

        return isAdded;
    }

    public AchievementComponent FindDailyAchievementComponent(int achvIndex)
    {
        return m_DailyAchievements.ContainsKey(achvIndex) ? m_DailyAchievements[achvIndex] : null;
    }

    private bool RemoveDailyAchievementComponent(int achvIndex)
    {
        var isRemoved = false;
        AchievementComponent achvComp = null;
        if (isRemoved = m_DailyAchievements.TryGetValue(achvIndex, out achvComp))
        {
            m_DailyAchievements.Remove(achvIndex);
            Destroy(achvComp.gameObject);
        }

        return isRemoved;
    }

    private AchievementComponent AddDailyAchievementComponent(CDailyAchieve dailyAchieve)
    {
        AchievementComponent achvComp = null;
        // 완료 (보상 획득)한 업적은 컴포넌트를 생성하지 않습니다.
        if (dailyAchieve != null && !dailyAchieve.m_bIsComplete)
        {
            var achvIndex = dailyAchieve.m_iAchieveIndex;
            var dailyAchieveList = DB_DailyAchieveList.Query(DB_DailyAchieveList.Field.Index, achvIndex);
            if (dailyAchieveList != null)
            {
                Type achvCompType;
                if (DailyAchievementDictionary.TryGetValue(achvIndex, out achvCompType))
                    achvComp = CreateAchievementInstance(achvCompType);

                if (achvComp != null)
                {
                    achvComp.Initialize(dailyAchieveList, dailyAchieve.m_iAchieveAccumulatedAmount);
                    if (!AddFinishedDailyAchievement(achvComp))
                        m_DailyAchievements.Add(achvIndex, achvComp);
                }
            }
        }

        return achvComp;
    }
}
