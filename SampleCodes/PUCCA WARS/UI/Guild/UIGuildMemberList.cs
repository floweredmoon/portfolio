using Common.Packet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildMemberList : UIObject
{
    public Text m_MemberCountText;
    public Text m_ApprovalText;
    public Button m_JoinTypeButton;
    public Text m_JoinTypeButtonText;
    public Button m_LeaveButton;
    public Text m_LeaveButtonText;
    public ScrollRect m_ScrollRect;
    public GameObjectPool m_GameObjectPool;
    public Image m_LeaderIconImage;

    List<UIGuildMemberObject> m_GuildMemberObjectList = new List<UIGuildMemberObject>();

    protected override void Awake()
    {
        m_JoinTypeButton.onClick.AddListener(OnJoinTypeButtonClick);
        m_LeaveButton.onClick.AddListener(OnLeaveButtonClick);
    }

    // Use this for initialization

    // Update is called once per frame

    protected override void OnEnable()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onJoinTypeChange += OnJoinTypeChange;
            Kernel.entry.guild.onGuildMemberListUpdate += OnGuildMemberListUpdate;
            Kernel.entry.guild.onApprovalUserListUpdate += OnApprovalUserListUpdate;
            //Kernel.entry.guild.onGuildMemberInfoResult += OnGuildMemberDetailInfoUpdate;
            Kernel.entry.guild.onKickResult += OnKickResult;

            Renewal();
        }

        if (Kernel.networkManager)
        {
            Kernel.networkManager.onException += OnException;
        }

        if (UIHUD.instance)
        {
            UIHUD.instance.onBackButtonClicked = OnBackButtonClicked;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onJoinTypeChange -= OnJoinTypeChange;
            Kernel.entry.guild.onGuildMemberListUpdate -= OnGuildMemberListUpdate;
            Kernel.entry.guild.onApprovalUserListUpdate -= OnApprovalUserListUpdate;
            //Kernel.entry.guild.onGuildMemberInfoResult -= OnGuildMemberDetailInfoUpdate;
            Kernel.entry.guild.onKickResult -= OnKickResult;
        }

        if (Kernel.networkManager)
        {
            Kernel.networkManager.onException -= OnException;
        }
    }

    void Renewal()
    {
        if (Kernel.entry == null)
        {
            return;
        }

        bool isLeader = Kernel.entry.guild.isLeader;

        //m_MemberCountText.text = string.Format("{0}/{1}", Kernel.entry.guild.guildBase.m_iMemberCount, Settings.Guild.MaximumMemberCount);
        //m_ApprovalText.gameObject.SetActive(isLeader);
        //m_JoinTypeButton.gameObject.SetActive(isLeader);
        m_JoinTypeButton.interactable = isLeader;
        SoundDataInfo.FindSoundUtility(m_JoinTypeButton.gameObject).enabled = isLeader;
        OnJoinTypeChange(Kernel.entry.guild.guildBase.m_bIsFreeJoin);
        m_LeaveButtonText.text = Languages.ToString(isLeader ? TEXT_UI.GUILD_BREAKUP : TEXT_UI.GUILD_WITHDRAW);

        OnApprovalUserListUpdate(Kernel.entry.guild.approvalUserList);
        OnGuildMemberListUpdate(Kernel.entry.guild.guildMemberList);
    }

    void Clear(bool isMember)
    {
        for (int i = 0; i < m_GuildMemberObjectList.Count; i++)
        {
            UIGuildMemberObject guildMemberObject = m_GuildMemberObjectList[i];
            if (isMember.Equals(guildMemberObject.isMember))
            {
                Push(guildMemberObject);
                i--;
            }
        }
    }

    void BuildLayout()
    {
        // 5f is spacing.
        float y = -5f;
        for (int i = 0; i < m_GuildMemberObjectList.Count; i++)
        {
            RectTransform rectTransform = m_GuildMemberObjectList[i].rectTransform;
            if (rectTransform)
            {
                rectTransform.anchoredPosition = new Vector2(0f, y);

                y = y - rectTransform.rect.height - 5f;
            }
        }

        m_ScrollRect.content.sizeDelta = new Vector2(m_ScrollRect.content.sizeDelta.x, Mathf.Abs(y));
    }

    void OnKickResult(long aid)
    {
        UIGuildMemberObject guildMemberObject = m_GuildMemberObjectList.Find(item => long.Equals(item.aid, aid));
        if (guildMemberObject)
        {
            Push(guildMemberObject);
            BuildLayout();
        }
    }
    /*
    void OnGuildMemberDetailInfoUpdate(long aid,
                                       CGuildBase guildBase,
                                       CGuildMember guildMember,
                                       CFranchiseRankingInfo franchiseRankingInfo,
                                       List<CCardInfo> cardInfoList)
    {
        UIUserInfo userInfo = Kernel.uiManager.Get<UIUserInfo>(UI.UserInfo, true, false);
        if (userInfo != null)
        {
            userInfo.SetGuildMember(guildBase, guildMember, franchiseRankingInfo, cardInfoList);
            Kernel.uiManager.Open(UI.UserInfo);
        }
    }
    */
    void OnGuildMemberListUpdate(List<CGuildMember> guildMemberList)
    {
        m_MemberCountText.text = string.Format("{0}/{1}", Kernel.entry.guild.memberCount, Kernel.entry.data.GetValue<int>(Const_IndexID.Const_Guild_Member_Limit));

        Clear(true);
        m_LeaderIconImage.gameObject.SetActive(false);

        if (guildMemberList != null && guildMemberList.Count > 0)
        {
            guildMemberList = guildMemberList.OrderByDescending(item => item.m_iRankingPoint) // m_iRankingPoint 내림차순 정렬
                                              .ThenByDescending(item => item.m_iSupportCardCount) // m_iSupportCardCount 내림차순 정렬
                                              .ThenBy(item => item.m_sUserName).ToList<CGuildMember>(); // m_sUserName 오름차순 정렬

            for (int i = 0; i < guildMemberList.Count; i++)
            {
                UIGuildMemberObject guildMemberObject = Pop(true);
                if (guildMemberObject)
                {
                    guildMemberObject.SetGuildMember(i + 1, guildMemberList[i]);

                    if (guildMemberList[i].m_sUserName.Equals(Kernel.entry.guild.guildHeadName))
                    {
                        UIUtility.SetParent(m_LeaderIconImage.transform, guildMemberObject.m_BackgroundImage.transform);
                        m_LeaderIconImage.gameObject.SetActive(true);
                    }
                }
            }
        }

        BuildLayout();
    }

    void OnApprovalUserListUpdate(List<CGuildUserBase> approvalUserList)
    {
        // isLeader

        Clear(false);

        if (approvalUserList != null && approvalUserList.Count > 0)
        {
            for (int i = 0; i < approvalUserList.Count; i++)
            {
                UIGuildMemberObject guildMemberObject = Pop(false);
                if (guildMemberObject)
                {
                    guildMemberObject.SetGuildUserBase(approvalUserList[i]);
                }
            }
        }

        BuildLayout();
    }

    void OnJoinTypeChange(bool isFreeJoin)
    {
        m_JoinTypeButton.image.overrideSprite = isFreeJoin ? null : TextureManager.GetSprite(SpritePackingTag.Extras, "ui_button_07");
        m_JoinTypeButtonText.text = Languages.ToString(isFreeJoin ? TEXT_UI.FREE : TEXT_UI.APPROVAL);

        Shadow[] comps = m_JoinTypeButtonText.GetComponentsInChildren<Shadow>(true);
        for (int i = 0; i < comps.Length; i++)
        {
            Shadow comp = comps[i];
            Color effectColor = Color.clear;
            if (comp is Shadow)
            {
                Kernel.colorManager.TryGetColor(string.Format("button{0:00}_shadow", isFreeJoin ? 2 : 7), out effectColor);
            }
            else if (comp is Outline)
            {
                Kernel.colorManager.TryGetColor(string.Format("button{0:00}_outline", isFreeJoin ? 2 : 7), out effectColor);
            }

            comp.effectColor = effectColor;
        }
    }

    void OnException(Result_Define.eResult result, string error, ePACKET_CATEGORY category, byte index)
    {
        if (ePACKET_CATEGORY.CG_GUILD == category &&
            eCG_GUILD.CHANGE_GUILD_JOIN_TYPE_ACK == (eCG_GUILD)index)
        {
            OnJoinTypeChange(Kernel.entry.guild.isFreeJoin);
        }
    }

    bool OnBackButtonClicked()
    {
        Kernel.uiManager.Close(UI.GuildMemberList);

        return true;
    }

    UIGuildMemberObject Pop(bool isMember)
    {
        UIGuildMemberObject guildMemberObject = m_GameObjectPool.Pop<UIGuildMemberObject>();
        if (guildMemberObject)
        {
            guildMemberObject.gameObject.SetActive(true);
            UIUtility.SetParent(guildMemberObject.transform, m_ScrollRect.content);

            if (isMember)
            {
                m_GuildMemberObjectList.Add(guildMemberObject);
            }
            else
            {
                m_GuildMemberObjectList.Insert(0, guildMemberObject);
            }

            return guildMemberObject;
        }

        return null;
    }

    void Push(UIGuildMemberObject guildMemberObject)
    {
        if (guildMemberObject)
        {
            guildMemberObject.gameObject.SetActive(false);
            UIUtility.SetParent(guildMemberObject.transform, m_GameObjectPool.transform);
            m_GameObjectPool.Push(guildMemberObject.gameObject);
            m_GuildMemberObjectList.Remove(guildMemberObject);
        }
    }

    void OnJoinTypeButtonClick()
    {
        if (Kernel.entry == null)
        {
            return;
        }

        if (!Kernel.entry.guild.isFreeJoin)
        {
            UIAlerter.Alert(Languages.ToString(TEXT_UI.JOIN_WAY_CHANGE_INFO),
                            UIAlerter.Composition.Confirm_Cancel,
                            OnResponseCallback,
                            Languages.ToString(TEXT_UI.JOIN_WAY_CHANGE));
        }
        else
        {
            OnResponseCallback(UIAlerter.Response.Confirm);
        }
    }

    void OnResponseCallback(UIAlerter.Response response, params object[] args)
    {
        if (response != UIAlerter.Response.Confirm)
        {
            return;
        }

        Kernel.entry.guild.REQ_PACKET_CG_GUILD_CHANGE_GUILD_JOIN_TYPE_SYN();
    }

    void OnLeaveButtonClick()
    {
        if (Kernel.entry != null)
        {
            if (Kernel.entry.guild.isLeader)
            {
                SoundDataInfo.RevertSound(m_LeaveButton.gameObject);

                // 1 : Except self.
                if (Kernel.entry.guild.memberCount > 1)
                {
                    SoundDataInfo.ChangeUISound(UISOUND.UIS_CANCEL_01, m_LeaveButton.gameObject);
                    NetworkEventHandler.OnNetworkException(Result_Define.eResult.DO_NOT_DISBAND_BECAUSE_MEMBER_IS_NOT_EMPTY);
                }
                else
                {
                    UIAlerter.Alert(Languages.ToString(TEXT_UI.GUILD_BREAKUP_INFO),
                                    UIAlerter.Composition.Confirm_Cancel,
                                    OnDisbandResponse,
                                    Languages.ToString(TEXT_UI.GUILD_BREAKUP));
                }
            }
            else
            {
                UIAlerter.Alert(Languages.ToString(TEXT_UI.GUILD_WITHDRAW_INFO),
                                    UIAlerter.Composition.Confirm_Cancel,
                                    OnLeaveResponse,
                                    Languages.ToString(TEXT_UI.GUILD_WITHDRAW));

            }
        }
    }

    void OnDisbandResponse(UIAlerter.Response response, params object[] args)
    {
        if (response != UIAlerter.Response.Confirm)
        {
            return;
        }

        Kernel.entry.guild.REQ_PACKET_CG_GUILD_DISBAND_GUILD_SYN();
    }

    void OnLeaveResponse(UIAlerter.Response response, params object[] args)
    {
        if (response != UIAlerter.Response.Confirm)
        {
            return;
        }

        Kernel.entry.guild.REQ_PACKET_CG_GUILD_LEAVE_GUILD_SYN();
    }
}
