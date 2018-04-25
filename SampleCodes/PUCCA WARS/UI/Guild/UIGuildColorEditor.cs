using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ToggleGroup))]
public class UIGuildColorEditor : MonoBehaviour
{
    public List<UIButton> m_Buttons;

    public int count
    {
        get
        {
            return (m_Buttons != null) ? m_Buttons.Count : 0;
        }
    }

    public Color color
    {
        get;
        private set;
    }

    public delegate void OnColorPick(Color color);
    public OnColorPick onColorPick;

    // Use this for initialization

    // Update is called once per frame

    public void Initialize()
    {
        for (int i = 0; i < m_Buttons.Count; i++)
        {
            UIButton button = m_Buttons[i];
            button.onClicked += OnClicked;
            Color color;
            if (Kernel.colorManager && Kernel.colorManager.TryGetColor(string.Format("guild_color_{0:00}", (i + 1)), out color))
            {
                button.targetGraphic.color = color;
            }
        }
    }

    public void Pick(int index)
    {
        if (m_Buttons != null && m_Buttons.Count > index)
        {
            OnClicked(m_Buttons[index]);
            //m_Buttons[index].onClick.Invoke();
        }
    }

    void OnClicked(UIButton button)
    {
        color = button.targetGraphic.color;

        if (onColorPick != null)
        {
            onColorPick(color);
        }
    }
}
