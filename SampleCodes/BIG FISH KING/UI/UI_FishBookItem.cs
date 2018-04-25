using Delta.Protocol;
using System.Collections;
using System.Collections.Generic;
using Table;
using UnityEngine;

public class UI_FishBookItem : MonoBehaviour
{
    #region Fields
    public UIWidget mWidget;
    public UITexture mTexture;
    public UILabel mLabel;
    public UIScrollView mFishScrollView;
    public UIGrid mFishGrid;
    public List<UI_FishBookFish> mFishList;
    public List<UI_FishBookStep> mStepList;
    public UI_CommonItem mItem;
    public UIButton mButton;
    #endregion

    #region Properties
    public uint localPointIndex
    {
        get;
        private set;
    }

    private uint bookIndex
    {
        get;
        set;
    }

    public int panelDepth
    {
        set
        {
            if (mFishScrollView != null)
            {
                var panel = mFishScrollView.GetComponent<UIPanel>();
                if (panel != null)
                {
                    panel.depth = value;
                }
            }
        }
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        Util_NGUI.EventDelegate_Set(this, mButton, "OnCompleteButtonClick");
    }

    // Use this for initialization

    // Update is called once per frame

    #endregion

    public void SetLocalPointData(delta_T_LocalPoint.Data localPointData)
    {
        if (localPointData != null)
        {
            localPointIndex = localPointData.uiID;

            StartCoroutine(SetLocalPointDataByCoroutine(localPointData));
        }
    }

    private IEnumerator SetLocalPointDataByCoroutine(delta_T_LocalPoint.Data localPointData)
    {
        if (localPointData != null)
        {
            Util_NGUI.SetTexture(mTexture, localPointData.LocalPointImageName);
            Util_NGUI.Label_SetText(mLabel, TableManager.Instance.GetLanguageString(localPointData.LocalPointNameID));

            // [WARNING]
            for (uint i = 0; i < mStepList.Count; i++)
            {
                var bookStep = mStepList[(int)i];
                if (bookStep != null)
                {
                    bookStep.SetEventDelegate(this, "SetBookInfo");
                    bookStep.SetBookInfo(localPointData.FirstFishBookIdx + i);
                }

                yield return null;
            }

            var bookIndex = localPointData.FirstFishBookIdx;
            var fishBookData = TableManager.Instance.fishBook.GetData(bookIndex);
            FishBookInfo validBookInfo;
            if (fishBookData != null && fishBookData.TryGetValidBookInfo(out validBookInfo))
            {
                bookIndex = (uint)validBookInfo.TableIndex;
            }

            yield return StartCoroutine(SetBookInfoByCoroutine(bookIndex, true));
        }
    }

    public void SetBookInfo(UI_FishBookStep component)
    {
        if (component != null)
        {
            StartCoroutine(SetBookInfoByCoroutine(component.bookIndex, false));
        }
    }

    private IEnumerator SetBookInfoByCoroutine(uint bookIndex, bool initialize)
    {
        if (this.bookIndex == bookIndex)
        {
            yield break;
        }

        this.bookIndex = bookIndex;

        GameObject gameObject = null;
        if (initialize)
        {
            gameObject = UIWaitForAction.Start(mWidget);
        }
        else
        {
            gameObject = UIWaitForAction.Start(mFishScrollView.GetComponent<UIPanel>());
        }

        StartCoroutine(Select(bookIndex));

        var fishBookData = TableManager.Instance.fishBook.GetData(bookIndex);
        if (fishBookData != null)
        {
            yield return StartCoroutine(ClearByCoroutine());

            for (int fishOrder = 1; fishOrder <= 12; fishOrder++)
            {
                int fishIndex = fishBookData.GetFieldData<int>("FishIdx" + fishOrder);
                if (fishIndex > 0)
                {
                    int fishGrade = fishBookData.GetFieldData<int>("FishGrade" + fishOrder);
                    var bookFish = GetInstance();
                    if (bookFish != null)
                    {
                        bookFish.transform.SetParent(mFishGrid.transform);
                        bookFish.transform.localPosition = Vector3.zero;
                        bookFish.transform.localScale = Vector3.one;
                        bookFish.SetFishData(bookIndex, fishOrder, fishGrade, (uint)fishIndex);
                        UnityHelper.SetActive(bookFish, true);
                    }

                    Util_NGUI.Reposition(mFishGrid);

                    yield return null;
                }
            }

            yield return new WaitForEndOfFrame();

            SpringPanel.Begin(mFishScrollView.gameObject, Vector3.zero, 8f);
            SetRewardInfo(fishBookData);
        }

        UIWaitForAction.Stop(gameObject);
    }

    private void SetRewardInfo(delta_T_FishBook.Data fishBookData, FishBookInfo bookInfo = null)
    {
        if (fishBookData != null)
        {
            var rewardData = TableManager.Instance.reward.GetData(fishBookData.RwdIdx);
            if (rewardData != null)
            {
                mItem.SetGoods((Delta.Protocol.eMailType)rewardData.Type, (int)rewardData.RwdNum);
            }

            bool isEnabled = false;
            if (bookInfo == null)
            {
                bookInfo = fishBookData.GetBookInfo();
            }
            if (bookInfo != null)
            {
                isEnabled = bookInfo.State == (int)FishBookInfo.eStateType.AllCollected;
            }

            Util_NGUI.Button_SetEnableWithCollider(mButton, isEnabled);
        }
    }

    private void OnCompleteButtonClick()
    {
        var bookInfo = FishBookManager.Instance.FindBookInfo(this.bookIndex);
        if (bookInfo != null && bookInfo.State == (int)FishBookInfo.eStateType.AllCollected)
        {
            FishBookManager.Instance.Complete(localPointIndex, (bookIndex) =>
            {
                if (bookIndex.HasValue)
                {
                    var fishBookData = TableManager.Instance.fishBook.GetData(bookIndex.Value);
                    if (fishBookData != null && fishBookData.Next > 0)
                    {
                        StartCoroutine(SetBookInfoByCoroutine(fishBookData.Next, false));
                    }
                    else
                    {
                        SetRewardInfo(fishBookData, bookInfo);
                    }
                }
            });
        }
    }

    private IEnumerator ClearByCoroutine()
    {
        foreach (var bookFish in mFishList)
        {
            if (bookFish != null)
            {
                UnityHelper.SetActive(bookFish, false);
                bookFish.transform.SetParent(transform);
            }

            yield return null;
        }
    }

    private UI_FishBookFish GetInstance()
    {
        var bookFish = mFishList.Find(match => !match.gameObject.activeSelf);
        if (bookFish == null)
        {
            var gameObject = ResourcesManager.Instantiate("UI/UI_FishBook_Fish");
            if (gameObject != null)
            {
                bookFish = gameObject.GetComponent<UI_FishBookFish>();
                if (bookFish != null)
                {
                    mFishList.Add(bookFish);
                }
                else Destroy(gameObject);
            }
        }

        return bookFish;
    }

    private IEnumerator Select(uint bookIndex)
    {
        if (mStepList != null && mStepList.Count > 0)
        {
            for (int i = 0; i < mStepList.Count; i++)
            {
                if (mStepList[i] != null)
                {
                    mStepList[i].Select(bookIndex);
                }

                yield return null;
            }
        }
    }
}
