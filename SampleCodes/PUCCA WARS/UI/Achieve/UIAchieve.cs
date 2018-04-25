using System.Collections.Generic;
using UnityEngine.UI;

public class UIAchieve : UIObject
{
    public List<Toggle> m_Toggles;
    public Text m_DailyAchieveDescriptionText;
    public UIAchieveList m_DailyAchieveList;
    public UIAchieveList m_AchieveList;

    protected override void Awake()
    {
        for (int i = 0; i < m_Toggles.Count; i++)
        {
            Toggle toggle = m_Toggles[i];
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(OnToggleValueChanged);
            }
        }

        if (Kernel.entry != null)
        {
            m_DailyAchieveDescriptionText.text = Languages.ToString(TEXT_UI.ACHIEVE_RESTART_INFO,
                                                                    Kernel.entry.data.GetValue<int>(Const_IndexID.Const_Achieve_Daily_Reset_Cycle_Hour));
        }
    }

    protected override void OnEnable()
    {
        UIAchieveObject.onInteractableChange += OnAchieveObjectInteractableChange;

        if (Kernel.entry != null)
        {
            Kernel.entry.achieve.onUpdateAchieveList += m_AchieveList.OnUpdateAchieveList;
            Kernel.entry.achieve.onCompleteAchieveResult += m_AchieveList.OnCompleteAchieveResult;
            Kernel.entry.achieve.onUpdateDailyAchieveList += m_DailyAchieveList.OnUpdateDailyAchieveList;
            Kernel.entry.achieve.onCompleteDailyAchieveResult += m_DailyAchieveList.OnCompleteDailyAchieveResult;

            m_AchieveList.OnUpdateAchieveList(Kernel.entry.achieve.achieveDictionary);
            m_DailyAchieveList.OnUpdateDailyAchieveList(Kernel.entry.achieve.dailyAchieveDictionary);
        }

        if (Kernel.achieveManager != null)
        {
            Kernel.achieveManager.onUpdateAchieveBase += m_AchieveList.OnUpdateAchieveBase;
            Kernel.achieveManager.onUpdateDailyAchieveBase += m_DailyAchieveList.OnUpdateDailyAchieveBase;
        }

        OnToggleValueChanged(true);
    }

    protected override void OnDisable()
    {
        UIAchieveObject.onInteractableChange -= OnAchieveObjectInteractableChange;

        if (Kernel.entry != null)
        {
            Kernel.entry.achieve.onUpdateAchieveList -= m_AchieveList.OnUpdateAchieveList;
            Kernel.entry.achieve.onCompleteAchieveResult -= m_AchieveList.OnCompleteAchieveResult;
            Kernel.entry.achieve.onUpdateDailyAchieveList -= m_DailyAchieveList.OnUpdateDailyAchieveList;
            Kernel.entry.achieve.onCompleteDailyAchieveResult -= m_DailyAchieveList.OnCompleteDailyAchieveResult;
        }

        if (Kernel.achieveManager != null)
        {
            Kernel.achieveManager.onUpdateAchieveBase -= m_AchieveList.OnUpdateAchieveBase;
            Kernel.achieveManager.onUpdateDailyAchieveBase -= m_DailyAchieveList.OnUpdateDailyAchieveBase;
        }
    }

    void OnToggleValueChanged(bool value)
    {
        if (!value)
        {
            // To avoid successive invoke.
            return;
        }

        // NullRefExcpt 처리
        m_AchieveList.gameObject.SetActive(m_Toggles[1].isOn);
        m_DailyAchieveList.gameObject.SetActive(m_Toggles[0].isOn);
        m_DailyAchieveDescriptionText.gameObject.SetActive(m_Toggles[0].isOn);
    }

    void OnAchieveObjectInteractableChange(bool interactable)
    {
        if (m_Toggles != null && m_Toggles.Count > 0)
        {
            for (int i = 0; i < m_Toggles.Count; i++)
            {
                Toggle toggle = m_Toggles[i];
                if (toggle != null)
                {
                    toggle.interactable = interactable;
                }
            }
        }
    }
}
