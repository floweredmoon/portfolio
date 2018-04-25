using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIAchieveNotification : UIObject
{
    public Text m_AchieveTitleText;
    public float m_Padding;
    public float m_DurationTime;
    float m_StartPosition;
    float m_EndPosition;
    int m_Count;
    float m_FixedDeltaTime2;
    string m_LastTitle;

    protected override void Awake()
    {
        m_StartPosition = rectTransform.anchoredPosition.y;
        m_EndPosition = rectTransform.rect.height + m_Padding;
    }

    protected override void OnEnable()
    {
        StartCoroutine("Animation");
    }

    protected override void OnDisable()
    {
        StopCoroutine("Animation");
    }

    IEnumerator Animation()
    {
        float fixedDeltaTime1 = 0f, y = 0f;
        while (true)
        {
            if (y < m_EndPosition)
            {
                y = (float)PennerDoubleAnimation.CubicEaseOut(fixedDeltaTime1, 0, 1f, m_DurationTime);
                y = y * m_EndPosition;
                y = Mathf.Clamp(y, m_StartPosition, m_EndPosition);
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -y);

                fixedDeltaTime1 = fixedDeltaTime1 + Time.fixedDeltaTime;
            }
            else
            {
                if (m_FixedDeltaTime2 >= m_DurationTime)
                {
                    m_FixedDeltaTime2 = 0f;
                    m_Count = 0;
                    m_LastTitle = string.Empty;
                    Kernel.uiManager.Close(UI.AchieveNotification);

                    yield break;
                }

                m_FixedDeltaTime2 = m_FixedDeltaTime2 + Time.fixedDeltaTime;
            }

            yield return 0;
        }
    }

    public void OnAchieveComplete(int achieveIndex, int achieveGroup, byte achieveStep)
    {
        m_FixedDeltaTime2 = 0f;

        DB_AchieveList.Schema achieveList = DB_AchieveList.Query(DB_AchieveList.Field.Achieve_Group, achieveGroup, DB_AchieveList.Field.Achieve_Step, achieveStep);
        if (achieveList != null)
        {
            DBStr_AchieveString.Schema achieveString = DBStr_AchieveString.Query(DBStr_AchieveString.Field.Achieve_Group, achieveGroup);
            if (achieveString != null)
            {
                string title = string.Empty;
                if (m_Count <= 0)
                {
                    m_LastTitle = title = achieveString.TITLE_STRING;
                }
                else
                {
                    title = string.Format("{0} + {1}", m_LastTitle, m_Count);
                }

                m_AchieveTitleText.text = title;
                m_Count++;
            }
        }
    }

    public void OnDailyAchieveComplete(int achieveIndex)
    {
        m_FixedDeltaTime2 = 0f;

        DB_DailyAchieveList.Schema dailyAchieveList = DB_DailyAchieveList.Query(DB_DailyAchieveList.Field.Index, achieveIndex);
        if (dailyAchieveList != null)
        {
            DBStr_DailyAchieveString.Schema dailyAchieveString = DBStr_DailyAchieveString.Query(DBStr_DailyAchieveString.Field.Index, achieveIndex);
            if (dailyAchieveString != null)
            {
                string title = string.Empty;
                if (m_Count <= 0)
                {
                    m_LastTitle = title = string.Format(dailyAchieveString.TITLE_STRING);
                }
                else
                {
                    title = string.Format("{0} + {1}", m_LastTitle, m_Count);
                }

                m_AchieveTitleText.text = title;
                m_Count++;
            }
        }
    }
}
