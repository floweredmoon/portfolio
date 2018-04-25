using Delta.Protocol;
using System;
using System.Collections;
using Table;
using UnityEngine;

public class UI_CraftItem : MonoBehaviour
{
    #region Fields
    public UIButton m_Button;
    public Transform m_ItemRoot;
    public UI_Item m_ItemComponent;
    public UISlider m_Slider;
    public UILabel m_SliderLabel;
    public UILabel m_ProductionLabel;
    public GameObject m_FXGameObject;
    public GameObject m_LockRoot;
    public UILabel m_LockLevelLabel;
    public UILabel m_LockDescriptionLabel;
    #endregion

    #region Properties
    public int slotIndex
    {
        get;
        set;
    }

    public uint? itemIndex
    {
        get;
        private set;
    }

    public bool isLock
    {
        get;
        private set;
    }

    private TimeSpan remainTimeSpan
    {
        get;
        set;
    }

    private bool isComplete
    {
        get;
        set;
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        Util_NGUI.EventDelegate_Set(this, m_Button, "OnClick");
    }

    // Use this for initialization

    // Update is called once per frame

    #endregion

    public void SetSlotInfo(int slotIndex)
    {
        this.slotIndex = slotIndex;

        StopAllCoroutines();

        var slotInfo = ProductionManager.Instance.FindSlotInfo(slotIndex);
        if (slotInfo != null)
        {
            var itemData = TableManager.Instance.item.GetData((uint)slotInfo.tiProductItem);
            var startTime = slotInfo.StartUTime.FromUnixTimeToDateTime();
            var endTime = slotInfo.FinishUTime.FromUnixTimeToDateTime();
            var totalTimeSpan = endTime - startTime;

            itemIndex = (itemData != null) ? (uint?)itemData.uiID : null;
            SetItem(itemData);
            StartCoroutine(UpdateTime(endTime, totalTimeSpan));
        }
        else
        {
            SetEmpty(slotIndex);
        }
    }

    private void SetEmpty(int slotIndex)
    {
        this.slotIndex = slotIndex;

        if (GetLock(slotIndex))
        {
            SetLock(true);
        }
        else
        {
            SetLock(false);
            // 24602 : [A8F971]제작이 가능한 공간 입니다.[-]
            Util_NGUI.Label_SetText(m_ProductionLabel, TableManager.Instance.GetLanguageString(24602));
        }

        SetItem(null);
        UnityHelper.SetActive(m_Slider, false);
        UnityHelper.SetActive(m_FXGameObject, false);
    }

    private IEnumerator UpdateTime(DateTime endTime, TimeSpan totalTimeSpan)
    {
        isComplete = false;

        uint langIdx = 0;
        while (!isComplete)
        {
            isComplete = (endTime != DateTime.MinValue) && (DateTime.UtcNow > endTime);
            if (isComplete)
            {
                // 24608 : 제작 완료
                langIdx = 24608;
            }
            else
            {
                // 24607 : 제작 중
                langIdx = 24607;

                remainTimeSpan = endTime - DateTime.UtcNow;
                GlobalFunction.SetLabelToSecondTime_TwoTimes(m_SliderLabel, (uint)remainTimeSpan.TotalSeconds);
                if (m_Slider != null)
                {
                    m_Slider.value = 1f - ((float)remainTimeSpan.TotalSeconds / (float)totalTimeSpan.TotalSeconds);
                }
            }

            Util_NGUI.Label_SetText(m_ProductionLabel, TableManager.Instance.GetLanguageString(langIdx));
            UnityHelper.SetActive(m_Slider, !isComplete);
            UnityHelper.SetActive(m_FXGameObject, !isComplete);

            yield return null;
        }
    }

    private void SetLock(bool isLock)
    {
        this.isLock = isLock;

        UnityHelper.SetActive(m_LockRoot, isLock);

        if (isLock)
        {
            Util_NGUI.Label_SetText(m_LockLevelLabel, GetLimitLevelString(slotIndex));
        }
    }

    private bool GetLock(int slotIndex)
    {
        bool isLock = false;
        int limitLevel = GetLimitLevel(slotIndex);
        // 1 ~ 4
        if (slotIndex > 0 && slotIndex < 5)
        {
            isLock = limitLevel > Global.account.userInfo.Level;
        }
        // 5 ~
        else if (slotIndex > 4)
        {
            isLock = limitLevel > Global.account.userInfo.VipLevel;
        }

        return isLock;
    }

    private string GetLimitLevelString(int slotIndex)
    {
        string levelLimitString = string.Empty;
        int limitLevel = GetLimitLevel(slotIndex);
        if (slotIndex > 0 && slotIndex < 5)
        {
            levelLimitString = string.Format("Lv {0}", limitLevel);
        }
        else if (slotIndex > 4)
        {
            levelLimitString = string.Format("P {0}", limitLevel);
        }

        return levelLimitString;
    }

    private int GetLimitLevel(int slotIndex)
    {
        int limitLevel = 0;
        // 1 ~ 4
        if (slotIndex > 0 && slotIndex < 5)
        {
            // Account Level
            limitLevel = ((slotIndex - 1) * 15);
        }
        // 5 ~
        else if (slotIndex > 4)
        {
            // Premium Level
            limitLevel = ((slotIndex - 5) * 5);
        }
        limitLevel = Mathf.Max(1, limitLevel);

        return limitLevel;
    }

    private void SetItem(delta_T_Item.Data itemData)
    {
        if (itemData != null)
        {
            if (m_ItemComponent == null)
            {
                CreateItemComponent();
            }
            if (m_ItemComponent != null)
            {
                m_ItemComponent.SetItem(itemData);
                UnityHelper.SetActive(m_ItemComponent, true);
            }
        }
        else
        {
            UnityHelper.SetActive(m_ItemComponent, false);
        }
    }

    private UI_Item CreateItemComponent()
    {
        if (m_ItemComponent == null)
        {
            UI_Item item = null;
            GameObject gameObject = ResourcesManager.Instantiate("UI/UI_Item");
            if (gameObject != null)
            {
                item = gameObject.GetComponent<UI_Item>();
                if (item != null)
                {
                    gameObject.transform.SetParent(m_ItemRoot);
                    gameObject.transform.localPosition = Vector3.zero;
                    gameObject.transform.localScale = Vector3.one;

                    item.stackVisible = false;
                    Util_NGUI.Button_SetEnableWithCollider(item.GetComponent<UIButton>(), false);
                    m_ItemComponent = item;
                }
                else Destroy(gameObject);
            }
        }

        return m_ItemComponent;
    }

    public void OnClick()
    {
        var slotInfo = ProductionManager.Instance.FindSlotInfo(slotIndex);
        if (slotInfo != null)
        {
            if (slotInfo.HasComplete())
            {
                ProductionManager.Instance.CompleteProduction(slotIndex);
            }
            else
            {
                if (itemIndex.HasValue)
                {
                    var gameObject = UI_PopupManager.sOpen("UI/UI_CraftDetailInfo");
                    if (gameObject != null)
                    {
                        var component = gameObject.GetComponent<UI_CraftDetailInfo>();
                        if (component != null)
                        {
                            component.SetFishingRod(itemIndex.Value, slotIndex);
                        }
                        else Destroy(gameObject);
                    }
                }
            }
        }
    }
}
