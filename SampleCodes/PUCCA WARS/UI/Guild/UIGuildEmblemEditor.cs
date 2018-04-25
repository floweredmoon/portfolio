using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ToggleSiblingGroup))]
public class UIGuildEmblemEditor : MonoBehaviour
{
    public Button m_RandomButton;
    public List<Toggle> m_Toggles;
    public List<UIGuildEmblemToggle> m_EmblemToggles;

    public int count
    {
        get
        {
            return (m_EmblemToggles != null) ? m_EmblemToggles.Count : 0;
        }
    }

    public delegate void OnRandomButtonClicked();
    public OnRandomButtonClicked onRandomButtonClicked;

    void Awake()
    {
        m_RandomButton.onClick.AddListener(OnRandomButtonClick);

        for (int i = 0; i < m_Toggles.Count; i++)
        {
            m_Toggles[i].onValueChanged.AddListener(OnToggleValueChange);
        }
    }

    // Use this for initialization

    // Update is called once per frame

    public void Pick(int index)
    {
        if (m_EmblemToggles != null && m_EmblemToggles.Count > index)
        {
            UIGuildEmblemToggle emblemToggle = m_EmblemToggles[index];
            if (emblemToggle)
            {
                emblemToggle.isOn = true;
            }
        }
    }

    void OnRandomButtonClick()
    {
        if (onRandomButtonClicked != null)
        {
            onRandomButtonClicked();
        }
    }

    public void OnToggleValueChange(bool value)
    {
        if (!value)
        {
            return;
        }

        int index = m_Toggles.FindIndex(item => item.isOn);
        if (index > -1)
        {
            for (int i = 0; i < m_EmblemToggles.Count; i++)
            {
                UIGuildEmblemToggle emblemToggle = m_EmblemToggles[i];
                if (emblemToggle)
                {
                    emblemToggle.SetEmblem(i);
                }
            }
        }
    }
}
