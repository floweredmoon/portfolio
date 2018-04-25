using Delta.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using Table;
using UnityEngine;

public class FishBookManager : kSingletonPersistent<FishBookManager>
{
    #region Properties

    // Key is bookIndex
    public Dictionary<uint, FishBookInfo> bookInfoDict
    {
        get; // ReadOnly
        private set;
    }

    private bool isLatestInfo
    {
        get;
        set;
    }

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        bookInfoDict = new Dictionary<uint, FishBookInfo>();
    }

    // Use this for initialization

    // Update is called once per frame

    #endregion

    public void UpdateBookInfo(uint stageIndex, uint fishIndex, FishBookCompleted protocol)
    {
        if (protocol != null)
        {
            var localPointIndex = (uint)(stageIndex / 1000);
            var bookIndex = (uint)protocol.TableIndex;
            var stateType = protocol.AllCollected == 1 ? FishBookInfo.eStateType.AllCollected : FishBookInfo.eStateType.NotYetCollected;
            // [WARNING] Coroutine
            StartCoroutine(UpdateBookInfoByCoroutine(bookIndex, localPointIndex, fishIndex, stateType,
                                                     () =>
                                                     {
                                                         isLatestInfo = false;
                                                         UpdateBadge();
                                                     }));
            Alert(bookIndex, localPointIndex, stateType);
        }
    }

    private IEnumerator UpdateBookInfoByCoroutine(uint bookIndex, uint localPointIndex, uint fishIndex, FishBookInfo.eStateType stateType, Action callback)
    {
        int fishFlag = 0;
        var bookInfo = FindBookInfo(bookIndex);
        if (bookInfo != null)
        {
            fishFlag = bookInfo.CatchedFishesBitFlag;
        }
        int fishOrder = 0;
        var fishBookData = TableManager.Instance.fishBook.GetData(bookIndex);
        if (fishBookData != null)
        {
            for (int i = 1; i <= 12; i++)
            {
                if (fishBookData.GetFieldData<int>("FishIdx" + i) == (int)fishIndex)
                {
                    fishOrder = i;
                    break;
                }

                yield return null;
            }
        }
        if (fishOrder > 0)
        {
            // Delta.Protocol.FishBookInfo.SetCatchedFishIndex
            fishFlag = fishFlag | (1 << (fishOrder - 1));
        }

        UpdateBookInfo(bookIndex, localPointIndex, fishFlag, stateType);

    }

    private void UpdateBookInfo(uint bookIndex, uint localPointIndex, int fishFlag, FishBookInfo.eStateType stateType)
    {
        Debug.LogFormat("[FishBookManager] UpdateBookInfo(bookIndex : {0}, localPointIndex : {1}, fishFlag : {2}, stateType : {3}", bookIndex, localPointIndex, fishFlag, stateType);
        var bookInfo = FindBookInfo(bookIndex);
        if (bookInfo == null)
        {
            bookInfo = new FishBookInfo();
            bookInfoDict.Add(bookIndex, bookInfo);
        }
        if (bookInfo != null)
        {
            // Deep Copy
            bookInfo.CatchedFishesBitFlag = fishFlag;
            bookInfo.State = (int)stateType;
            bookInfo.TableIndex = (int)bookIndex;
            bookInfo.tiLocalPoint = (int)localPointIndex;
        }
    }

    public FishBookInfo FindBookInfo(uint bookIndex)
    {
        FishBookInfo bookInfo = null;
        if (bookInfoDict.ContainsKey(bookIndex))
        {
            bookInfo = bookInfoDict[bookIndex];
        }

        return bookInfo;
    }

    public void Complete(uint localPointIndex, Action<uint?> callback)
    {
        StartCoroutine(CompleteByCoroutine(localPointIndex, callback));
    }

    private IEnumerator CompleteByCoroutine(uint localPointIndex, Action<uint?> callback)
    {
        var protocol = new ServerRequest<FishBookRewardReq, FishBookRewardRes>();
        protocol.showLoadingCircle = true;
        protocol.openErrorMsgBoxWhenReqFailed = true;
        protocol.req.tiLocalPoint = (int)localPointIndex;

        yield return protocol.Send();

        uint? bookIndex = null;
        if (protocol.IsSuccess())
        {
            if (protocol.res.RewardedTableIndex.HasValue)
            {
                bookIndex = (uint)protocol.res.RewardedTableIndex.Value;
                var bookInfo = FindBookInfo(bookIndex ?? 0);
                if (bookInfo != null)
                {
                    bookInfo.State = (int)FishBookInfo.eStateType.AlreadyReward;
                    isLatestInfo = false;
                    UpdateBadge();
                }
            }
        }

        callback.InvokeWithNullCheck(bookIndex);
    }

    public void RequestBookInfo(uint localPointIndex, Action<FishBookInfo> callback = null)
    {
        StartCoroutine(RequestBookInfoByCoroutine(localPointIndex, callback));
    }

    public IEnumerator RequestBookInfoByCoroutine(uint localPointIndex, Action<FishBookInfo> callback = null)
    {
        FishBookInfo bookInfo = null;
        var protocol = new ServerRequest<FishBookInfoReq, FishBookInfoRes>();
        protocol.openErrorMsgBoxWhenReqFailed = true;
        protocol.req.tiLocalPoint = (int)localPointIndex;

        yield return protocol.Send();

        if (protocol.IsSuccess())
        {
            if (protocol.res.List != null && protocol.res.List.Length > 0)
            {
                bookInfo = protocol.res.List[0];
                if (bookInfo != null)
                {
                    UpdateBookInfo((uint)bookInfo.TableIndex,
                                   (uint)bookInfo.tiLocalPoint,
                                   bookInfo.CatchedFishesBitFlag,
                                   (FishBookInfo.eStateType)bookInfo.State);
                    UpdateBadge();
                }
            }
        }

        callback.InvokeWithNullCheck(bookInfo);
    }

    public void RequestBookInfoList(Action callback = null)
    {
        StartCoroutine(RequestBookInfoListByCoroutine(callback));
    }

    private IEnumerator RequestBookInfoListByCoroutine(Action callback = null)
    {
        if (isLatestInfo)
        {
            callback.InvokeWithNullCheck();
            yield break;
        }

        if (bookInfoDict.Count > 0)
        {
            bookInfoDict.Clear();
        }

        var protocol = new ServerRequest<FishBookInfoReq, FishBookInfoRes>();
        //protocol.showLoadingCircle = true;
        protocol.openErrorMsgBoxWhenReqFailed = true;

        yield return protocol.Send();

        if (protocol.IsSuccess())
        {
            if (protocol.res.List != null && protocol.res.List.Length > 0)
            {
                foreach (var bookInfo in protocol.res.List)
                {
                    if (bookInfo != null)
                    {
                        if (!bookInfoDict.ContainsKey((uint)bookInfo.TableIndex))
                        {
                            bookInfoDict.Add((uint)bookInfo.TableIndex, bookInfo);
                        }
                    }
                }
            }

            isLatestInfo = true;
            UpdateBadge();
        }

        callback.InvokeWithNullCheck();
    }

    #region Extras

    private void Alert(uint bookIndex, uint localPointIndex, FishBookInfo.eStateType stateType)
    {
        var localPointData = TableManager.Instance.localPoint.GetData(localPointIndex);
        var fishBookData = TableManager.Instance.fishBook.GetData(bookIndex);
        if (localPointData != null && fishBookData != null)
        {
            var localPointName = TableManager.Instance.GetLanguageString(localPointData.LocalPointNameID);
            uint langIdx = 0;
            switch (stateType)
            {
                case FishBookInfo.eStateType.AllCollected:
                    // 27123 : 축하합니다! {0} {1} 단계 어류 도감을 완성했습니다!
                    langIdx = 27123;
                    break;
                case FishBookInfo.eStateType.NotYetCollected:
                    // 27124 : {0} {1} 단계 어류 도감에 기록되었습니다!
                    langIdx = 27124;
                    break;
            }

            UI_ContentPopup.sShowContentPopup(TableManager.Instance.GetLanguageString(langIdx, localPointName, fishBookData.Step));
        }
    }

    private void UpdateBadge()
    {
        StartCoroutine(UpdateBadgeByCoroutine());
    }

    private IEnumerator UpdateBadgeByCoroutine()
    {
        var isEnabled = false;
        if (bookInfoDict != null && bookInfoDict.Count > 0)
        {
            foreach (var bookInfo in bookInfoDict.Values)
            {
                if (bookInfo != null && bookInfo.State == (int)FishBookInfo.eStateType.AllCollected)
                {
                    isEnabled = true;
                    break;
                }

                yield return null;
            }
        }

        if (isEnabled)
        {
            LobbyNewActiveDataManager.Instance.AddLobbyNewActiveData(LobbyNewActiveDataManager.eLobbyIconType.Lobby_FishBook);
        }
        else
        {
            LobbyNewActiveDataManager.Instance.RemoveLobbyNewActiveData(LobbyNewActiveDataManager.eLobbyIconType.Lobby_FishBook);

            if (Lobby.singletone != null)
            {
                Lobby.singletone.SetUiLobbyNewIconActiveToType(LobbyNewActiveDataManager.eLobbyIconType.Lobby_FishBook, false);
            }
        }
    }

    #endregion
}
