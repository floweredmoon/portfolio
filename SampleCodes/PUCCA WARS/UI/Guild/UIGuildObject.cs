using Common.Packet;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildObject : MonoBehaviour
{
    public UIGuildFlag m_GuildFlag;
    public Text m_LevelText;
    public Text m_NameText;
    public Text m_LeaderNameText;
    public Text m_RankingPointText;
    public Text m_CapacityText;

    protected long m_GID;

    // Use this for initialization

    // Update is called once per frame
    /*
    void OnDisable()
    {
        m_GID = 0;
    }
    */
    public virtual void SetGuildBase(CGuildBase guildBase)
    {
        if (guildBase != null)
        {
            m_GID = guildBase.m_Gid;

            m_GuildFlag.SetGuildEmblem(guildBase.m_sGuildEmblem);
            m_LevelText.text = string.Format("{0}{1}", Languages.ToString(TEXT_UI.LV), guildBase.m_byGuildLevel);
            m_NameText.text = guildBase.m_sGuildName;
            m_LeaderNameText.text = guildBase.m_sGuildHeadName;
            m_RankingPointText.text = Languages.ToString<int>(guildBase.m_iGuildRankingPoint);
            m_CapacityText.text = string.Format("{0}/{1}", guildBase.m_iMemberCount, Kernel.entry.data.GetValue<int>(Const_IndexID.Const_Guild_Member_Limit));
        }
    }
}
