using Delta.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Table;
using UnityEngine;

public class ContestDataManager : kSingletonPersistent<ContestDataManager>
{
    #region Fields

    private List<ContestKey> keyList = new List<ContestKey>();
    private List<ContestInfo> infoList = new List<ContestInfo>();
    private List<delta_T_Contest.Data> dataList = new List<delta_T_Contest.Data>();

    #endregion

    // Use this for initialization

    // Update is called once per frame

    public void RequestContestKey(Action<List<ContestKey>> callback)
    {
        // #1277
        StartCoroutine(RequestContestKeyList(callback));
    }

    private IEnumerator RequestContestKeyList(Action<List<ContestKey>> callback)
    {
        var protocol = new ServerRequest<FinishedContestResultReq, FinishedContestResultRes>()
        {
            openErrorMsgBoxWhenReqFailed = true,
        };

        yield return protocol.Send();

        if (protocol.IsSuccess())
        {
            keyList.Clear();
            if (protocol.res.List != null && protocol.res.List.Length > 0)
            {
                foreach (var key in protocol.res.List)
                {
                    if (key != null && !key.ParticipableNow())
                    {
                        keyList.Add(key);
                    }

                    yield return null;
                }
            }
            // DateTime 내림차순 정렬
            keyList.Sort(CompareByDateTime);
        }

        if (infoList != null && infoList.Count > 0)
        {
            infoList.RemoveAll(NotExists);
        }

        callback.InvokeWithNullCheck(keyList);
    }

    private ContestKey GetLast()
    {
        ContestKey lastKey = null;
        if (keyList != null && keyList.Count > 0)
        {
            lastKey = keyList[0];
        }

        return lastKey;
    }

    public void RequestContestInfo(ContestKey key, Action<ContestInfo> callback)
    {
        if (key == null)
        {
            callback.InvokeWithNullCheck(null);

            return;
        }

        ContestInfo info = null;
        if (infoList != null && infoList.Count > 0)
        {
            info = infoList.Find(item => Equals(item.key, key));
        }
        if (info == null)
        {
            info = new ContestInfo(key);
            infoList.Add(info);
        }

        // #1231
        if (info != null)
        {
            StartCoroutine(info.RequestContestInfo(callback));
        }
    }

    private int CompareByDateTime(ContestKey lhs, ContestKey rhs)
    {
        int result = 0;
        if (lhs != null && rhs != null)
        {
            var lhsDateTime = new DateTime(lhs.Year, lhs.Month, lhs.Day);
            var rhsDateTime = new DateTime(rhs.Year, rhs.Month, rhs.Day);
            var lhsData = TableManager.Instance.contest.GetData((uint)lhs.tiIndex);
            var rhsData = TableManager.Instance.contest.GetData((uint)rhs.tiIndex);
            if (lhsData != null && rhsData != null)
            {
                lhsDateTime = lhsDateTime + lhsData.EndTime.ToTimeSpan();
                rhsDateTime = rhsDateTime + rhsData.EndTime.ToTimeSpan();
            }

            result = (lhsDateTime > rhsDateTime) ? -1 : 1;
        }

        return result;
    }

    private bool NotExists(ContestInfo info)
    {
        bool exists = false;
        if (info != null)
        {
            exists = keyList.Exists(match => Equals(match, info.key));
        }

        return !exists;
    }

    private bool Equals(ContestKey lhs, ContestKey rhs)
    {
        bool equals = false;
        if (lhs != null && rhs != null)
        {
            equals = (lhs.ClassOfLevel == rhs.ClassOfLevel) && (lhs.Day == rhs.Day) && (lhs.Month == rhs.Month) && (lhs.tiIndex == rhs.tiIndex) && (lhs.Year == rhs.Year);
        }

        return equals;
    }
}

public class ContestInfo
{

    #region Constructor

    public ContestInfo(ContestKey key)
    {
        this.key = key;
        rankInfoList = new List<ContestRankInfo>();
        requestable = true;
    }

    #endregion

    #region Properties

    public ContestKey key
    {
        get;
        private set;
    }

    public List<ContestRankInfo> rankInfoList
    {
        get;
        private set;
    }

    public ContestRankInfo myRank
    {
        get;
        set;
    }

    public int participants
    {
        get;
        set;
    }

    public bool isEmpty
    {
        get
        {
            return (rankInfoList == null || rankInfoList.Count <= 0);
        }
    }

    public int startIndex
    {
        get
        {
            if (rankInfoList != null && rankInfoList.Count > 0)
            {
                return (rankInfoList.Count + 1);
            }

            return 1;
        }
    }

    public int endIndex
    {
        get
        {
            return (startIndex + 9);
        }
    }

    public bool requestable
    {
        get;
        private set;
    }

    public int lastIndex
    {
        get;
        private set;
    }

    #endregion

    public IEnumerator RequestContestInfo(Action<ContestInfo> callback)
    {
        // #1231
        lastIndex = (rankInfoList != null) ? rankInfoList.Count : 0;

        if (!requestable)
        {
            callback.InvokeWithNullCheck(this);

            yield break;
        }

        requestable = false;

        var protocol = new ServerRequest<FinishedContestResultDetailReq, FinishedContestResultDetailRes>()
        {
            showLoadingCircle = false,
            openErrorMsgBoxWhenReqFailed = true,
        };
        protocol.req.key = key;
        protocol.req.StartRank = startIndex;
        protocol.req.EndRank = endIndex;
        protocol.req.IncludeMyRank = (isEmpty ? 1 : 0);
        protocol.req.IncludeUsers = (isEmpty ? 1 : 0);

        yield return protocol.Send();

        if (protocol.IsSuccess())
        {
            if (isEmpty)
            {
                myRank = protocol.res.MyRank;
                participants = protocol.res.Users;
            }

            if (protocol.res.Rank != null && protocol.res.Rank.Length > 0)
            {
                foreach (ContestRankInfo rankInfo in protocol.res.Rank)
                {
                    if (rankInfo != null)
                    {
                        rankInfoList.Add(rankInfo);
                    }

                    yield return null;
                }
            }

            // #1294
            requestable = (startIndex > 1) && ((startIndex % 10) == 1) && (startIndex < 31);
        }
        else
        {
            requestable = true;
        }

        callback.InvokeWithNullCheck(this);
    }
}
