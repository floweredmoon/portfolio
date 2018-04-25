using Common.Packet;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildCardRequest : UIObject
{
    public List<Toggle> m_ToggleList;
    public ScrollRect m_ScrollRect;
    public GameObjectPool m_Pool;
    List<UICharCard> m_List = new List<UICharCard>();

    public Vector2 m_CellSize;
    public RectOffset m_Padding;
    public Vector2 m_Spacing;
    public int m_ConstraintCount;

    protected override void Awake()
    {
        base.Awake();

        for (int i = 0; i < m_ToggleList.Count; i++)
        {
            m_ToggleList[i].onValueChanged.AddListener(OnToggleValueChanged);
        }
    }

    // Use this for initialization

    // Update is called once per frame

    protected override void OnEnable()
    {
        base.OnEnable();

        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onSupportRequestResult += OnRequestResult;
        }

        OnToggleValueChanged(true);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onSupportRequestResult -= OnRequestResult;
        }
    }

    void OnRequestResult(List<CGuildRequestCard> guildRequestCardList)
    {
        Kernel.uiManager.Close(ui);
    }

    void OnClicked(UICharCard item)
    {
        Kernel.entry.guild.REQ_PACKET_CG_GUILD_REQUEST_CARD_SUPPORT_SYN(item.cardIndex);
    }

    void BuildLayout()
    {
        float x = m_Padding.left + (m_CellSize.x * .5f);
        float y = -(m_Padding.top + (m_CellSize.y * .5f));
        for (int i = 0; i < m_List.Count; i++)
        {
            if (i > 0 && int.Equals(i % 6, 0))
            {
                x = m_Padding.left + (m_CellSize.x * .5f);
                y = y - m_CellSize.y - m_Spacing.y;
            }

            UICharCard charCard = m_List[i];
            if (charCard)
            {
                charCard.rectTransform.anchoredPosition = new Vector2(x, y);
            }

            x = x + m_CellSize.x + m_Spacing.x;
        }

        float rowCount = Mathf.Ceil((float)m_List.Count / (float)m_ConstraintCount);
        x = (m_ConstraintCount * m_CellSize.x) + ((m_ConstraintCount - 1f) * m_Spacing.x) + m_Padding.left + m_Padding.right;
        y = (rowCount * m_CellSize.y) + ((rowCount - 1f) * m_Spacing.y) + m_Padding.top + m_Padding.bottom;

        m_ScrollRect.content.sizeDelta = new Vector2(x, y);
    }

    void Renewal(ClassType classType)
    {
        Clear();

        List<CCardInfo> cardInfoList = Kernel.entry.character.cardInfoList;
        if (cardInfoList != null && cardInfoList.Count > 0)
        {
            for (int i = 0; i < cardInfoList.Count; i++)
            {
                CCardInfo cardInfo = cardInfoList[i];
                if (cardInfo != null)
                {
                    DB_Card.Schema card = DB_Card.Query(DB_Card.Field.Index, cardInfo.m_iCardIndex);
                    if (card != null)
                    {
                        if (card.Grade_Type >= Grade_Type.Grade_S)
                        {
                            continue;
                        }

                        if (card.ClassType.Equals(classType))
                        {
                            UICharCard charCard = m_Pool.Pop<UICharCard>();
                            if (charCard)
                            {
                                m_List.Add(charCard);
                                UIUtility.SetParent(charCard.transform, m_ScrollRect.content);
                                charCard.cid = cardInfo.m_Cid;
                                charCard.onClicked += OnClicked;
                                charCard.gameObject.SetActive(true);
                            }
                        }
                    }
                }
            }
        }

        BuildLayout();
    }

    void Clear()
    {
        for (int i = 0; i < m_List.Count; i++)
        {
            UICharCard charCard = m_List[i];
            if (charCard)
            {
                charCard.onClicked -= OnClicked;
                charCard.gameObject.SetActive(false);
                UIUtility.SetParent(charCard.transform, transform);
                m_Pool.Push(charCard.gameObject);
            }
        }

        m_List.Clear();
    }

    void OnToggleValueChanged(bool value)
    {
        if (!value)
        {
            return;
        }

        for (int i = 0; i < m_ToggleList.Count; i++)
        {
            Toggle toggle = m_ToggleList[i];
            if (toggle.isOn)
            {
                Renewal((ClassType)i + 2);
                break;
            }
        }
    }
}
