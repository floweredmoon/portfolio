using Common.Packet;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIGuildWaitingList : UIGuildList
{
    public Text m_CapacityText;

    // Use this for initialization

    // Update is called once per frame

    protected override void OnEnable()
    {
        base.OnEnable();

        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onJoinResult += OnJoinResult;
            Kernel.entry.guild.onJoinRequestCancelResult += OnJoinRequestCancelResult;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onJoinResult -= OnJoinResult;
            Kernel.entry.guild.onJoinRequestCancelResult -= OnJoinRequestCancelResult;
        }
    }

    protected override void OnGuildListUpdate(List<CGuildBase> recommendGuildList, List<CGuildBase> waitingApprovalList)
    {
        base.OnGuildListUpdate(recommendGuildList, waitingApprovalList);

        m_CapacityText.text = string.Format("{0}/{1}", waitingApprovalList.Count, Kernel.entry.data.GetValue<int>(Const_IndexID.Const_Guild_Join_Apply_Limit));

        isEmpty = (waitingApprovalList == null) || (waitingApprovalList.Count == 0);
        if (!isEmpty)
        {
            for (int i = 0; i < waitingApprovalList.Count; i++)
            {
                UIGuildWaitingObject item = Pop<UIGuildWaitingObject>();
                if (item)
                {
                    item.SetGuildBase(waitingApprovalList[i]);
                }
            }
        }

        BuildLayout();
    }

    void OnJoinResult(long gid, string guildName, bool isJoin)
    {
        if (isJoin)
        {
            return;
        }

        // 전체 목록 갱신이 아닌, 해당 오브젝트만 찾아서 관리하도록 수정해야 합니다.
        OnGuildListUpdate(null, Kernel.entry.guild.waitingApprovalList);
    }

    void OnJoinRequestCancelResult(long aid)
    {
        // 전체 목록 갱신이 아닌, 해당 오브젝트만 찾아서 관리하도록 수정해야 합니다.
        OnGuildListUpdate(null, Kernel.entry.guild.waitingApprovalList);
    }
}
