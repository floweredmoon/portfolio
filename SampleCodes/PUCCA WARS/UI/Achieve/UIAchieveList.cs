using Common.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIAchieveList : MonoBehaviour
{
    #region Variables
    public ScrollRect m_ScrollRect;
    public GameObjectPool m_AchieveObjectPool;
    public Text m_DescriptionText; // For daily achieve list.
    public float m_Spacing;
    // Key : achieveGroup or achieveIndex.
    Dictionary<int, UIAchieveObject> m_AchieveObjectDictionary = new Dictionary<int, UIAchieveObject>();
    StringBuilder m_StringBuilder = new StringBuilder(64); // 64 : 예상
    bool m_DailyAchieveListRequested;
    int m_AchieveDailyResetCycleHour;
    #endregion

    #region MonoBehaviour
    // Use this for initialization
    void Start()
    {
        if (Kernel.entry != null)
        {
            m_AchieveDailyResetCycleHour = Kernel.entry.data.GetValue<int>(Const_IndexID.Const_Achieve_Daily_Reset_Cycle_Hour);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // To Coroutine or InvokeRepeating.
        if (m_DescriptionText != null && Kernel.entry != null && Kernel.entry.achieve.isCompleteAllDailyAchieve)
        {
            DateTime lastDailyAchieveCompleteTime = TimeUtility.ToDateTime(Kernel.entry.achieve.lastDailyAchieveCompleteTime);
            lastDailyAchieveCompleteTime = lastDailyAchieveCompleteTime.AddHours(m_AchieveDailyResetCycleHour);
            TimeSpan ts = lastDailyAchieveCompleteTime - TimeUtility.currentServerTime;
            m_StringBuilder.Remove(0, m_StringBuilder.Length);
            // 로컬라이징 시 데이터 및 코드 변경이 필요합니다.
            if (ts.Hours > 0)
            {
                m_StringBuilder.AppendFormat("{0}{1}", ts.Hours, Languages.ToString(TEXT_UI.HOUR));
            }
            m_StringBuilder.AppendFormat(" {0}{1}", ((ts.Minutes > 0) ? ts.Minutes : 1), Languages.ToString(TEXT_UI.MINUTE));

            m_DescriptionText.text = Languages.ToString(TEXT_UI.ACHIEVE_RESTART, m_StringBuilder.ToString());

            // To Achieve.Update().
            if (!m_DailyAchieveListRequested && ts.TotalSeconds <= 0)
            {
                Kernel.entry.achieve.REQ_PACKET_CG_READ_DAILY_ACHIEVE_LIST_SYN();
                m_DailyAchieveListRequested = true;
            }
        }
    }

    void OnEnable()
    {
        if (Kernel.networkManager != null)
        {
            Kernel.networkManager.onException += OnAchieveCompleteException;
        }

        UIAchieveObject.interactable = true;
    }

    void OnDisable()
    {
        if (Kernel.networkManager != null)
        {
            Kernel.networkManager.onException -= OnAchieveCompleteException;
        }

        UIAchieveObject.interactable = true;
    }
    #endregion

    void OnAchieveCompleteException(Result_Define.eResult result, string error, ePACKET_CATEGORY packetCategory, byte packetIndex)
    {
        switch (packetCategory)
        {
            case ePACKET_CATEGORY.CG_GAME:
                switch (packetIndex)
                {
                    case (byte)eCG_GAME.COMPLETE_ACHEIVE_ACK:
                    case (byte)eCG_GAME.COMPLETE_DAILU_ACHEIVE_ACK:
                        UIAchieveObject.interactable = true;
                        break;
                }
                break;
        }
    }

    #region Daily Achieve
    public void OnUpdateDailyAchieveBase(int achieveIndex, int achieveAccumulate, bool isCompleted)
    {
        UIAchieveObject achieveObject;
        if (m_AchieveObjectDictionary.TryGetValue(achieveIndex, out achieveObject))
        {
            CDailyAchieve dailyAchieve = Kernel.entry.achieve.FindDailyAchieve(achieveIndex);
            if (dailyAchieve != null)
            {
                achieveObject.SetDailyAchieve(dailyAchieve, null, false);
            }
            else Debug.LogError(string.Format("CDailyAchieve could not be found. (achieveIndex : {0}", achieveIndex));
        }
        else Debug.LogError(string.Format("UIAchieveObject could not be found. (achieveIndex : {0}", achieveIndex));

        if (isCompleted)
        {
            BuildLayout();
        }
    }

    public void OnCompleteDailyAchieveResult(CDailyAchieve dailyAchieve, CReceivedGoods receivedGoods, bool isLevelUp)
    {
        if (dailyAchieve != null)
        {
            UIAchieveObject achieveObject;
            if (m_AchieveObjectDictionary.TryGetValue(dailyAchieve.m_iAchieveIndex, out achieveObject))
            {
                if (achieveObject != null)
                {
                    achieveObject.OnCompleteDailyAchieveResult(dailyAchieve, receivedGoods, isLevelUp);
                }
                else Debug.LogError(string.Format("UIAchieveObject has been destroyed. but you are still trying to access it. (achieveIndex : {0})", dailyAchieve.m_iAchieveIndex));
            }

            bool isCompleteAllDailyAchieve = Kernel.entry.achieve.isCompleteAllDailyAchieve;
            if (isCompleteAllDailyAchieve)
            {
                RemoveAll();
                //BuildLayout();
                m_DescriptionText.gameObject.SetActive(isCompleteAllDailyAchieve);
            }
            else
            {
                BuildLayout();
            }

            UIAchieveObject.interactable = true;
        }
    }

    public void OnUpdateDailyAchieveList(Dictionary<int, CDailyAchieve> dailyAchieveDictionary)
    {
        if (m_DailyAchieveListRequested)
        {
            m_DailyAchieveListRequested = false;
        }

        RemoveAll();

        bool isCompleteAllDailyAchieve = Kernel.entry.achieve.isCompleteAllDailyAchieve;
        if (!isCompleteAllDailyAchieve)
        {
            foreach (var dailyAchieve in dailyAchieveDictionary.Values)
            {
                if (dailyAchieve != null)
                {
                    UIAchieveObject achieveObject = m_AchieveObjectPool.Pop<UIAchieveObject>();
                    if (achieveObject != null)
                    {
                        UIUtility.SetParent(achieveObject.transform, m_ScrollRect.content);
                        m_AchieveObjectDictionary.Add(dailyAchieve.m_iAchieveIndex, achieveObject);
                        achieveObject.SetDailyAchieve(dailyAchieve, null, false);
                        achieveObject.gameObject.SetActive(true);
                    }
                }
            }
        }

        BuildLayout();
        m_DescriptionText.gameObject.SetActive(isCompleteAllDailyAchieve);
    }
    #endregion

    #region Achieve
    public void OnUpdateAchieveBase(int achieveIndex, int achieveGroup, int achieveStep, int achieveAccumulate, bool isCompleted)
    {
        UIAchieveObject achieveObject;
        if (m_AchieveObjectDictionary.TryGetValue(achieveGroup, out achieveObject))
        {
            CAchieve achieve = Kernel.entry.achieve.FindAchieve(achieveGroup);
            if (achieve != null)
            {
                achieveObject.SetAchieve(achieveGroup, achieve.m_byCompleteStep, achieveAccumulate);
            }
            else Debug.LogError(string.Format("CAchieve could not be found. (achieveGroup : {0})", achieveGroup));
        }
        else Debug.LogError(string.Format("UIAchieveObject could not be found. (achieveGroup : {0})", achieveGroup));

        if (isCompleted)
        {
            BuildLayout();
        }
    }

    void OnCompleteAchieveAnimationFinished()
    {
        BuildLayout();
    }

    public void OnCompleteAchieveResult(int achieveGroup, byte achieveCompleteStep, int achieveAccumulate, CReceivedGoods receivedGoods, bool isLevelUp)
    {
        UIAchieveObject achieveObject;
        if (m_AchieveObjectDictionary.TryGetValue(achieveGroup, out achieveObject))
        {
            achieveObject.OnCompleteAchieveResult(achieveGroup, achieveCompleteStep, achieveAccumulate, receivedGoods, isLevelUp);
        }
    }

    public void OnUpdateAchieveList(Dictionary<int, CAchieve> achieveDictionary)
    {
        RemoveAll();

        foreach (var schema in DB_AchieveList.instance.schemaList)
        {
            int achieveGroup = schema.Achieve_Group;
            UIAchieveObject achieveObject;
            // achieveGroup 당 하나의 UIAchieveObject를 생성합니다.
            if (!m_AchieveObjectDictionary.TryGetValue(achieveGroup, out achieveObject))
            {
                achieveObject = m_AchieveObjectPool.Pop<UIAchieveObject>();
                if (achieveObject != null)
                {
                    if (achieveObject.achieveListTransform == null)
                    {
                        achieveObject.achieveListTransform = transform;
                    }

                    byte achieveCompleteStep = 0;
                    int achieveAccumulate = 0;
                    CAchieve achieve;
                    if (achieveDictionary != null && achieveDictionary.TryGetValue(achieveGroup, out achieve))
                    {
                        achieveCompleteStep = achieve.m_byCompleteStep;
                        achieveAccumulate = achieve.m_iAchieveAccumulatedAmount;
                    }
                    else Debug.LogError(string.Format("CAchieve could not be found. (achieveGroup : {0})", achieveGroup));

                    // 완료되지 않은 업적의 achieveAccumulate는 AchieveBase의 값을 사용합니다.
                    if (achieveCompleteStep < Kernel.entry.achieve.achieveLastStep)
                    {
                        AchieveBase achieveBase = Kernel.achieveManager.FindAchieveBase(achieveGroup);
                        if (achieveBase != null)
                        {
                            achieveAccumulate = achieveBase.achieveAccumulate;
                        }
                        else Debug.LogError(string.Format("AchieveBase could not be found. (achieveGroup : {0})", achieveGroup));
                    }

                    UIUtility.SetParent(achieveObject.transform, m_ScrollRect.content);
                    m_AchieveObjectDictionary.Add(achieveGroup, achieveObject);
                    achieveObject.onCompleteAchieveAnimationFinished += OnCompleteAchieveAnimationFinished;
                    achieveObject.SetAchieve(achieveGroup, achieveCompleteStep, achieveAccumulate);
                    achieveObject.gameObject.SetActive(true);
                }
            }
        }

        BuildLayout();
    }
    #endregion

    void BuildLayout()
    {
        float y = 0f;
        if (m_AchieveObjectDictionary != null && m_AchieveObjectDictionary.Count > 0)
        {
            List<UIAchieveObject> achieveObjects = m_AchieveObjectDictionary.Values
                .OrderBy(item => item.isCompleted) // isCompleted 오름차순 정렬
                .ThenByDescending(item => item.acquirable) // acquirable 내림차순 정렬
                .ThenBy(item => item.achieveIndex).ToList<UIAchieveObject>(); // achieveIndex 오름차순 정렬
            if (achieveObjects != null && achieveObjects.Count > 0)
            {
                for (int i = 0; i < achieveObjects.Count; i++)
                {
                    UIAchieveObject achieveObject = achieveObjects[i];
                    if (achieveObject != null)
                    {
                        RectTransform rectTransform = achieveObject.rectTransform;
                        if (rectTransform != null)
                        {
                            rectTransform.anchoredPosition = new Vector2(0f, y);
                            y = y - rectTransform.rect.height - m_Spacing;
                        }

                        // UIScrollRectContentActivator 컴포넌트에 의해 비활성화될 경우가 있어, 임시 처리합니다.
                        if (!achieveObject.gameObject.activeSelf)
                        {
                            achieveObject.gameObject.SetActive(true);
                        }
                    }
                }

                y = y - m_Spacing;
            }
        }

        m_ScrollRect.content.sizeDelta = new Vector2(m_ScrollRect.content.sizeDelta.x, Mathf.Abs(y));
    }

    void RemoveAll()
    {
        if (m_AchieveObjectDictionary != null && m_AchieveObjectDictionary.Count > 0)
        {
            foreach (var achieveObject in m_AchieveObjectDictionary.Values)
            {
                if (achieveObject != null)
                {
                    if (!achieveObject.isDaily)
                    {
                        achieveObject.onCompleteAchieveAnimationFinished -= OnCompleteAchieveAnimationFinished;
                    }

                    achieveObject.gameObject.SetActive(false);
                    UIUtility.SetParent(achieveObject.transform, transform);
                    m_AchieveObjectPool.Push(achieveObject.gameObject);
                }
            }

            m_AchieveObjectDictionary.Clear();
        }
    }
}
