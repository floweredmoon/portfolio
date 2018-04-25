using Common.Packet;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildChatObject : MonoBehaviour
{
    public Image m_BackgroundImage;
    public Image m_PortraitImage;
    public Image m_FrameImage;
    public Text m_NameText;
    public RectTransform m_BubbleRectTransform;
    public Text m_ChatText;
    public Image m_TimeFrameImage;
    public Text m_TimeText;

    public bool m_IsOwn;
    public float m_Right;
    public float m_Bottom;
    static Vector2 m_MinSize;
    static Vector2 m_MaxSize;
    public long m_Sequence;
    public long m_AID;
    DateTime m_RegTime;

    public bool isOwn
    {
        get
        {
            return m_IsOwn;
        }
    }

    public bool isSystem
    {
        get
        {
            return (m_AID == 0);
        }
    }

    public DateTime regTime
    {
        set
        {
            if (m_RegTime != value)
            {
                m_RegTime = value;

                CancelInvoke();
                InvokeRepeating("UpdateTime", 0f, 1f);
            }
        }
    }

    #region RectTransform
    RectTransform m_RectTransform;

    public RectTransform rectTransform
    {
        get
        {
            if (!m_RectTransform)
            {
                m_RectTransform = transform as RectTransform;
            }

            return m_RectTransform;
        }
    }
    #endregion

    void Awake()
    {
        m_MinSize = m_ChatText.rectTransform.rect.size;
        m_MaxSize = m_ChatText.rectTransform.rect.size;
    }

    // Use this for initialization

    // Update is called once per frame

    void OnDisable()
    {
        CancelInvoke();
    }

    public CGuildChatting guildChatting
    {
        set
        {
            if (value != null)
            {
                m_Sequence = value.m_Sequence;
                m_AID = value.m_AID;

                string content = string.Empty;
                if (isSystem)
                {
                    // m_sUserName : userName, guildLevel 등 데이터 문자열
                    content = Languages.StringToTEXT_UI(value.m_sMsg, value.m_sUserName);
                }
                else
                {
                    content = value.m_sMsg;
                }

                m_ChatText.text = gameObject.name = content;

                FitSize();

                if (!isSystem)
                {
                    CGuildMember guildMember = Kernel.entry.guild.FindGuildMember(value.m_AID);
                    if (guildMember != null)
                    {
                        DB_Card.Schema card = DB_Card.Query(DB_Card.Field.Index, guildMember.m_iLeaderCardIndex);
                        if (card != null)
                        {
                            m_BackgroundImage.sprite = TextureManager.GetGradeTypeBackgroundSprite(card.Grade_Type);
                            m_FrameImage.sprite = TextureManager.GetGradeTypeFrameSprite(card.Grade_Type);
                        }

                        m_PortraitImage.sprite = TextureManager.GetPortraitSprite(guildMember.m_iLeaderCardIndex);
                    }

                    m_NameText.text = value.m_sUserName;
                    regTime = TimeUtility.ToDateTime(value.m_iRegTime);

                    float x = Mathf.Abs(m_ChatText.rectTransform.anchoredPosition.x) + m_ChatText.rectTransform.rect.width + m_Right;
                    float y = Mathf.Abs(m_ChatText.rectTransform.anchoredPosition.y) + m_ChatText.rectTransform.rect.height + m_Bottom;
                    m_BubbleRectTransform.sizeDelta = new Vector2(x, y);

                    Vector2 worldPosition = m_BubbleRectTransform.transform.TransformPoint(0f, m_BubbleRectTransform.rect.yMin, 0f);
                    Vector2 localPosition = rectTransform.InverseTransformPoint(worldPosition);
                    rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, Mathf.Abs(localPosition.y));
                }
            }
        }
    }

    void FitSize()
    {
        m_ChatText.rectTransform.sizeDelta = m_MaxSize; // Initialize size.
        m_ChatText.horizontalOverflow = (m_ChatText.preferredWidth > m_MaxSize.x) ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
        UIUtility.FitSizeToContent(m_ChatText);

        if (m_ChatText.rectTransform.sizeDelta.y < m_MinSize.y)
        {
            m_ChatText.rectTransform.sizeDelta = new Vector2(m_ChatText.rectTransform.sizeDelta.x,
                                                             m_MinSize.y);
        }
    }

    void UpdateTime()
    {
        // Called by InvokeRepeating().
        m_TimeText.text = Languages.TimeSpanToString(TimeUtility.currentServerTime - m_RegTime);
        m_TimeText.rectTransform.sizeDelta = new Vector2(m_TimeText.preferredWidth, m_TimeText.rectTransform.sizeDelta.y);
        m_TimeFrameImage.rectTransform.sizeDelta = new Vector2(Mathf.Abs(m_TimeText.rectTransform.anchoredPosition.x) +
                                                               m_TimeText.rectTransform.sizeDelta.x +
                                                               10f,
                                                               m_TimeFrameImage.rectTransform.sizeDelta.y);
    }
}
