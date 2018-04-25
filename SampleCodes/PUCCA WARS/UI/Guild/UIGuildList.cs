using Common.Packet;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildList : MonoBehaviour
{
    public ScrollRect m_ScrollRect;
    public UIScrollRectContentActivator m_ScrollRectContentActivator;
    public GridLayoutGroup m_GridLayoutGroup;
    public GameObjectPool m_GameObjectPool;
    public List<UIGuildObject> m_List;
    public Text m_EmptyText;
    public RectOffset m_Padding;
    public Vector2 m_CellSize;
    public Vector2 m_Spacing;

    protected bool isEmpty
    {
        get
        {
            return m_EmptyText ? m_EmptyText.gameObject.activeSelf : false;
        }

        set
        {
            if (m_EmptyText.gameObject.activeSelf != value)
            {
                m_EmptyText.gameObject.SetActive(value);
            }
        }
    }

    protected virtual void Awake()
    {
        if (m_ScrollRectContentActivator)
        {
            m_ScrollRectContentActivator.Add(m_EmptyText.rectTransform);
        }
    }

    // Use this for initialization

    // Update is called once per frame

    protected virtual void OnEnable()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onGuildListUpdate += OnGuildListUpdate;

            OnGuildListUpdate(Kernel.entry.guild.recommendGuildList, Kernel.entry.guild.waitingApprovalList);
        }
    }

    protected virtual void OnDisable()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onGuildListUpdate -= OnGuildListUpdate;
        }
    }

    protected virtual void OnGuildListUpdate(List<CGuildBase> recommendGuildList, List<CGuildBase> waitingApprovalList)
    {
        Clear();
    }

    protected void BuildLayout()
    {
        float y = -m_Padding.top;
        for (int i = 0; i < m_List.Count; i++)
        {
            RectTransform rectTransform = (RectTransform)m_List[i].transform;
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, y);

            y = y - m_CellSize.y;
            y = y - m_Spacing.y;
        }

        m_ScrollRect.content.sizeDelta = new Vector2(m_ScrollRect.content.sizeDelta.x, Mathf.Abs(y));
    }

    protected void Clear()
    {
        for (int i = 0; i < m_List.Count; i++)
        {
            UIGuildObject item = m_List[i];
            if (item)
            {
                Push(item);
            }
        }

        m_List.Clear();
    }

    protected T Pop<T>() where T : UIGuildObject
    {
        if (m_GameObjectPool)
        {
            UIGuildObject item = m_GameObjectPool.Pop<T>();
            if (item)
            {
                m_List.Add(item);
                UIUtility.SetParent(item.transform, m_ScrollRect.content);
                item.gameObject.SetActive(true);

                return (T)item;
            }
        }

        return null;
    }

    protected void Push(UIGuildObject item)
    {
        if (item && m_GameObjectPool)
        {
            item.gameObject.SetActive(false);
            UIUtility.SetParent(item.transform, m_GameObjectPool.transform);
            m_GameObjectPool.Push(item.gameObject);
        }
    }
}
