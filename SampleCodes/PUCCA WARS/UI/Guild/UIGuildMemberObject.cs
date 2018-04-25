using Common.Packet;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildMemberObject : MonoBehaviour
{
    private TextLevelMaxEffect m_LevelMaxEffect;

    public Button m_Button;
    public GameObject m_WaitingTagGameObject;
    public Image m_RankImage;
    public Text m_RankText;
    public Image m_BackgroundImage;
    public Image m_FrameImage;
    public Image m_PortraitImage;
    public Text m_LevelText;
    public Text m_NameText;
    public RectTransform m_FrameContainer;
    public Text m_LastConnectTimeText;
    public Text m_SupprtCardCountText;
    public Text m_RankingPointText;
    public Button m_ConfirmButton;
    public Button m_KickButton;
    public Image m_LineImage;
    public Sprite m_SelfSprite;

    long m_AID;
    int m_Rank;
    bool m_IsMember;
    DateTime m_LastConnectTime;

    public long aid
    {
        get
        {
            return m_AID;
        }
    }

    public bool isMember
    {
        get
        {
            return m_IsMember;
        }
    }

    DateTime lastConnectTime
    {
        set
        {
            if (m_LastConnectTime != value)
            {
                m_LastConnectTime = value;

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
                m_RectTransform = (RectTransform)transform;
            }

            return m_RectTransform;
        }
    }
    #endregion

    void Awake()
    {
        m_Button.onClick.AddListener(OnButtonClick);
        m_ConfirmButton.onClick.AddListener(OnConfirmButtonClick);
        m_KickButton.onClick.AddListener(OnKickButtonClick);
    }

    // Use this for initialization

    // Update is called once per frame

    void OnDisable()
    {
        // ref. PUC-643, 임시 주석 처리
        // UIScrollRectContentActivator와 충돌 예상
        //CancelInvoke();
    }

    public void SetGuildUserBase(CGuildUserBase guildUserBase, bool renewal = true)
    {
        if (guildUserBase != null)
        {
            m_AID = guildUserBase.m_AID;

            m_Button.image.overrideSprite = guildUserBase.m_AID.Equals(Kernel.entry.account.userNo) ? m_SelfSprite : null;

            DB_Card.Schema card = DB_Card.Query(DB_Card.Field.Index, guildUserBase.m_iLeaderCardIndex);
            if (card != null)
            {
                m_BackgroundImage.sprite = TextureManager.GetGradeTypeBackgroundSprite(card.Grade_Type);
                m_FrameImage.sprite = TextureManager.GetGradeTypeFrameSprite(card.Grade_Type);
                m_PortraitImage.sprite = TextureManager.GetPortraitSprite(guildUserBase.m_iLeaderCardIndex);
            }

            m_LevelText.text = string.Format("{0}{1}", Languages.ToString(TEXT_UI.LV), guildUserBase.m_byAccountLevel);
            m_NameText.text = guildUserBase.m_sUserName;
            lastConnectTime = TimeUtility.ToDateTime(guildUserBase.m_iLastConnectTime);
            m_IsMember = false;

            if (m_LevelText != null)
                m_LevelMaxEffect = m_LevelText.GetComponent<TextLevelMaxEffect>();

            if (m_LevelMaxEffect != null)
                m_LevelMaxEffect.MaxValue = Kernel.entry.data.GetValue<byte>(Const_IndexID.Const_Account_Level_Limit);

            if (m_LevelMaxEffect != null)
                m_LevelMaxEffect.Value = guildUserBase.m_byAccountLevel;

            if (renewal)
            {
                Renewal();
            }
        }
    }

    public void SetGuildMember(int ranking, CGuildMember guildMember)
    {
        if (guildMember != null)
        {
            this.m_Rank = ranking;

            SetGuildUserBase(guildMember, false);
            m_SupprtCardCountText.text = Languages.ToString<int>(guildMember.m_iSupportCardCount);
            m_RankingPointText.text = Languages.ToString<int>(guildMember.m_iRankingPoint);

            m_IsMember = true;
            Renewal();
        }
    }

    void Renewal()
    {
        m_WaitingTagGameObject.SetActive(!m_IsMember);

        bool highRank = (m_Rank <= 3);
        if (highRank)
        {
            m_RankImage.sprite = TextureManager.GetSprite(SpritePackingTag.Guild, string.Format("{0}st_Class", m_Rank));
        }
        else
        {
            m_RankText.text = m_Rank.ToString();
        }

        m_RankImage.gameObject.SetActive(highRank && m_IsMember);
        m_RankText.gameObject.SetActive(!highRank && m_IsMember);

        bool isLeader = Kernel.entry.guild.isLeader;
        bool isSelf = Kernel.entry.account.userNo.Equals(m_AID);

        m_ConfirmButton.gameObject.SetActive(!m_IsMember && isLeader);
        m_KickButton.gameObject.SetActive(isLeader && !isSelf);
        m_FrameContainer.gameObject.SetActive(m_IsMember);

        RectTransform rectTransform = (RectTransform)m_ConfirmButton.transform;
        float x = -19f;
        if (m_ConfirmButton.gameObject.activeSelf)
        {
            x = x - rectTransform.rect.width;
        }

        if (m_KickButton.gameObject.activeSelf)
        {
            x = x - 9f;
            rectTransform = (RectTransform)m_KickButton.transform;
            rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
            x = x - rectTransform.rect.width;
        }

        if (m_FrameContainer.gameObject.activeSelf)
        {
            x = (x < -19f) ? (x - 20f) : -10f;
            m_FrameContainer.anchoredPosition = new Vector2(x, m_FrameContainer.anchoredPosition.y);
            x = x - m_FrameContainer.rect.width;
        }

        x = this.rectTransform.rect.width - m_LineImage.rectTransform.anchoredPosition.x - Mathf.Abs(x) - 10f; // 10f : Margin.
        m_LineImage.rectTransform.sizeDelta = new Vector2(x, m_LineImage.rectTransform.sizeDelta.y);
    }

    void UpdateTime()
    {
        // Called by InvokeRepeating().
        m_LastConnectTimeText.text = string.Format("{0} {1}",
                                                   Languages.ToString(TEXT_UI.ACCESS_TIME),
                                                   Languages.TimeSpanToString(TimeUtility.currentServerTime - m_LastConnectTime));
    }

    void OnButtonClick()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.ranking.REQ_PACKET_CG_READ_DETAIL_USER_INFO_SYN(m_AID);
            //Kernel.entry.guild.REQ_PACKET_CG_GUILD_DETAIL_MEMBER_INFO_SYN(m_AID);
        }
    }

    void OnConfirmButtonClick()
    {
        if (Kernel.entry != null)
        {
            if (Kernel.entry.guild.memberCount < Kernel.entry.data.GetValue<int>(Const_IndexID.Const_Guild_Member_Limit))
            {
                Kernel.entry.guild.REQ_PACKET_CG_GUILD_DECISION_JOIN_REQUEST_SYN(m_AID, true);
            }
            else
            {
                NetworkEventHandler.OnNetworkException(Result_Define.eResult.MAX_GUILD_MEMBER);
            }
        }
    }

    void OnKickButtonClick()
    {
        if (Kernel.entry != null)
        {
            if (m_IsMember)
            {
                CGuildMember guildMember = Kernel.entry.guild.FindGuildMember(m_AID);
                if (guildMember != null)
                {
                    UIAlerter.Alert(Languages.ToString(TEXT_UI.FORCIBLY_INFO, guildMember.m_sUserName),
                                    UIAlerter.Composition.Confirm_Cancel,
                                    OnKickResponse,
                                    Languages.ToString(TEXT_UI.FORCIBLY));
                }
                else
                {
                    Debug.LogError(m_AID);
                }
            }
            else
            {
                CGuildUserBase approvalUser = Kernel.entry.guild.FindApprovalUser(m_AID);
                if (approvalUser != null)
                {
                    UIAlerter.Alert(//string.Format("{0} {1}", approvalUser.m_sUserName, Languages.ToString(TEXT_UI.JOIN_REFUSAL_INFO)),
                                    Languages.ToString(TEXT_UI.JOIN_REFUSAL_INFO, approvalUser.m_sUserName),
                                    UIAlerter.Composition.Confirm_Cancel,
                                    OnRejectResponse,
                                    Languages.ToString(TEXT_UI.JOIN_REFUSAL));
                }
                else
                {
                    Debug.LogError(m_AID);
                }
            }
        }
    }

    void OnKickResponse(UIAlerter.Response response, params object[] args)
    {
        if (response != UIAlerter.Response.Confirm)
        {
            return;
        }

        Kernel.entry.guild.REQ_PACKET_CG_GUILD_KICK_OUT_SYN(m_AID);
    }

    void OnRejectResponse(UIAlerter.Response response, params object[] args)
    {
        if (response != UIAlerter.Response.Confirm)
        {
            return;
        }

        Kernel.entry.guild.REQ_PACKET_CG_GUILD_DECISION_JOIN_REQUEST_SYN(m_AID, false);
    }
}
