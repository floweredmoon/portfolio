using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AchievementComponent : MonoBehaviour
{

    private int m_Score;

    #region Properties

    public int achvIndex
    {
        get;
        private set;
    }

    public int achvGroup
    {
        get;
        private set;
    }

    public int currentStep
    {
        get;
        private set;
    }

    public int currentScore
    {
        get
        {
            return m_Score;
        }

        set
        {
            if (m_Score != value)
            {
                var isFinished = this.isFinished;

                m_Score = value;

                if (isDaily)
                {
                    AchievementManager.instance.UpdateDailyAchievement(achvIndex, m_Score, this.isFinished);

                    if (!isFinished)
                        AchievementManager.instance.FinishDailyAchievement(achvIndex);
                }
                else
                {
                    AchievementManager.instance.UpdateNormalArchievement(achvIndex, achvGroup, currentStep, m_Score, this.isFinished);

                    if (!isFinished)
                        AchievementManager.instance.FinishNormalArchievement(achvIndex, achvGroup, currentStep);
                }
            }
        }
    }

    public int finalScore
    {
        get;
        private set;
    }

    public bool isFinished
    {
        get
        {
            return currentScore >= finalScore;
        }
    }

    public bool isDaily
    {
        get;
        private set;
    }

    #endregion

    #region Abstract Methods

    protected abstract void OnEnable();

    protected abstract void OnDisable();

    #endregion

    public void Update(int currentStep, int currentScore)
    {
        this.currentStep = currentStep;
        this.currentScore = currentScore;
    }

    public void Initialize(DB_AchieveList.Schema achieveList, int currentStep, int currentScore)
    {
        if (achieveList != null)
        {
            this.achvGroup = achvGroup;
            this.currentStep = currentStep;
            this.currentScore = currentScore;
            achvIndex = achieveList.Index;
            finalScore = achieveList.Terms_COUNT;
            isDaily = false;
        }
    }

    public void Initialize(DB_DailyAchieveList.Schema dailyAchieveList, int currentScore)
    {
        if (dailyAchieveList != null)
        {
            this.achvIndex = dailyAchieveList.Index;
            this.currentScore = currentScore;
            finalScore = dailyAchieveList.Terms_Count;
            isDaily = true;
        }
    }
}
