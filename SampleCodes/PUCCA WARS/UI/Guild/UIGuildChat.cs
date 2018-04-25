using Common.Packet;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildChat : MonoBehaviour
{
    public GameObjectPool m_ChatObjectPool;
    public GameObjectPool m_OwnChatObjectPool;
    public GameObjectPool m_SystemChatObjectPool;
    public ScrollRect m_ScrollRect;
    public InputField m_InputField;
    public Button m_SendButton;
    public float m_Top;
    public float m_Bottom;
    public float m_Spacing;

    List<UIGuildChatObject> m_ChatObjectList = new List<UIGuildChatObject>();
    int m_CachedStartIndex;

    void Awake()
    {
        m_SendButton.onClick.AddListener(OnSendButtonClick);

        if (Kernel.entry != null)
        {
            m_InputField.characterLimit = Kernel.entry.data.GetValue<int>(Const_IndexID.Const_Guild_Chat_Character_Limit);
        }
    }

    // Use this for initialization

    // Update is called once per frame

    void OnEnable()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onGuildChatListUpdate += OnGuildChatListUpdate;

            int length = 0;
            List<CGuildChatting> guildChatList = Kernel.entry.guild.guildChatList;
            if (guildChatList != null && guildChatList.Count > 0)
            {
                length = guildChatList.Count;
            }

            OnGuildChatListUpdate(Kernel.entry.guild.guildChatList, m_CachedStartIndex, length - m_CachedStartIndex, true);
            Kernel.entry.guild.isNewChat = false;
        }
    }

    void OnDisable()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onGuildChatListUpdate -= OnGuildChatListUpdate;
        }
    }

    public bool active
    {
        set
        {
            m_InputField.gameObject.SetActive(value);
            m_SendButton.gameObject.SetActive(value);
        }
    }

    bool focusing
    {
        set
        {
            // 0.1f : 임시
            if (value) // || m_ScrollRect.verticalNormalizedPosition < 0.1f)
            {
                m_ScrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }

    void BuildLayout()
    {
        float y = -m_Top;
        for (int i = 0; i < m_ChatObjectList.Count; i++)
        {
            UIGuildChatObject item = m_ChatObjectList[i];
            if (item)
            {
                item.rectTransform.anchoredPosition = new Vector2(0f, y);
                y = y - item.rectTransform.rect.height - m_Spacing;
            }
        }
        y = y + m_Spacing;
        y = y - m_Bottom;
        m_ScrollRect.content.sizeDelta = new Vector2(m_ScrollRect.content.sizeDelta.x, Mathf.Abs(y));
    }

    void OnGuildChatListUpdate(List<CGuildChatting> guildChatList, int startIndex, int length, bool focusing)
    {
        //Debug.Log(string.Format("guildChatList.Count : {0}, startIndex : {1}, length : {2}, focusing : {3}", guildChatList.Count, startIndex, length, focusing));
        if (guildChatList != null && guildChatList.Count > 0)
        {
            for (int i = startIndex; i < startIndex + length; i++)
            {
                CGuildChatting item = guildChatList[i];
                if (item != null)
                {
                    bool isSystem = item.m_AID.Equals(0);
                    bool isOwn = long.Equals(Kernel.entry.account.userNo, guildChatList[i].m_AID);
                    UIGuildChatObject obj = Pop(isSystem, isOwn);
                    if (obj)
                    {
                        obj.guildChatting = guildChatList[i];
                    }
                }
            }

            m_CachedStartIndex = guildChatList.Count;
        }

        BuildLayout();
        this.focusing = focusing;
    }

    void Push(UIGuildChatObject item)
    {
        if (item)
        {
            m_ChatObjectList.Remove(item);
            item.gameObject.SetActive(false);
            UIUtility.SetParent(item.transform, transform);

            if (item.isSystem)
            {
                m_SystemChatObjectPool.Push(item.gameObject);
            }
            else if (item.isOwn)
            {
                m_OwnChatObjectPool.Push(item.gameObject);
            }
            else
            {
                m_ChatObjectPool.Push(item.gameObject);
            }
        }
    }

    UIGuildChatObject Pop(bool isSystem, bool isOwn)
    {
        UIGuildChatObject item;
        if (isSystem)
        {
            item = m_SystemChatObjectPool.Pop<UIGuildChatObject>();
        }
        else if (isOwn)
        {
            item = m_OwnChatObjectPool.Pop<UIGuildChatObject>();

        }
        else
        {
            item = m_ChatObjectPool.Pop<UIGuildChatObject>();
        }

        if (item)
        {
            m_ChatObjectList.Add(item);
            UIUtility.SetParent(item.transform, m_ScrollRect.content);
            item.gameObject.SetActive(true);

            return item;
        }

        return null;
    }

    void OnSendButtonClick()
    {
        if (Kernel.entry != null)
        {
            string value = m_InputField.text;
            if (!string.IsNullOrEmpty(value))
            {
                // Filtering
                Kernel.entry.guild.REQ_PACKET_CG_GUILD_SEND_CHAT_SYN(value);
                m_InputField.text = string.Empty;
            }
        }
    }
}
