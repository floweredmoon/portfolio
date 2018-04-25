using Common.Packet;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildRecommendObject : UIGuildObject
{
    public Button m_JoinButton;
    public Image m_GreetingFrameImage;
    public Text m_GreetingText;
    public Text m_FreeText;
    public Text m_InviteText;

    void Awake()
    {
        m_JoinButton.onClick.AddListener(OnJoinButtonClick);
    }

    // Use this for initialization

    // Update is called once per frame

    public override void SetGuildBase(CGuildBase guildBase)
    {
        if (guildBase != null)
        {
            base.SetGuildBase(guildBase);

            float width = m_GreetingText.rectTransform.rect.width - (guildBase.m_bIsFreeJoin ? m_FreeText.rectTransform.rect.width : m_InviteText.rectTransform.rect.width);

            m_FreeText.gameObject.SetActive(guildBase.m_bIsFreeJoin);
            m_InviteText.gameObject.SetActive(!guildBase.m_bIsFreeJoin);
            m_GreetingText.rectTransform.sizeDelta = new Vector2(width, m_GreetingText.rectTransform.sizeDelta.y);
            UIUtility.EllipsisSingleLine(m_GreetingText, guildBase.m_sGuildIntroduce);
        }
    }

    void OnJoinButtonClick()
    {
        if (Kernel.entry == null)
        {
            return;
        }

        if (Kernel.entry.guild.waitingApprovalCount >= Kernel.entry.data.GetValue<int>(Const_IndexID.Const_Guild_Join_Apply_Limit))
        {
            NetworkEventHandler.OnNetworkException(Result_Define.eResult.MAX_APPROVAL_LIST);
        }
        else
        {
            CGuildBase guildBase = Kernel.entry.guild.FindRecommendGuild(m_GID);
            if (guildBase != null)
            {
                if (guildBase.m_iMemberCount >= Kernel.entry.data.GetValue<int>(Const_IndexID.Const_Guild_Member_Limit))
                {
                    NetworkEventHandler.OnNetworkException(Result_Define.eResult.MAX_GUILD_MEMBER);
                }
                else
                {
                    Kernel.entry.guild.REQ_PACKET_CG_GUILD_JOIN_GUILD_SYN(m_GID);
                }
            }
        }
    }
}
