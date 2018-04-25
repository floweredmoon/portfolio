using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UI_FishBookPopup : MonoBehaviour
{
    #region Fields
    public UIScrollView mScrollView;
    public UIGrid mGrid;
    #endregion

    #region Properties
    private bool initialized
    {
        get;
        set;
    }

    private uint localPointIndex
    {
        get;
        set;
    }
    #endregion

    #region MonoBehaviour
    // Use this for initialization
    void Start()
    {
        StartCoroutine(SetBookInfoList());
    }

    // Update is called once per frame
    #endregion

    public void CenterOn(uint localPointIndex)
    {
        this.localPointIndex = localPointIndex;
    }

    private IEnumerator SetBookInfoList()
    {
        var respond = false;
        FishBookManager.Instance.RequestBookInfoList(() =>
        {
            respond = true;
        });

        while (!respond)
        {
            yield return null;
        }


        int panelDepth = mScrollView.GetComponent<UIPanel>().depth + 1; // [WARNING]
        Transform centerOnTarget = null;
        foreach (var localPointData in TableManager.Instance.localPoint.GetValues())
        {
            // uiID < 1000 : 대회 출조지
            // FirstFishBookIdx > 0 : 어류 도감 여부
            if (localPointData.uiID < 1000 && localPointData.FirstFishBookIdx > 0)
            {
                var bookItem = CreateInstance();
                if (bookItem != null)
                {
                    if (localPointIndex == localPointData.uiID)
                    {
                        centerOnTarget = bookItem.transform;
                    }

                    bookItem.panelDepth = panelDepth;
                    bookItem.SetLocalPointData(localPointData);
                }

                Util_NGUI.Reposition(mGrid);
                Util_NGUI.ResetPosition(mScrollView);
            }

            yield return null;
        }

        if (centerOnTarget != null)
        {
            SpringPanel.Begin(mScrollView.gameObject, -centerOnTarget.localPosition, 8f);
        }
    }

    private UI_FishBookItem CreateInstance()
    {
        UI_FishBookItem bookItem = null;
        var gameObject = ResourcesManager.Instantiate("UI/UI_FishBook_Item");
        if (gameObject != null)
        {
            bookItem = gameObject.GetComponent<UI_FishBookItem>();
            if (bookItem != null)
            {
                bookItem.transform.SetParent(mGrid.transform);
                bookItem.transform.localPosition = Vector3.zero;
                bookItem.transform.localScale = Vector3.one;
            }
            else Destroy(gameObject);
        }

        return bookItem;
    }
}
