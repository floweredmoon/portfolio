using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Common.Packet;

public class UIGuildInfo : UIObject
{
    public UIGuildFlag m_GuildFlag;
    public Text m_GuildNameText;
    public Text m_GuildLeaderNameText;
    public Text m_GuildMemberCountText;
    public Text m_GuildTotalSupportedCardCountText;
    public Text m_GuildRankingPointText;
    public Text m_GuildIntroduceText;

    // Use this for initialization

    // Update is called once per frame

    public void SetGuildInfo(CGuildBase guildBase)
    {
        if (guildBase != null)
        {
            m_GuildFlag.SetGuildEmblem(guildBase.m_sGuildEmblem);
            m_GuildFlag.guildLevel = guildBase.m_byGuildLevel;
            m_GuildNameText.text = guildBase.m_sGuildName;
            m_GuildLeaderNameText.text = guildBase.m_sGuildHeadName;
            m_GuildMemberCountText.text = string.Format("{0}/{1}", guildBase.m_iMemberCount, Kernel.entry.data.GetValue<int>(Const_IndexID.Const_Guild_Member_Limit));
            m_GuildTotalSupportedCardCountText.text = Languages.ToString(guildBase.m_iTotalSupportedCardCount);
            m_GuildRankingPointText.text = Languages.ToString(guildBase.m_iGuildRankingPoint);
            m_GuildIntroduceText.text = guildBase.m_sGuildIntroduce;
        }
    }
}
