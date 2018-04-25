using Common.Packet;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIGuild : UIObject
{
    private TextLevelMaxEffect m_LevelMaxEffect;
    public Text m_NameText;
    public UIGuildFlag m_GuildFlag;
    public UISlider m_ExpSlider;
    public Text m_LevelText;
    public Text m_LeaderNameText;
    public Text m_RankText1; // 아, 귀찮아
    public Text m_RankText2; // 아, 귀찮아
    public Text m_MemberCountText;
    public Text m_TotalSupportCardCountText;
    public Text m_RankingPointText;
    public UIInputField m_GreetingInputField;
    public Button m_IntroduceEditButton;
    public Button m_MemberButton;
    public Button m_ShopButton;
    public Toggle m_Toggle;
    public Image m_SupportCardIconImage;
    public Image m_ChatIconIamge;
    public Image m_NewIconImage;
    public UIGuildChat m_GuildChat;
    public UIGuildSupportList m_GuildSupportList;

    protected override void Awake()
    {
        base.Awake();

        m_IntroduceEditButton.onClick.AddListener(OnIntroduceEditButtonClick);
        m_MemberButton.onClick.AddListener(OnMemberButtonClick);
        m_ShopButton.onClick.AddListener(OnShopButtonClick);
        m_Toggle.onValueChanged.AddListener(OnToggleValueChanged);
        m_GreetingInputField.interactable = false;

        if (Kernel.entry != null)
        {
            m_GreetingInputField.characterLimit = Kernel.entry.data.GetValue<int>(Const_IndexID.Const_Guild_Introduce_Length_Limit);
            m_GreetingInputField.lineLimit = 3;
        }

        if (m_LevelText != null && m_LevelMaxEffect == null)
            m_LevelMaxEffect = m_LevelText.GetComponent<TextLevelMaxEffect>();
    }

    // Use this for initialization

    // Update is called once per frame

    protected override void OnEnable()
    {
        base.OnEnable();

        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onGuildBaseUpdate += SetGuildBase;
            Kernel.entry.guild.onGuildDisbandResult += OnGuildDisbandResult;
            Kernel.entry.guild.onGuildMemberListUpdate += OnGuildMemberListUpdate;

            SetGuildBase(Kernel.entry.guild.guildBase);
        }

        if (Kernel.packetRequestIterator)
        {
            Kernel.packetRequestIterator.AddPacketRequestInfo<PACKET_CG_GUILD_REFRESH_CHATTING_LIST_SYN>(4f, Kernel.entry.guild.PACKET_CG_GUILD_REFRESH_CHATTING_LIST_SYN);
        }

        OnToggleValueChanged(m_Toggle.isOn);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onGuildBaseUpdate -= SetGuildBase;
            Kernel.entry.guild.onGuildDisbandResult -= OnGuildDisbandResult;
            Kernel.entry.guild.onGuildMemberListUpdate -= OnGuildMemberListUpdate;
        }

        if (Kernel.packetRequestIterator)
        {
            Kernel.packetRequestIterator.RemovePacketRequestInfo<PACKET_CG_GUILD_REFRESH_CHATTING_LIST_SYN>();
        }
    }

    void OnGuildMemberListUpdate(List<CGuildMember> guildMemberList)
    {
        if (Kernel.uiManager)
        {
            Kernel.uiManager.Open(UI.GuildMemberList);
        }
    }

    void OnGuildIntroduceUpdate(string guildIntroduce)
    {
        m_GreetingInputField.text = guildIntroduce;
    }

    void OnGuildLevelUp(byte guildLevel)
    {
        DB_GuildLevel.Schema table = DB_GuildLevel.Query(DB_GuildLevel.Field.GulidLevel, guildLevel);
        if (table != null)
        {
            m_ExpSlider.maxValue = table.Max_Exp;
        }
    }

    void OnGuildExpUpdate(long guildExp)
    {
        m_ExpSlider.value = guildExp;
    }

    void SetGuildBase(CGuildBase guildBase)
    {
        if (guildBase != null)
        {
            m_NameText.text = guildBase.m_sGuildName;
            m_LevelText.text = guildBase.m_byGuildLevel.ToString();
            m_LeaderNameText.text = guildBase.m_sGuildHeadName;
            long guildRanking = Kernel.entry.guild.guildRanking;
            long totalRankedGuildCount = Kernel.entry.guild.totalRankedGuildCount;
            float ratio = (totalRankedGuildCount > 0) ? ((float)guildRanking / (float)totalRankedGuildCount) * 100f : 0f;

            m_RankText1.text = guildRanking.ToString();
            UIUtility.FitSizeToContent(m_RankText1);
            m_RankText2.text = string.Format("{0} ({1:F0}%)", Languages.Ordinal(guildRanking), ratio);
            UIUtility.FitSizeToContent(m_RankText2);
            float x = -(m_RankText2.rectTransform.anchoredPosition.x + (m_RankText2.rectTransform.sizeDelta.x * .5f));
            m_RankText1.rectTransform.anchoredPosition = new Vector2(x, m_RankText1.rectTransform.anchoredPosition.y);

            m_MemberCountText.text = string.Format("{0}/{1}", Languages.ToString<int>(guildBase.m_iMemberCount), Kernel.entry.data.GetValue<int>(Const_IndexID.Const_Guild_Member_Limit));
            m_TotalSupportCardCountText.text = Languages.ToString<int>(guildBase.m_iTotalSupportedCardCount);
            m_RankingPointText.text = Languages.ToString<int>(guildBase.m_iGuildRankingPoint);

            if (m_LevelMaxEffect != null)
                m_LevelMaxEffect.MaxValue = Kernel.entry.guild.MaxLevel;

            if (m_LevelMaxEffect != null)
                m_LevelMaxEffect.Value = guildBase.m_byGuildLevel;

            OnGuildIntroduceUpdate(guildBase.m_sGuildIntroduce);
            m_IntroduceEditButton.gameObject.SetActive(Kernel.entry.guild.isLeader);
            //m_GreetingInputField.interactable = Kernel.entry.guild.isLeader;
            OnGuildLevelUp(guildBase.m_byGuildLevel);
            OnGuildExpUpdate(guildBase.m_GuildExp);
            m_GuildFlag.SetGuildEmblem(guildBase.m_sGuildEmblem);
        }
    }

    void OnGuildDisbandResult()
    {
        Kernel.sceneManager.LoadScene(Scene.Lobby);
    }

    void OnIntroduceEditButtonClick()
    {
        if (Kernel.uiManager)
        {
            Kernel.uiManager.Open(UI.GuildIntroduceEdit);
        }
    }

    void OnMemberButtonClick()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.guild.REQ_PACKET_CG_GUILD_MEMBER_LIST_SYN();
        }
    }

    void OnShopButtonClick()
    {
        if (Kernel.uiManager)
        {
            Kernel.uiManager.Open(UI.GuildShop);
        }
    }

    void OnToggleValueChanged(bool value)
    {
        // true : UIGuildChat, false : UIGuildSupportList
        m_ChatIconIamge.gameObject.SetActive(!value);
        m_GuildChat.gameObject.SetActive(value);
        m_GuildChat.active = value;
        m_SupportCardIconImage.gameObject.SetActive(value);
        m_GuildSupportList.gameObject.SetActive(!value);
        m_GuildSupportList.active = !value;
        // Kernel.entry.guild.cardSupportable is deprecated.
        m_NewIconImage.gameObject.SetActive(value ? false /*Kernel.entry.guild.cardSupportable*/ : Kernel.entry.guild.isNewChat);
    }
}
