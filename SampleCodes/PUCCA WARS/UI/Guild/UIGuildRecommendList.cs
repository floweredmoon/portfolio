using Common.Packet;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildRecommendList : UIGuildList
{
    public InputField m_SearchInputField;
    public Button m_SearchButton;
    public Button m_RefreshButton;
    public UIGuildRecommendObject m_SearchResult;

    bool search
    {
        set
        {
            //m_SearchInputField.text = string.Empty;
            m_SearchResult.gameObject.SetActive(value);
            m_ScrollRect.gameObject.SetActive(!value);
        }
    }

    protected override void Awake()
    {
        m_ScrollRectContentActivator.Add((RectTransform)m_SearchResult.transform);
        m_SearchButton.onClick.AddListener(OnSearchButtonClick);
        m_RefreshButton.onClick.AddListener(OnRefreshButtonClick);

        if (Kernel.entry != null)
        {
            m_SearchInputField.characterLimit = Kernel.entry.data.GetValue<int>(Const_IndexID.Const_Guild_Name_Length_Limit);
        }
    }

    // Use this for initialization

    // Update is called once per frame

    protected override void OnEnable()
    {
        base.OnEnable();

        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onJoinResult += OnJoinResult;
            Kernel.entry.guild.onSearchResult += OnSearchResult;
        }

        search = false;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onJoinResult -= OnJoinResult;
            Kernel.entry.guild.onSearchResult -= OnSearchResult;
        }
    }

    protected override void OnGuildListUpdate(List<CGuildBase> recommendGuildList, List<CGuildBase> waitingApprovalList)
    {
        base.OnGuildListUpdate(recommendGuildList, waitingApprovalList);

        search = false;
        isEmpty = (recommendGuildList == null) || (recommendGuildList.Count == 0);
        if (!isEmpty)
        {
            for (int i = 0; i < recommendGuildList.Count; i++)
            {
                UIGuildRecommendObject item = Pop<UIGuildRecommendObject>();
                if (item)
                {
                    item.SetGuildBase(recommendGuildList[i]);
                }
            }
        }

        BuildLayout();
    }

    void OnJoinResult(long gid, string guildName, bool isJoin)
    {
        if (!isJoin)
        {
            UINotificationCenter.Enqueue(Languages.ToString(TEXT_UI.GUILD_JOIN_MSG, guildName));
            OnGuildListUpdate(Kernel.entry.guild.recommendGuildList, null);
        }
    }

    void OnSearchResult(CGuildBase guildBase)
    {
        if (guildBase != null)
        {
            m_SearchResult.SetGuildBase(guildBase);
            search = true;
        }
    }

    void OnSearchButtonClick()
    {
        if (Kernel.entry == null)
        {
            return;
        }

        string guildName = m_SearchInputField.text;
        if (string.IsNullOrEmpty(name))
        {
            NetworkEventHandler.OnNetworkException(Result_Define.eResult.EMPTY_SEARCH_GUILD_NAME);
        }
        else
        {
            Kernel.entry.guild.REQ_PACKET_CG_GUILD_SEARCH_GUILD_SYN(guildName);
        }
    }

    void OnRefreshButtonClick()
    {
        m_SearchInputField.text = string.Empty;

        if (Kernel.entry != null)
        {
            Kernel.entry.guild.REQ_PACKET_CG_GUILD_RECOMMEND_LIST_SYN();
        }
    }
}
