using System.Collections;
using System.Collections.Generic;
using Table;
using UnityEngine;

public class UI_CraftPopup : MonoBehaviour
{
    #region Fields
    public kRollingNumbers m_NormalPowder;
    public kRollingNumbers m_GoldPowder;
    public kRollingNumbers m_BattleCoin;
    public List<UIToggle> m_ToggleList;
    public UIScrollView m_ProductScrollView;
    public List<UIGrid> m_ProductGridList;
    public List<UI_Item> m_ProductList;
    public UIScrollView m_SlotScrollView;
    public UIGrid m_SlotGrid;
    public List<UI_CraftItem> m_SlotList;
    #endregion

    #region Properties
    // CreateSlotList, UpdateSlotInfo Coroutine에서 사용
    private bool slotsInitialized
    {
        get;
        set;
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        StartCoroutine(CreateProductList());
        StartCoroutine(CreateSlotList());
    }

    // Use this for initialization
    private void Start()
    {
        StartCoroutine(UpdateSlotInfo());
    }

    // Update is called once per frame

    private void OnEnable()
    {
        ProductionManager.Instance.onCancelProduction += OnCancelProduction;
        ProductionManager.Instance.onCompleteProduction += OnCompleteProduction;
        ProductionManager.Instance.onStartProduction += OnStartProduction;

        OnUpdateUserInfo(true);
    }

    private void OnDisable()
    {
        ProductionManager.Instance.onCancelProduction -= OnCancelProduction;
        ProductionManager.Instance.onCompleteProduction -= OnCompleteProduction;
        ProductionManager.Instance.onStartProduction -= OnStartProduction;
    }
    #endregion

    private void OnCompleteProduction(int slotIndex, uint itemIndex, bool immediate)
    {
        UI_PopupManager.sClose(); // UI_CraftDetailInfo
        StartDirection(slotIndex, itemIndex);
    }

    private void OnStartProduction(int slotIndex, uint itemIndex, bool immediate)
    {
        UI_PopupManager.sClose(); // UI_CraftDetailInfo

        if (immediate)
        {
            StartDirection(slotIndex, itemIndex);
        }
        else
        {
            UpdateSlotInfo(slotIndex);
            OnUpdateUserInfo(false);
        }
    }

    private void StartDirection(int slotIndex, uint itemIndex)
    {
        GameObject gameObject = UI_PopupManager.sOpen("UI/UI_Craft_Complete");
        if (gameObject != null)
        {
            UI_CraftComplete component = gameObject.GetComponent<UI_CraftComplete>();
            if (component != null)
            {
                component.Set(itemIndex);
                StartCoroutine(WaitForDirection(slotIndex));
            }
            else Destroy(gameObject);
        }
    }

    private IEnumerator WaitForDirection(int slotIndex)
    {
        // UI_CraftComplete
        yield return new UIPopupCloseYield();

        UpdateSlotInfo(slotIndex);
        OnUpdateUserInfo(false);
    }

    private void OnCancelProduction(int slotIndex)
    {
        OnUpdateUserInfo(false);
        UpdateSlotInfo(slotIndex);
    }

    private void OnUpdateUserInfo(bool initialize)
    {
        int normalPowder = Global.account.userInfo.NormalPowder;
        int goldPowder = Global.account.userInfo.GoldPowder;
        int battleCoin = Global.account.userInfo.BattleCoin;

        if (initialize)
        {
            m_NormalPowder.InitValue(normalPowder);
            m_GoldPowder.InitValue(goldPowder);
            m_BattleCoin.InitValue(battleCoin);
        }
        else
        {
            m_NormalPowder.MoveToTime(normalPowder);
            m_GoldPowder.MoveToTime(goldPowder);
            m_BattleCoin.MoveToTime(battleCoin);
        }
    }

    private IEnumerator UpdateSlotInfo()
    {
        while (!slotsInitialized)
        {
            yield return null;
        }

        UIWaitForAction.Start(m_SlotScrollView.GetComponent<UIPanel>());

        slotsInitialized = false; // [WARNING]
        ProductionManager.Instance.GetSlotInfoList((slotInfoList) =>
        {
            slotsInitialized = true;
        });

        while (!slotsInitialized)
        {
            yield return null;
        }

        if (m_SlotList != null && m_SlotList.Count > 0)
        {
            for (int i = 0; i < m_SlotList.Count; i++)
            {
                var slot = m_SlotList[i];
                if (slot != null)
                {
                    slot.SetSlotInfo(slot.slotIndex);
                }

                Util_NGUI.Reposition(m_SlotGrid);

                yield return null;
            }
        }

        Util_NGUI.SetScrollBar(m_SlotScrollView, 0f);
        UIWaitForAction.Stop(m_SlotScrollView);
    }

    private void UpdateSlotInfo(int slotIndex)
    {
        var slot = FindSlot(slotIndex);
        if (slot != null)
        {
            slot.SetSlotInfo(slotIndex);
        }
    }

    private UI_CraftItem FindSlot(int slotIndex)
    {
        UI_CraftItem slot = null;
        if (m_SlotList != null && m_SlotList.Count > 0)
        {
            slot = m_SlotList.Find(match => match.slotIndex == slotIndex);
        }

        return slot;
    }

    public void OnProductClick(UI_Item product)
    {
        if (product != null)
        {
            var gameObject = UI_PopupManager.sOpen("UI/UI_CraftDetailInfo");
            if (gameObject != null)
            {
                var component = gameObject.GetComponent<UI_CraftDetailInfo>();
                if (component != null)
                {
                    component.SetFishingRod(product.itemTableIndex);
                }
                else Destroy(gameObject);
            }
        }
    }

    private UIGrid FindProductGrid(delta_T_ItemProduction.Data itemProductionData)
    {
        UIGrid productGrid = null;
        if (itemProductionData != null)
        {
            int index = itemProductionData.TabNomber - 1;
            if (m_ProductGridList != null && m_ProductGridList.Count > index)
            {
                productGrid = m_ProductGridList[index];
            }
        }

        return productGrid;
    }

    private IEnumerator CreateProductList()
    {
        if (m_ProductScrollView != null)
        {
            UIWaitForAction.Start(m_ProductScrollView.GetComponent<UIPanel>());
        }

        UIGrid parentGrid = null;
        foreach (var itemProductionData in TableManager.Instance.itemProduction.GetValues())
        {
            var itemData = TableManager.Instance.item.GetData(itemProductionData.uiID);
            if (itemData != null)
            {
                var product = UI_Item.CreateItem("UI/UI_Item_LargeIcon", this, "OnProductClick");
                if (product != null)
                {
                    parentGrid = FindProductGrid(itemProductionData);
                    if (parentGrid != null)
                    {
                        product.transform.SetParent(parentGrid.transform);
                        product.transform.localScale = Vector3.one;
                    }

                    product.SetItem(itemData);
                    product.itemCountVisible = true;
                }
            }

            Util_NGUI.Reposition(parentGrid);

            yield return null;
        }

        Util_NGUI.SetScrollBar(m_ProductScrollView, 0f);
        UIWaitForAction.Stop(m_ProductScrollView);
    }

    private IEnumerator CreateSlotList()
    {
        UIWaitForAction.Start(m_SlotScrollView.GetComponent<UIPanel>());

        for (int i = 1; i <= 8; i++)
        {
            var slot = CreateSlot();
            if (slot != null)
            {
                slot.slotIndex = i;
                m_SlotList.Add(slot);
            }

            Util_NGUI.Reposition(m_SlotGrid);

            yield return null;
        }

        slotsInitialized = true;
        Util_NGUI.SetScrollBar(m_SlotScrollView, 0f);
        UIWaitForAction.Stop(m_SlotScrollView);
    }

    private UI_CraftItem CreateSlot()
    {
        UI_CraftItem slot = null;
        var gameObject = ResourcesManager.Instantiate("UI/UI_Craft_Item");
        if (gameObject != null)
        {
            slot = gameObject.GetComponent<UI_CraftItem>();
            if (slot != null)
            {
                slot.transform.SetParent(m_SlotGrid.transform);
                slot.transform.localScale = Vector3.one;
            }
            else Destroy(gameObject);
        }

        return slot;
    }
}
