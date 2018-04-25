using UnityEngine;
using System.Collections;

public class UI_FishBookInfo : MonoBehaviour
{
    #region Fields
    public UILabel mNameLabel;
    public UI_FishObject mFishObject;
    public UILabel mDetailLabel;
    public GameObject mBubble;
    #endregion

    #region MonoBehaviour
    // Use this for initialization

    // Update is called once per frame

    private void OnEnable()
    {
        UI_AccountTopInfo.SetVisible(false);
    }

    private void OnDisable()
    {
        UnityHelper.DestroyObject(mFishObject);
        UI_AccountTopInfo.SetVisible(true);
    }
    #endregion

    public void SetFishInfo(uint fishIndex)
    {
        var fishData = TableManager.Instance.fish.GetData(fishIndex);
        if (fishData != null)
        {
            Util_NGUI.Label_SetText(mNameLabel, TableManager.Instance.GetLanguageString(fishData.FishNameTableId));
            Util_NGUI.Label_SetText(mDetailLabel, TableManager.Instance.GetLanguageString(fishData.FishContentsId));

            GameObject gameObject = ResourcesManager.Instantiate(fishData.PrefabName + "_Fin");
            if (gameObject != null)
            {
                UI_FishObject fishObj = gameObject.GetComponent<UI_FishObject>();
                if (fishObj != null)
                {
                    mFishObject = fishObj;
                    UnityHelper.SetActive(fishObj.lastFishInfoCamera, false);
                    UnityHelper.SetActive(fishObj.measuringCamera, true);
                    if (fishObj.measuringCamera != null)
                    {
                        // #1498
                        fishObj.measuringCamera.depth = 3;
                        fishObj.measuringCamera.transform.localPosition = new Vector3(0f, -.04f, -.6f);

                        PictureInPicture component = fishObj.measuringCamera.GetComponent<PictureInPicture>();
                        if (component != null)
                        {
                            component.width = 48;
                            component.height = 33;
                            component.xOffset = 0f;
                            component.yOffset = -2.9f;
                        }

                        UnityHelper.SetActive(mBubble, true);
                        if (mBubble != null)
                        {
                            mBubble.transform.SetParent(fishObj.measuringCamera.transform);
                            mBubble.transform.localPosition = new Vector3(0f, -1.232788f, 2.114894f);
                            mBubble.transform.localScale = Vector3.one;
                        }
                    }
                }
                else Destroy(gameObject);
            }
        }
    }

    public void OnClick()
    {
        if (mFishObject != null)
        {
            mFishObject.SetAnimatorTrigger();
        }
    }
}
