using Common.Packet;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildSupportList : MonoBehaviour
{
    public ScrollRect m_ScrollRect;
    public Button m_RequestButton;
    public Text m_RequestButtonText;
    public GameObjectPool m_Pool;
    public Text m_EmptyText;

    public float m_Top;
    public float m_Bottom;
    public float m_Spacing;

    List<UIGuildSupportObject> m_List = new List<UIGuildSupportObject>();
    List<long> m_SequenceList = new List<long>();

    void Awake()
    {
        m_RequestButton.onClick.AddListener(OnRequestButtonClick);
    }

    // Use this for initialization

    // Update is called once per frame
    void Update()
    {
        if (Kernel.entry == null)
        {
            return;
        }

        if (Kernel.entry.guild.cardRequestable)
        {
            m_RequestButton.image.overrideSprite = null;
            m_RequestButtonText.text = Languages.ToString(TEXT_UI.REQUEST);
        }
        else
        {
            int unixEpoch = (int)TimeUtility.currentServerTimeUnixEpoch - Kernel.entry.guild.lastCardRequestedTime;
            TimeSpan ts = TimeSpan.FromSeconds((Kernel.entry.guild.guildCardRequestCycleSec - unixEpoch));
            Sprite disabledSprite = TextureManager.GetSprite(SpritePackingTag.Extras, "ui_button_disable");

            m_RequestButton.image.overrideSprite = disabledSprite;
            m_RequestButtonText.text = string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);

            foreach (var item in m_RequestButtonText.GetComponents<Shadow>())
            {
                if (item is Outline)
                {
                    Color effectColor;
                    Kernel.colorManager.TryGetColor("ui_button_05_outline", out effectColor);
                    item.effectColor = effectColor;
                }
                else if (item is Shadow)
                {
                    Color effectColor;
                    Kernel.colorManager.TryGetColor("ui_button_05_shadow", out effectColor);
                    item.effectColor = effectColor;
                }
            }
        }
    }

    void OnEnable()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onGuildRequestCardListUpdate += OnGuildRequestCardListUpdate;
            Kernel.entry.guild.onSupportResult += OnSupportResult;

            OnGuildRequestCardListUpdate(Kernel.entry.guild.guildRequestCardList);
        }

        if (Kernel.packetRequestIterator)
        {
            Kernel.packetRequestIterator.AddPacketRequestInfo<PACKET_CG_GUILD_GET_CARD_REQUEST_LIST_SYN>(4f);
        }
    }

    void OnDisable()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onGuildRequestCardListUpdate -= OnGuildRequestCardListUpdate;
            Kernel.entry.guild.onSupportResult -= OnSupportResult;
        }

        if (Kernel.packetRequestIterator)
        {
            Kernel.packetRequestIterator.RemovePacketRequestInfo<PACKET_CG_GUILD_GET_CARD_REQUEST_LIST_SYN>();
        }
    }

    bool isEmpty
    {
        set
        {
            if (m_EmptyText.gameObject.activeSelf != value)
            {
                m_EmptyText.gameObject.SetActive(value);
            }
        }
    }

    public bool active
    {
        set
        {
            m_RequestButton.gameObject.SetActive(value);
        }
    }

    void OnSupportResult(List<CGuildRequestCard> guildRequestCardList, long sequence)
    {
        CGuildRequestCard guildRequestCard = Kernel.entry.guild.FindGuildRequestCard(sequence);
        if (guildRequestCard != null)
        {
            UIGuildSupportObject guildSupportObject = m_List.Find(item => item.sequence == sequence);
            if (guildSupportObject != null)
            {
                guildSupportObject.guildRequestCard = guildRequestCard;
            }
        }
    }

    void OnGuildRequestCardListUpdate(List<CGuildRequestCard> guildRequestCardList)
    {
        m_SequenceList.Clear();
        if (guildRequestCardList != null && guildRequestCardList.Count > 0)
        {
            for (int i = 0; i < guildRequestCardList.Count; i++)
            {
                CGuildRequestCard guildRequestCard = guildRequestCardList[i];
                // ref. PUC-560
                // 서버에서 처리되지 않는 부분, 임시 처리합니다.
                if (guildRequestCard.m_bIsReceiveComplete ||
                    guildRequestCard.m_iReceivedCardCount.Equals(guildRequestCard.m_iMaxCardCount))
                {
                    continue;
                }

                UIGuildSupportObject guildSupportObject = Find(guildRequestCard.m_Sequence);
                if (guildSupportObject == null)
                {
                    guildSupportObject = Pop();
                }

                if (guildSupportObject != null)
                {
                    guildSupportObject.guildRequestCard = guildRequestCard;

                    m_SequenceList.Add(guildRequestCard.m_Sequence);
                }
            }
        }

        isEmpty = (m_SequenceList.Count == 0);

        for (int i = 0; i < m_List.Count; i++)
        {
            UIGuildSupportObject guildSupportObject = m_List[i];
            if (guildSupportObject)
            {
                if (!m_SequenceList.Contains(guildSupportObject.sequence))
                {
                    Push(guildSupportObject);
                }
            }
        }

        BuildLayout();
    }

    UIGuildSupportObject Find(long sequence)
    {
        return m_List.Find(item => long.Equals(item.sequence, sequence));
    }

    void BuildLayout()
    {
        float y = -m_Top;
        for (int i = 0; i < m_List.Count; i++)
        {
            UIGuildSupportObject item = m_List[i];
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

    void OnRequestButtonClick()
    {
        if (Kernel.entry == null)
        {
            return;
        }

        SoundDataInfo.ChangeUISound(UISOUND.UIS_CANCEL_01, m_RequestButton.gameObject);

        if (Kernel.entry.guild.cardRequestable)
        {
            SoundDataInfo.RevertSound(m_RequestButton.gameObject);
            Kernel.uiManager.Open(UI.GuildCardRequest);
        }
        else
        {
            NetworkEventHandler.OnNetworkException(Result_Define.eResult.CARD_REQUEST_TIME_LIMITED);
        }
    }

    void Push(UIGuildSupportObject item)
    {
        if (item)
        {
            item.sequence = 0; // Dispose.
            item.gameObject.SetActive(false);
            m_List.Remove(item);
            UIUtility.SetParent(item.transform, m_Pool.transform);
            m_Pool.Push(item.gameObject);
        }
    }

    UIGuildSupportObject Pop()
    {
        UIGuildSupportObject item = m_Pool.Pop<UIGuildSupportObject>();
        if (item)
        {
            m_List.Add(item);
            UIUtility.SetParent(item.transform, m_ScrollRect.content);
            item.gameObject.SetActive(true);

            return item;
        }

        return null;
    }
}
