using Common.Packet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildDonation : UIObject
{
    private TextLevelMaxEffect m_LevelMaxEffect;

    public ScrollRect m_ScrollRect;
    public GameObjectPool m_Pool;
    List<UIGuildDonationMemberObject> m_List = new List<UIGuildDonationMemberObject>();
    public Text m_DonationCountText;
    public List<UIGuildDonationButton> m_DonationButtonList;
    public Text m_GuildLevelText;
    public UISlider m_GuildExpSlider;

    // Use this for initialization

    // Update is called once per frame

    protected override void OnEnable()
    {
        base.OnEnable();

        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onDonationResult += OnDonationResult;
            Kernel.entry.guild.onGuildBaseUpdate += OnGuildBaseUpdate;

            Renewal();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onDonationResult -= OnDonationResult;
            Kernel.entry.guild.onGuildBaseUpdate -= OnGuildBaseUpdate;
        }
    }

    void OnGuildBaseUpdate(CGuildBase guildBase)
    {
        if (guildBase != null)
        {
            m_GuildLevelText.text = string.Format("{0}{1}", Languages.ToString(TEXT_UI.LV), guildBase.m_byGuildLevel);

            int maxExp = 0;
            DB_GuildLevel.Schema guildLevel = DB_GuildLevel.Query(DB_GuildLevel.Field.GulidLevel, guildBase.m_byGuildLevel);
            if (guildLevel != null)
            {
                maxExp = guildLevel.Max_Exp;
            }

            m_GuildExpSlider.maxValue = maxExp;
            m_GuildExpSlider.value = guildBase.m_GuildExp;

            if (m_GuildLevelText != null)
                m_LevelMaxEffect = m_GuildLevelText.GetComponent<TextLevelMaxEffect>();

            if (m_LevelMaxEffect != null)
                m_LevelMaxEffect.MaxValue = Kernel.entry.guild.MaxLevel;

            if (m_LevelMaxEffect != null)
                m_LevelMaxEffect.Value = guildBase.m_byGuildLevel;
        }
    }

    void OnDonationResult(byte guildLevel, long guildExp)
    {
        UINotificationCenter.Enqueue(Languages.ToString(TEXT_UI.DONATION_SUCCESS));
        Renewal();
    }

    public float m_Top;
    public float m_Bottom;
    public float m_Spacing;

    void BuildLayout()
    {
        float y = -m_Top;
        for (int i = 0; i < m_List.Count; i++)
        {
            UIGuildDonationMemberObject item = m_List[i];
            if (item)
            {
                y = y - (item.rectTransform.rect.height * .5f);
                item.rectTransform.anchoredPosition = new Vector2(0f, y);
                y = y - (item.rectTransform.rect.height * .5f) - m_Spacing;
            }
        }

        y = y + m_Spacing - m_Bottom;
        y = Mathf.Abs(y);
        m_ScrollRect.content.sizeDelta = new Vector2(m_ScrollRect.content.sizeDelta.x, y);
    }

    void Renewal(List<CGuildMember> guildMemberList)
    {
        for (int i = 0; i < m_List.Count; i++)
        {
            UIGuildDonationMemberObject item = m_List[i];
            if (item)
            {
                item.gameObject.SetActive(false);
                UIUtility.SetParent(item.transform, transform);
                m_Pool.Push(item.gameObject);
            }
        }
        m_List.Clear();

        if (guildMemberList != null && guildMemberList.Count > 0)
        {
            guildMemberList = guildMemberList.OrderByDescending(item => item.m_byDonationCount) // m_byDonationCount 내림차순 정렬
                                             .ThenBy(item => item.m_sUserName).ToList<CGuildMember>(); // m_sUserName 오름차순 정렬

            for (int i = 0; i < guildMemberList.Count; i++)
            {
                UIGuildDonationMemberObject item = m_Pool.Pop<UIGuildDonationMemberObject>();
                if (item)
                {
                    UIUtility.SetParent(item.transform, m_ScrollRect.content);
                    item.SetGuildMember(i + 1, guildMemberList[i]);
                    item.gameObject.SetActive(true);

                    m_List.Add(item);
                }
            }
        }

        BuildLayout();
    }

    void Renewal()
    {
        if (Kernel.entry == null)
        {
            return;
        }

        m_DonationCountText.text = string.Format("{0}/{1}", Kernel.entry.guild.donationCount, Kernel.entry.data.GetValue<int>(Const_IndexID.Const_Guild_Donate_Limit));
        OnGuildBaseUpdate(Kernel.entry.guild.guildBase);
        Renewal(Kernel.entry.guild.guildMemberList);
    }
}
