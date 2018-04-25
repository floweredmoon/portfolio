using Delta.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using Table;
using UnityEngine;

public class UI_CraftDetailInfo : MonoBehaviour
{
    #region Fields
    public UILabel m_TitleLabel;
    public Transform m_ItemRoot;
    public UI_Item m_ItemComponent;
    public UIScrollView m_ArgsScrollView;
    public UIGrid m_ArgsGrid;
    public List<UILabel> m_ArgLabelList;
    public UILabel m_RequireNormalPowderLabel;
    public UILabel m_RequireGoldPowderLabel;
    public UILabel m_RequireBattleCoinLabel;
    public UILabel m_RequireGoldLabel;
    public UILabel m_RequireEcoPointLabel;
    public UIButton m_CompleteImmediateButton;
    public UILabel m_CompleteImmediateCashLabel;
    public UILabel m_ProductionTimeLabel;
    public UISlider m_Slider;
    public UILabel m_SliderLabel;
    public UIButton m_StartProductionButton;
    public UILabel m_StartProductionButtonLabel;
    #endregion

    #region Properties
    public int slotIndex
    {
        get;
        private set;
    }

    public uint itemIndex
    {
        get;
        private set;
    }

    private bool isProceeding // ->
    {
        get;
        set;
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
        Util_NGUI.EventDelegate_Set(this, m_CompleteImmediateButton, "OnCompleteImmediateButtonClick");
    }

    // Use this for initialization

    // Update is called once per frame

    private void OnEnable()
    {
        ProductionManager.Instance.onCancelProduction += OnCancelProduction;
        ProductionManager.Instance.onStartProduction += OnStartProduction;
    }

    private void OnDisable()
    {
        ProductionManager.Instance.onCancelProduction -= OnCancelProduction;
        ProductionManager.Instance.onStartProduction -= OnStartProduction;
    }
    #endregion

    private void OnStartProduction(int slotIndex, uint itemIndex, bool immediate)
    {
        if (!immediate)
        {
            Util_NGUI.SoundPlayToUIButton(m_StartProductionButton);

            var fishingRodData = TableManager.Instance.fishingRod.GetData(itemIndex);
            if (fishingRodData != null)
            {
                string fishingRodName = TableManager.Instance.GetLanguageString(fishingRodData.FishingRodNameTableId);
                // 24618 : {0} {1} 제작을 시작합니다.
                string message = TableManager.Instance.GetLanguageString(24618, fishingRodName, fishingRodData.RodLength.ToString("F1"));

                UI_ContentPopup.sShowContentPopup(message);
            }
        }
    }

    private void OnCancelProduction(int slotIndex)
    {
        UI_PopupManager.sClose();
    }

    public void SetFishingRod(uint itemIndex, int? slotIndex = null) // -> itemIndex, slotIndex
    {
        this.slotIndex = slotIndex ?? -1;
        this.itemIndex = itemIndex;

        var itemProductionData = TableManager.Instance.itemProduction.GetData(itemIndex);
        if (itemProductionData != null)
        {
            var itemData = TableManager.Instance.item.GetData(itemIndex);
            var fishingRodData = TableManager.Instance.fishingRod.GetData(itemIndex);
            if (itemData != null && fishingRodData != null)
            {
                // 20115 : 제작
                Util_NGUI.Label_SetText(m_TitleLabel, string.Format("{0} {1:F1} {2}",
                                                                    TableManager.Instance.GetLanguageString(itemData.NameStringID),
                                                                    fishingRodData.RodLength,
                                                                    TableManager.Instance.GetLanguageString(20115)));
                SetItem(itemData);
                SetArgs(fishingRodData);
            }

            GoodsChunk requireGoods;
            if (ProductionManager.TryGetProductionRequireGoods(itemIndex, out requireGoods))
            {
                foreach (var requireGood in requireGoods.goods)
                {
                    UILabel label = null;
                    bool available = false;
                    switch (requireGood.type)
                    {
                        case Delta.Protocol.eMailType.BattleCoin:
                            label = m_RequireBattleCoinLabel;
                            available = requireGood.amount <= Global.account.userInfo.BattleCoin;
                            break;
                        case Delta.Protocol.eMailType.EcoPoint:
                            label = m_RequireEcoPointLabel;
                            available = requireGood.amount <= Global.account.userInfo.Eco;
                            break;
                        case Delta.Protocol.eMailType.Gold:
                            label = m_RequireGoldLabel;
                            available = requireGood.amount <= Global.account.userInfo.Gold;
                            break;
                        case Delta.Protocol.eMailType.GoldPowder:
                            label = m_RequireGoldPowderLabel;
                            available = requireGood.amount <= Global.account.userInfo.GoldPowder;
                            break;
                        case eMailType.NormalPowder:
                            label = m_RequireNormalPowderLabel;
                            available = requireGood.amount <= Global.account.userInfo.NormalPowder;
                            break;
                    }

                    if (label != null)
                    {
                        Util_NGUI.Label_SetText(label, requireGood.amount.ToStringWithComma());
                        label.color = available ? Color.white : Color.red;
                    }
                }
            }

            uint requireCash;
            if (ProductionManager.TryGetRequireCashForCompleteImmediate(itemIndex, out requireCash))
            {
                Util_NGUI.Label_SetText(m_CompleteImmediateCashLabel, requireCash.ToStringWithComma());
            }

            var slotInfo = ProductionManager.Instance.FindSlotInfo(slotIndex ?? -1);
            isProceeding = slotInfo != null;
            uint langIdx = 0;
            string methodName = string.Empty;
            if (isProceeding)
            {
                var endTime = slotInfo.FinishUTime.FromUnixTimeToDateTime();
                TimeSpan totalTimeSpan;
                ProductionManager.TryGetProductionRequireTime(itemIndex, out totalTimeSpan);

                StartCoroutine(UpdateTime(endTime, totalTimeSpan));
                StartCoroutine(Calculate(endTime));
                methodName = "OnCancelProductionButtonClick";
                // 24609 : 제작 취소
                langIdx = 24609;
                UnityHelper.SetActive(m_ProductionTimeLabel, false);
            }
            else
            {
                methodName = "OnStartProductionButtonClick";
                // 24603 : 제작 시작
                langIdx = 24603;

                TimeSpan productionTimeSpan;
                if (ProductionManager.TryGetProductionRequireTime(itemIndex, out productionTimeSpan))
                {
                    Util_NGUI.Label_SetText(m_ProductionTimeLabel, GlobalFunction.GetTimeStringToSecondTime_TwoTimes((uint)productionTimeSpan.TotalSeconds));
                    UnityHelper.SetActive(m_ProductionTimeLabel, true);
                }
            }
            Util_NGUI.EventDelegate_Set(this, m_StartProductionButton, methodName);
            Util_NGUI.Label_SetText(m_StartProductionButtonLabel, TableManager.Instance.GetLanguageString(langIdx));
        }
    }

    private void SetArgs(delta_T_FishingRod.Data fishingRodData) // ->
    {
        if (fishingRodData == null)
        {
            return;
        }

        var fishingRodStat = kItemExtensions.GetItemStat(fishingRodData.uiID, 0);
        if (fishingRodData == null)
        {
            return;
        }

        ClearArgs();

        // 21026 : 요구 레벨 : [FF8F31]{0}[-]
        // 21011 : 요구 레벨 : [A8F971]{0}[-]
        uint langIdx = (uint)(Global.account.userInfo.Level < fishingRodData.RodNeedLevel ? 21026 : 21011);
        SetArg(TableManager.Instance.GetLanguageString(langIdx, fishingRodData.RodNeedLevel));

        // 24511 : 튜닝 단계 : [43F7FF]{0}단계[-]
        //SetArg(TableManager.Instance.GetLanguageString(24511, 0));

        // 21004 : 공격력 : [A8F971]{0} ~ {1}[-]
        SetArg(TableManager.Instance.GetLanguageString(21004, fishingRodStat[StatType.AttackMin].GetAsInt(), fishingRodStat[StatType.AttackMax].GetAsInt()));

        // 21038 : 치명타 확률 : [A8F971]{0}%[-]
        SetArg(TableManager.Instance.GetLanguageString(21038, fishingRodStat[StatType.CriticalProbability].GetAsInt()));

        // 21005 : 치명타 데미지량 : [A8F971]{0}%[-]
        SetArg(TableManager.Instance.GetLanguageString(21005, fishingRodStat[StatType.CriticalDgPer].GetAsInt()));

        // 21027 : 치명타 포인트/회 : [A8F971]{0}[-]
        SetArg(TableManager.Instance.GetLanguageString(21027, fishingRodStat[StatType.CriticalStHit].GetAsInt()));

        // 21046 : 더블 치명타 확률 : [A8F971]{0}%[-]
        // #2077
        //SetArg(TableManager.Instance.GetLanguageString(21046, fishingRodStat[StatType.DoubleCriticalProb].GetAsInt()));

        Util_NGUI.Reposition(m_ArgsGrid);
        Util_NGUI.SetScrollBar(m_ArgsScrollView, 0f);
    }

    private void SetArg(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        UILabel labelComp = null;
        if (m_ArgLabelList != null && m_ArgLabelList.Count > 0)
        {
            foreach (var argLabel in m_ArgLabelList)
            {
                if (argLabel != null && !argLabel.gameObject.activeSelf)
                {
                    labelComp = argLabel;
                    break;
                }
            }
        }
        if (labelComp == null)
        {
            labelComp = CreateArg();
        }

        if (labelComp != null)
        {
            Util_NGUI.Label_SetText(labelComp, value);
            UnityHelper.SetActive(labelComp, true);
        }
    }

    private void ClearArgs()
    {
        if (m_ArgLabelList != null && m_ArgLabelList.Count > 0)
        {
            foreach (var argLabel in m_ArgLabelList)
            {
                UnityHelper.SetActive(argLabel, false);
            }
        }
    }

    private UILabel CreateArg()
    {
        UILabel labelComp = null;
        GameObject gameObject = ResourcesManager.Instantiate("UI/UI_ItemDetailInfo_Parameter_Text");
        if (gameObject != null)
        {
            labelComp = gameObject.GetComponent<UILabel>();
            if (labelComp != null)
            {
                labelComp.transform.SetParent(m_ArgsGrid.transform);
                labelComp.transform.localScale = Vector3.one;
                UnityHelper.SetActive(labelComp, false);
                m_ArgLabelList.Add(labelComp);
            }
        }

        return labelComp;
    }

    private void SetItem(delta_T_Item.Data itemData)
    {
        if (itemData == null)
        {
            return;
        }

        if (m_ItemComponent == null)
        {
            GameObject gameObject = ResourcesManager.Instantiate("UI/UI_Item_LargeIcon");
            if (gameObject != null)
            {
                m_ItemComponent = gameObject.GetComponent<UI_Item>();
                if (m_ItemComponent != null)
                {
                    m_ItemComponent.stackVisible = false;
                    m_ItemComponent.transform.SetParent(m_ItemRoot);
                    m_ItemComponent.transform.localScale = Vector3.one;
                    m_ItemComponent.transform.localPosition = Vector3.zero;
                }
            }
        }

        if (m_ItemComponent != null)
        {
            m_ItemComponent.SetItem(itemData);
        }
    }

    private IEnumerator UpdateTime(DateTime endTime, TimeSpan totalTimeSpan) // -> , startTime
    {
        isComplete = false;
        while (!isComplete)
        {
            isComplete = DateTime.UtcNow > endTime;
            if (isComplete)
            {
                // Disable start production button and complete immediate button ...
            }
            else
            {
                remainTimeSpan = endTime - DateTime.UtcNow;

                if (m_Slider != null)
                {
                    m_Slider.value = 1f - ((float)remainTimeSpan.TotalSeconds / (float)totalTimeSpan.TotalSeconds);
                    UnityHelper.SetActive(m_Slider, true);
                }

                GlobalFunction.SetLabelToSecondTime_TwoTimes(m_SliderLabel, (uint)remainTimeSpan.TotalSeconds);
                UnityHelper.SetActive(m_SliderLabel, true);
            }

            yield return null;
        }

        yield break;
    }

    private IEnumerator Calculate(DateTime endTime) // ->
    {
        bool isComplete = false;
        TimeSpan remainTimeSpan;

        while (!isComplete)
        {
            isComplete = DateTime.UtcNow > endTime;
            remainTimeSpan = endTime - DateTime.UtcNow;

            uint requireCash;
            if (ProductionManager.TryGetRequireCashForCompleteImmediate(itemIndex, (int)remainTimeSpan.TotalSeconds, out requireCash))
            {
                Util_NGUI.Label_SetText(m_CompleteImmediateCashLabel, requireCash.ToStringWithComma());
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public void OnCompleteImmediateButtonClick()
    {
        // [WARNING]
        //if (ProductionManager.CanStartProduction(itemIndex))

        bool isCan = false;
        if (isProceeding)
        {
            isCan = ProductionManager.CanCompleteImmediateBySlotIndex(slotIndex);
        }
        else
        {
            isCan = ProductionManager.CanCompleteImmediateByItemIndex(itemIndex);
        }

        if (isCan)
        {
            var itemData = TableManager.Instance.item.GetData(itemIndex);
            if (itemData != null)
            {
                // 10016 : 알림
                // 24606 : {0} 낚싯대 제작을  [A8F971]즉시 완료[-]합니다. 진행 하시겠습니까?
                UI_PopupManager.sOpen_YesNoMessagePopup(TableManager.Instance.GetLanguageString(10016),
                                                        TableManager.Instance.GetLanguageString(24606, TableManager.Instance.GetLanguageString(itemData.NameStringID)),
                                                        gameObject,
                                                        "OnCompleteImmediateConfirm",
                                                        "OnCompleteImmediateCancel");
            }
        }
        else
        {
            UI_PopupManager.sCreateChargeCashContinue();
        }
    }

    public void OnCompleteImmediateConfirm()
    {
        UI_PopupManager.sClose();

        if (isProceeding)
        {
            ProductionManager.Instance.CompleteProductionImmediate(slotIndex);
        }
        else
        {
            ProductionManager.Instance.StartProduction(itemIndex, true);
        }
    }

    public void OnCompleteImmediateCancel()
    {
        UI_PopupManager.sClose();
    }

    public void OnStartProductionButtonClick()
    {
        // [WARNING]
        //if (ProductionManager.CanStartProduction(itemIndex))
        {
            var itemData = TableManager.Instance.item.GetData(itemIndex);
            if (itemData != null)
            {
                // 10016 : 알림
                // 24605 : {0} 낚싯대 제작을 시작 하시겠습니까?
                UI_PopupManager.sOpen_YesNoMessagePopup(TableManager.Instance.GetLanguageString(10016),
                                                        TableManager.Instance.GetLanguageString(24605, TableManager.Instance.GetLanguageString(itemData.NameStringID)),
                                                        gameObject,
                                                        "OnStartProductionConfirm",
                                                        "OnStartProductionCancel");
            }
        }
    }

    public void OnStartProductionConfirm()
    {
        UI_PopupManager.sClose();
        ProductionManager.Instance.StartProduction(itemIndex, false);
    }

    public void OnStartProductionCancel()
    {
        UI_PopupManager.sClose();
    }

    public void OnCancelProductionButtonClick()
    {
        // 10016 : 알림
        // 24605 : "“정말로 [FF8F31]제작을 취소[-] 하시겠습니까?” [FF8F31]""제작 취소는 복구되지 않습니다.""[-]"
        UI_PopupManager.sOpen_YesNoMessagePopup(TableManager.Instance.GetLanguageString(10016),
                                                TableManager.Instance.GetLanguageString(24613),
                                                gameObject,
                                                "OnCancelProductionConfirm",
                                                "OnCancelProductionCancel");
    }

    public void OnCancelProductionConfirm()
    {
        UI_PopupManager.sClose();
        ProductionManager.Instance.CancelProduction(slotIndex);
    }

    public void OnCancelProductionCancel()
    {
        UI_PopupManager.sClose();
    }
}
